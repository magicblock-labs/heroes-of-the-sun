using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using TokenMinter;
using TokenMinter.Program;
using TokenMinter.Errors;
using TokenMinter.Accounts;
using TokenMinter.Types;

namespace TokenMinter
{
    namespace Accounts
    {
        public partial class SessionToken
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 1081168673100727529UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{233, 4, 115, 14, 46, 21, 1, 15};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "fyZWTdUu1pS";
            public PublicKey Authority { get; set; }

            public PublicKey TargetProgram { get; set; }

            public PublicKey SessionSigner { get; set; }

            public long ValidUntil { get; set; }

            public static SessionToken Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                SessionToken result = new SessionToken();
                result.Authority = _data.GetPubKey(offset);
                offset += 32;
                result.TargetProgram = _data.GetPubKey(offset);
                offset += 32;
                result.SessionSigner = _data.GetPubKey(offset);
                offset += 32;
                result.ValidUntil = _data.GetS64(offset);
                offset += 8;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum TokenMinterErrorKind : uint
        {
        }
    }

    namespace Types
    {
    }

    public partial class TokenMinterClient : TransactionalBaseClient<TokenMinterErrorKind>
    {
        public TokenMinterClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId = null) : base(rpcClient, streamingRpcClient, programId ?? new PublicKey(TokenMinterProgram.ID))
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>> GetSessionTokensAsync(string programAddress = TokenMinterProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = SessionToken.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>(res);
            List<SessionToken> resultingAccounts = new List<SessionToken>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => SessionToken.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>> GetSessionTokenAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>(res);
            var resultingAccount = SessionToken.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeSessionTokenAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, SessionToken> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                SessionToken parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = SessionToken.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<TokenMinterErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<TokenMinterErrorKind>>{};
        }
    }

    namespace Program
    {
        public class BurnTokenAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey AssociatedTokenAccount { get; set; }

            public PublicKey MintAccount { get; set; }

            public PublicKey TokenProgram { get; set; } = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
            public PublicKey AssociatedTokenProgram { get; set; } = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
        }

        public class CreateTokenAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey MintAccount { get; set; }

            public PublicKey MetadataAccount { get; set; }

            public PublicKey TokenProgram { get; set; } = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
            public PublicKey TokenMetadataProgram { get; set; } = new PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
            public PublicKey Rent { get; set; } = new PublicKey("SysvarRent111111111111111111111111111111111");
        }

        public class MintTokenAccounts
        {
            public PublicKey SessionToken { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey MintAccount { get; set; }

            public PublicKey AssociatedTokenAccount { get; set; }

            public PublicKey TokenProgram { get; set; } = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
            public PublicKey AssociatedTokenProgram { get; set; } = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public static class TokenMinterProgram
        {
            public const string ID = "4ZxRnucEWC62kVktmx27cz9d1PzWWNgiZLT5VWFLbfB2";
            public static Solana.Unity.Rpc.Models.TransactionInstruction BurnToken(BurnTokenAccounts accounts, ulong amount, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.AssociatedTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MintAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5351999914653558201UL, offset);
                offset += 8;
                _data.WriteU64(amount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateToken(CreateTokenAccounts accounts, string token_name, string token_symbol, string token_uri, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MintAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MetadataAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5470338735940580436UL, offset);
                offset += 8;
                offset += _data.WriteBorshString(token_name, offset);
                offset += _data.WriteBorshString(token_symbol, offset);
                offset += _data.WriteBorshString(token_uri, offset);
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction MintToken(MintTokenAccounts accounts, ulong amount, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SessionToken == null ? programId : accounts.SessionToken, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MintAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.AssociatedTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4101212246258452908UL, offset);
                offset += 8;
                _data.WriteU64(amount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}