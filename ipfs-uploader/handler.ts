import { APIGatewayProxyEventV2, APIGatewayProxyResultV2 } from 'aws-lambda';
import Busboy from 'busboy';
import { createHash } from 'crypto';
import { fetch } from 'undici';

const PINATA_UPLOAD_URL = 'https://uploads.pinata.cloud/v3/files';

// cache secret across invocations
let cachedPinataJwt: string | null = null;

async function getPinataJwt(): Promise<string> {
    if (cachedPinataJwt) return cachedPinataJwt;
    const envJwt = (process.env.PINATA_JWT || '').trim();
    if (!envJwt) throw new Error('Missing PINATA_JWT env var');
    cachedPinataJwt = envJwt.startsWith('Bearer ') ? envJwt.slice(7).trim() : envJwt;
    return cachedPinataJwt!;
}

type ParsedForm = {
    file?: { filename: string; contentType: string; data: Buffer };
    contentType?: string;
};

function parseMultipart(event: APIGatewayProxyEventV2): Promise<ParsedForm> {
    return new Promise((resolve, reject) => {
        const headers = event.headers || {};
        const contentTypeHeader = (headers['content-type'] as any) || (headers['Content-Type'] as any);
        const ctLower = (contentTypeHeader || '').toLowerCase();
        if (!ctLower.startsWith('multipart/form-data'))
            return reject(new Error('Expected multipart/form-data'));

        const busboy = Busboy({ headers: { 'content-type': contentTypeHeader || '' } });
        const result: ParsedForm = {};
        const buffers: Buffer[] = [];
        let fileMeta: { filename: string; contentType: string } | null = null;
        const maxBytes = Number(process.env.MAX_UPLOAD_BYTES || 5 * 1024 * 1024);
        let received = 0;
        let fileData: Buffer | null = null;

        busboy.on('file', (fieldname, file, info) => {
            const { filename, mimeType } = info;
            fileMeta = { filename, contentType: mimeType };
            file.on('data', (data: Buffer) => {
                received += data.length;
                if (received > maxBytes) {
                    busboy.emit('error', new Error('File exceeds max size'));
                    return;
                }
                buffers.push(data);
            });
            file.on('end', () => {
                fileData = Buffer.concat(buffers);
            });
        });

        busboy.on('field', (name, val) => {
            if (name === 'content_type') result.contentType = val;
        });

        busboy.on('error', (err) => reject(err));
        busboy.on('finish', () => {
            if (!fileData || !fileMeta) return reject(new Error('No file provided'));
            result.file = { filename: fileMeta.filename, contentType: fileMeta.contentType, data: fileData };
            resolve(result);
        });

        // body may be base64-encoded by APIG
        const body = event.body ? (event.isBase64Encoded ? Buffer.from(event.body, 'base64') : Buffer.from(event.body)) : Buffer.alloc(0);
        busboy.end(body);
    });
}

function json(statusCode: number, body: unknown): APIGatewayProxyResultV2 {
    return {
        statusCode,
        headers: {
            'content-type': 'application/json',
            'access-control-allow-origin': process.env.CORS_ORIGIN || '*', // tighten in prod
            'access-control-allow-headers': 'authorization,content-type',
            'access-control-allow-methods': 'OPTIONS,POST'
        },
        body: JSON.stringify(body)
    };
}

function validateMime(form: ParsedForm) {
    const ct = (form.contentType || form.file?.contentType || '').toLowerCase();
    const ok = ct === 'image/png' || ct === 'application/json';
    if (!ok) throw { code: 'VALIDATION_FAILED', message: 'content_type must be one of [image/png, application/json]', status: 400 };
    return ct;
}

export const handler = async (event: APIGatewayProxyEventV2): Promise<APIGatewayProxyResultV2> => {
    try {
        const method = (event as any)?.requestContext?.http?.method || (event as any)?.httpMethod || 'POST';
        if (method === 'OPTIONS') {
            return json(200, { status: 'ok' });
        }

        const form = await parseMultipart(event);
        const contentType = validateMime(form);
        if (!form.file) throw { code: 'VALIDATION_FAILED', message: 'file is required', status: 400 };

        // SHA-256 for audit (no PII)
        const sha256 = createHash('sha256').update(form.file.data).digest('hex');

        // Build multipart to forward to Pinata v3
        // We’ll craft the multipart body manually to keep deps light.
        const boundary = '----LambdaFormBoundary' + Math.random().toString(16).slice(2);
        const CRLF = '\r\n';
        const parts: Buffer[] = [];

        function pushTextPart(name: string, value: string) {
            parts.push(Buffer.from(`--${boundary}${CRLF}`));
            parts.push(Buffer.from(`Content-Disposition: form-data; name="${name}"${CRLF}${CRLF}`));
            parts.push(Buffer.from(value + CRLF));
        }
        function pushFilePart(name: string, filename: string, mime: string, data: Buffer) {
            parts.push(Buffer.from(`--${boundary}${CRLF}`));
            parts.push(Buffer.from(`Content-Disposition: form-data; name="${name}"; filename="${filename}"${CRLF}`));
            parts.push(Buffer.from(`Content-Type: ${mime}${CRLF}${CRLF}`));
            parts.push(data);
            parts.push(Buffer.from(CRLF));
        }

        pushFilePart('file', form.file.filename || (contentType === 'image/png' ? 'image.png' : 'metadata.json'), contentType, form.file.data);
        pushTextPart('network', 'public');
        parts.push(Buffer.from(`--${boundary}--${CRLF}`));

        const pinataJwt = await getPinataJwt();
        const res = await fetch(PINATA_UPLOAD_URL, {
            method: 'POST',
            headers: {
                'authorization': `Bearer ${pinataJwt}`,
                'content-type': `multipart/form-data; boundary=${boundary}`
            },
            body: Buffer.concat(parts)
        });

        const text = await res.text();
        if (!res.ok) {
            console.error('Pinata error', res.status, text);
            return json(502, { status: 'error', code: 'PINATA_ERROR', detail: `Pinata upload failed (${res.status})` });
        }

        // Expected shape: { data: { cid: "<cid>" } }
        let cid: string | undefined;
        try {
            const parsed = JSON.parse(text);
            cid = parsed?.data?.cid;
        } catch {
            console.error('Pinata non-JSON response:', text);
        }
        if (!cid) {
            console.error('Pinata response missing CID:', text);
            return json(502, { status: 'error', code: 'PINATA_ERROR', detail: 'Pinata response missing CID' });
        }

        const ipfs_url = `ipfs://${cid}`;
        const gateway_url = `https://gateway.pinata.cloud/ipfs/${cid}`;

        // Minimal audit log
        console.log(JSON.stringify({
            event: 'upload_ok',
            bytes: form.file.data.length,
            sha256,
            mime: contentType,
            cid
        }));

        return json(200, {
            status: 'ok',
            cid,
            ipfs_url,
            gateway_url
        });

    } catch (err: any) {
        const status = err?.status || 500;
        const code = err?.code || 'INTERNAL_ERROR';
        const detail = err?.message || 'Unexpected error';
        console.error('upload_error', { code, detail, stack: err?.stack });
        return json(status, { status: 'error', code, detail });
    }
};