using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Connectors;
using Model;
using Notifications;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils.Injection;
using Random = UnityEngine.Random;
using Transaction = Solana.Unity.Rpc.Models.Transaction;

namespace View.UI
{
    public class ManageTokenCreation : InjectableBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private InputField nameInput;
        [SerializeField] private InputField symbolInput;
        [SerializeField] private InputField descriptionInput;

        [SerializeField] private Text recipeFood;
        [SerializeField] private Text recipeWater;
        [SerializeField] private Text recipeWood;
        [SerializeField] private Text recipeStone;

        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private GameObject progressContainer;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Image progressBar;

        [Inject] private TokenConnector _token;

        [Inject] private SmartObjectLocationConnector _loc;
        [Inject] private PlayerHeroModel _hero;
        [Inject] private SmartObjectTokenLauncherConnector _tokenLauncher;

        [Inject] private RedrawSmartObjects _redrawSmartObjects;
        private ushort _recipeFoodValue;
        private ushort _recipeWaterValue;
        private ushort _recipeWoodValue;
        private ushort _recipeStoneValue;

        enum TokenCreationSteps
        {
            ImageUpload,
            MetadataUpload,
            MintCreation,
            SmartObjectCreation,
            TokenLauncherInitialisation
        }


#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string accept);
#endif

        // Pinata V3 Upload API + JWT
        private const string PinataJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySW5mb3JtYXRpb24iOnsiaWQiOiJmYTVjZGNjNS1hZTYxLTQzMjMtYmZmYi04MGI3MTllYzk0MmQiLCJlbWFpbCI6Im4uYmFseWFuaXRzYUBnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwicGluX3BvbGljeSI6eyJyZWdpb25zIjpbeyJkZXNpcmVkUmVwbGljYXRpb25Db3VudCI6MSwiaWQiOiJGUkExIn1dLCJ2ZXJzaW9uIjoxfSwibWZhX2VuYWJsZWQiOmZhbHNlLCJzdGF0dXMiOiJBQ1RJVkUifSwiYXV0aGVudGljYXRpb25UeXBlIjoic2NvcGVkS2V5Iiwic2NvcGVkS2V5S2V5IjoiMTVmNzEzZGJkOTk5YjZkZDZmMTIiLCJzY29wZWRLZXlTZWNyZXQiOiIyODQwMzVkZWJlZjNiZmVkZmRiNGVmZmZiMDU5ODg4NzFmZjczNGJhY2RjYzU3MjRhODVjYjZjNzBhYTkzMmM1IiwiZXhwIjoxNzc5MTQwNjU2fQ.NtPtNGnlL72X_2GCFn-T-PkAZr4moD34fCVZmrssFOU";

        private const string PinataV3UploadUrl = "https://uploads.pinata.cloud/v3/files";


        private void OnEnable()
        {
            submitButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            progressContainer.SetActive(false);

            PickRandomTokenIcon();

            nameInput.text = string.Empty;
            symbolInput.text = string.Empty;
            descriptionInput.text = string.Empty;

            recipeFood.text = "0";
            recipeWater.text = "0";
            recipeWood.text = "0";
            recipeStone.text = "0";

            OnRecipeChange();
        }

