using System;
using Solana.Unity.Programs.Models.TokenProgram;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connectors;
using Model;
using Newtonsoft.Json;
using Notifications;
using Smartobjecttokenlauncher.Accounts;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;
using View.UI.World;

namespace View.Exploration.SmartObjectTypes
{
    public class RenderSmartObjectTokenLauncher : InjectableBehaviour, ISmartObject
    {
        [Inject] private SmartObjectTokenLauncherConnector _connector;
        [Inject] private TokenConnector _token;
        [Inject] private RequestInteractionWithSmartObject _interact;
        [Inject] private HideInteractionWithSmartObject _interactHide;
        [Inject] private PlayerHeroModel _playerHero;
        [Inject] private HeroConnector _heroConnector;

        [SerializeField] private TokenInfo tokenInfo;

        [SerializeField] private GameObject purchaseControl;
        [SerializeField] private Text quantityText;

        private SmartObjectTokenLauncher _data;

        private void Start()
        {
            _interact.Add(OnInteractionRequest);
            _interactHide.Add(OnInteractionHide);
        }

        private void OnInteractionRequest(PublicKey value)
        {
            if (_connector.EntityPda != value) return;

            purchaseControl.SetActive(true);
        }

        private void OnInteractionHide()
        {
            purchaseControl.SetActive(false);
        }

        public void OnPurchaseRequest()
        {
            if (int.TryParse(quantityText.text, out int quantity) && quantity > 0)
                _ = Purchase(quantity);
        }

        private async Task Purchase(int quantity)
        {
            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false))
                .SetFeePayer(Web3.Account)
                .AddInstruction(
                    AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        Web3.Account,
                        Web3.Account,
                        _data.Mint));

            var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account }));
            await Web3.Wallet.SignAndSendTransaction(tx, true);

            await _connector.Interact(quantity, _data.Mint);
        }

        public async Task SetEntity(string value)
        {
            tokenInfo.gameObject.SetActive(false);

            await _connector.SetEntityPda(value, false);
            await _connector.Subscribe(OnDataUpdate);
            OnDataUpdate(await _connector.LoadData());
        }

        public async Task UpdateData()
        {
            OnDataUpdate(await _connector.LoadData());
        }

        private void OnDataUpdate(SmartObjectTokenLauncher value)
        {
            _data = value;
            _ = Redraw();
        }

        private async Task Redraw()
        {
            try
            {
                if (_data == null)
                    return;
                
                if (_data.Mint == PublicKey.DefaultPublicKey)//invalid token
                {
                    if (gameObject)
                        Destroy(gameObject);
                    return;
                }
                
                Debug.Log("[TokenLauncher] mint address: " + _data.Mint);
                var metadata = await _token.LoadMetadata(_data.Mint);
                
                Debug.Log($"[TokenLauncher] metadata [{_data.Mint}] : {JsonConvert.SerializeObject(metadata)}");
                if (metadata == null)
                {
                    if (gameObject)
                        Destroy(gameObject);
                    return;
                }
                
                var mintAccount = await Web3.Rpc.GetAccountInfoAsync(_data.Mint, Commitment.Processed);
                var mintData = mintAccount.Result?.Value?.Data[0];

                if (mintData == null){
                    if (gameObject)
                        Destroy(gameObject);
                    return;
                }

                var mintBytes = Convert.FromBase64String(mintData);
                var mint = TokenMint.Deserialize(mintBytes);
                Debug.Log("[TokenLauncher] mint data: " + JsonConvert.SerializeObject(mint));
                
                var supply = mint.Supply;
                var price = TokenConnector.CalculateTokenPrice(supply);
                Debug.Log($"[TokenLauncher] Current token price (bonding curve): {price}");
                
                PlayerPrefs.SetString($"{RenderSmartObject.CachePrefix}:{_connector.EntityPda}",
                    _connector.GetComponentName());

                tokenInfo.gameObject.SetActive(true);
                tokenInfo.SetData(metadata, _data.Recipe, price);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            _interact.Remove(OnInteractionRequest);
            _interactHide.Remove(OnInteractionHide);
            _ = _connector.Unsubscribe();
        }
    }
}