        public void OpenImage()
        {
#if false// UNITY_EDITOR

            var path = EditorUtility.OpenFilePanel("Select PNG Image", "", "png");

            if (string.IsNullOrEmpty(path)) return;

            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2); // Size will be overridden
            if (texture.LoadImage(fileData))
            {
                // Create sprite
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                icon.sprite = sprite;
                Debug.Log("Sprite loaded from: " + path);
            }
            else
            {
                Debug.LogError("Failed to load texture from PNG.");
            }

#elif UNITY_WEBGL
            UploadFile(gameObject.name, nameof(OnFileUploaded), ".png");
#else
            PickRandomTokenIcon();

#endif
        }

        private void PickRandomTokenIcon()
        {
            var sprites = Resources.LoadAll<Sprite>("TokenIcons");
            icon.sprite = sprites[Random.Range(0, sprites.Length)];
        }

        // Called from JS with PNG bytes
        public void OnFileUploaded(string payload)
        {
            var split = payload.Split('|');
            if (split.Length != 2)
            {
                Debug.LogError("Invalid payload from JS");
                return;
            }

            string fileName = split[0];
            string base64Data = split[1];
            byte[] fileData = Convert.FromBase64String(base64Data);

            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                icon.sprite = sprite;
                Debug.Log("Loaded sprite from: " + fileName);
            }
            else
            {
                Debug.LogError("Texture load failed.");
            }
        }


        public void OnRecipeChange()
        {
            if (!ushort.TryParse(recipeFood.text, out _recipeFoodValue) ||
                !ushort.TryParse(recipeWater.text, out _recipeWaterValue) ||
                !ushort.TryParse(recipeWood.text, out _recipeWoodValue) ||
                !ushort.TryParse(recipeStone.text, out _recipeStoneValue))
            {
                submitButton.interactable = false;
                return;
            }

            // if (_recipeFoodValue > _hero.Get().Backpack.Food)
            // {
            //     _recipeFoodValue = _hero.Get().Backpack.Food;
            //     recipeFood.text = _recipeFoodValue.ToString();
            // }
            //
            // if (_recipeWaterValue > _hero.Get().Backpack.Water)
            // {
            //     _recipeWaterValue = _hero.Get().Backpack.Water;
            //     recipeWater.text = _recipeWaterValue.ToString();
            // }
            //
            // if (_recipeWoodValue > _hero.Get().Backpack.Wood)
            // {
            //     _recipeWoodValue = _hero.Get().Backpack.Wood;
            //     recipeWood.text = _recipeWoodValue.ToString();
            // }
            //
            // if (_recipeStoneValue > _hero.Get().Backpack.Stone)
            // {
            //     _recipeStoneValue = _hero.Get().Backpack.Stone;
            //     recipeStone.text = _recipeStoneValue.ToString();
            // }

            if (_recipeFoodValue + _recipeWaterValue + _recipeWoodValue + _recipeStoneValue == 0)
            {
                submitButton.interactable = false;
                return;
            }

            submitButton.interactable = true;
        }

        public void OnSubmit()
        {
            submitButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(false);
            progressContainer.SetActive(true);

            StartCoroutine(UploadImageAndMetadataV3());
        }

        private IEnumerator UploadImageAndMetadataV3()
        {
            SetProgressStep(TokenCreationSteps.ImageUpload);

            var texture = icon.sprite.texture;
            var pngData = texture.EncodeToPNG();
            string imageIpfsUrl = null;
            yield return UploadFileToPinataV3(pngData, "image.png", "image/png", cid => imageIpfsUrl = cid);

            var metadata = new SolanaMetadata
            {
                name = nameInput.text,
                symbol = symbolInput.text,
                description = descriptionInput.text,
                image = imageIpfsUrl,
                properties = new MetadataProperties
                {
                    category = "image",
                    creators = new[]
                    {
                        new Creator { address = Web3.Wallet.Account.PublicKey, share = 100 }
                    }
                }
            };

            SetProgressStep(TokenCreationSteps.MetadataUpload);

            var metadataJson = JsonUtility.ToJson(metadata, true);

            var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            string metadataIpfsUrl = null;
            yield return UploadFileToPinataV3(metadataBytes, "metadata.json", "application/json",
                cid =>
                {
                    metadataIpfsUrl = cid;

                    var isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
                    var cacheDir = Path.Combine(Application.persistentDataPath, "cache");
                    var jsonPath = Path.Combine(cacheDir, $"{cid}.meta.json");
                    if (isWebGL) return;
                    try
                    {
                        File.WriteAllText(jsonPath, metadataJson);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to cache metadata: " + e.Message);
                    }
                });

            _ = CreateTokenAndSmartObject(metadata.name, metadata.symbol, metadataIpfsUrl);
        }

        private void SetProgressStep(TokenCreationSteps value)
        {
            progressLabel.text = value switch
            {
                TokenCreationSteps.ImageUpload => "Uploading image to IPFS..",
                TokenCreationSteps.MetadataUpload => "Uploading Metadata to IPFS..",
                TokenCreationSteps.MintCreation => "Creating Mint Account",
                TokenCreationSteps.SmartObjectCreation => "Creating Smart Object..",
                TokenCreationSteps.TokenLauncherInitialisation => "Initialising Token Launcher..",
                _ => "..."
            };

            progressBar.fillAmount = (float)value / Enum.GetValues(typeof(TokenCreationSteps)).Length;
        }

        private async Task CreateTokenAndSmartObject(string metadataName, string metadataSymbol, string metadataIpfsUrl)
        {
            SetProgressStep(TokenCreationSteps.MintCreation);

            var mint = new Account();

            var minimumRent = await Web3.Rpc.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);

            var pda = Pda.GetMintAuthorityPDA(mint, new PublicKey("AdrPpoYr67ZcDZsQxsPgeosE3sQbZxercbUn8i1dcvap"));

            var transaction = new TransactionBuilder()
                    .SetRecentBlockHash(await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false))
                    .SetFeePayer(Web3.Account)
                    .AddInstruction(
                        SystemProgram.CreateAccount(
                            Web3.Account,
                            mint.PublicKey,
                            minimumRent.Result,
                            TokenProgram.MintAccountDataSize,
                            TokenProgram.ProgramIdKey))
                    .AddInstruction(
                        TokenProgram.InitializeMint(
                            mint.PublicKey,
                            0,
                            pda,
                            pda))
                ;

            var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account, mint }));
            var res = await Web3.Wallet.SignAndSendTransaction(tx);
            Debug.Log(res.Result);

            try
            {
                SetProgressStep(TokenCreationSteps.SmartObjectCreation);
                await _loc.SetSeed($"TL@{_hero.Get().X}x{_hero.Get().Y}");
                await _loc.Init(_hero.Get().X, _hero.Get().Y);

                SetProgressStep(TokenCreationSteps.TokenLauncherInitialisation);

                await _tokenLauncher.SetEntityPda(_loc.EntityPda, true, true);

                await _tokenLauncher.Init(metadataName, metadataSymbol, metadataIpfsUrl, mint, _recipeFoodValue,
                    _recipeWaterValue, _recipeWoodValue, _recipeStoneValue);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            gameObject.SetActive(false);

            _redrawSmartObjects.Dispatch();
        }

        private IEnumerator UploadFileToPinataV3(byte[] fileData, string fileName, string contentType,
            System.Action<string> onSuccess)
        {
            var boundary = "----UnityFormBoundary" + System.Guid.NewGuid().ToString("N");
            var body = new System.Collections.Generic.List<byte>();

            void Append(string str) => body.AddRange(Encoding.UTF8.GetBytes(str));
            void AppendBytes(byte[] bytes) => body.AddRange(bytes);

            Append($"--{boundary}\r\n");
            Append($"Content-Disposition: form-data; name=\"file\"; filename=\"{fileName}\"\r\n");
            Append($"Content-Type: {contentType}\r\n\r\n");
            AppendBytes(fileData);
            Append($"\r\n");

            Append($"--{boundary}\r\n");
            Append($"Content-Disposition: form-data; name=\"network\"\r\n\r\n");
            Append("public\r\n");
            Append($"--{boundary}--\r\n");

            var request = new UnityWebRequest(PinataV3UploadUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(body.ToArray());
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {PinataJwt}");
            request.SetRequestHeader("Content-Type", $"multipart/form-data; boundary={boundary}");

            yield return request.SendWebRequest();

            Debug.Log("Status Code: " + request.responseCode);
            Debug.Log("Response: " + request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Upload failed (v3): {request.error}");
                yield break;
            }

            var cid = ExtractPinataCid(request.downloadHandler.text);
            var ipfsUrl = $"https://gateway.pinata.cloud/ipfs/{cid}";
            Debug.Log($"✅ Uploaded to IPFS (v3): {ipfsUrl}");

            onSuccess?.Invoke(ipfsUrl);
        }

        private string ExtractPinataCid(string json)
        {
            var result = JsonUtility.FromJson<PinataResponse>(json);
            return result.data.cid;
        }

        [System.Serializable]
        private class PinataResponseData
        {
            public string cid;
        }

        [System.Serializable]
        private class PinataResponse
        {
            public PinataResponseData data;
        }

        [System.Serializable]
        public class SolanaMetadata
        {
            public string name;
            public string symbol;
            public string description;
            public string image;
            public MetadataProperties properties;
        }

        [Serializable]
        public class MetadataProperties
        {
            public string category;
            public Creator[] creators;
        }

        [Serializable]
        public class Creator
        {
            public string address;
            public int share;
        }
    }
}