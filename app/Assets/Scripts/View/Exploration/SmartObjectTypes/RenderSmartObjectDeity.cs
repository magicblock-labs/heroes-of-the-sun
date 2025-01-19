using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connectors;
using Hero.Program;
using Model;
using Notifications;
using Smartobjectdeity.Accounts;
using Solana.Unity.Wallet;
using Utils;
using Utils.Injection;

namespace View.Exploration.SmartObjectTypes
{
    public class RenderSmartObjectDeity : InjectableBehaviour
    {
        [Inject] private PlayerConnector _playerConnector;
        [Inject] private SmartObjectDeityConnector _connector;
        [Inject] private RequestInteractionWithSmartObject _interact;
        [Inject] private DialogInteractionStateModel _dialogInteractionState;
        [Inject] private InteractionStateModel _interactionState;
        [Inject] private TokenConnector _token;

        private SmartObjectDeity _data;

        private void Start()
        {
            _interact.Add(OnInteractionRequest);
            _dialogInteractionState.AnswerSubmitted.Add(OnAnswerSubmitted);

            _=_connector.Initialize();
        }

        private void OnAnswerSubmitted(int value)
        {
            if (_dialogInteractionState.GetCurrentInvoker() == new PublicKey(_connector.EntityPda))
                _ = _connector.Interact(value, new("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB"), _token.GetMintExtraAccounts());
        }

        private void OnInteractionRequest(PublicKey value)
        {
            if (_connector.EntityPda != value) return;
            if (_data.NextInteractionTime > Web3Utils.GetNodeTime()) return;

            _dialogInteractionState.SetConversationIndex(0, value);
            _interactionState.SetState(InteractionState.Dialog);
        }

        public async Task SetEntity(string value)
        {
            await _connector.SetEntityPda(value, false);
            var smartObjectDeity = await _connector.LoadData();

            if (smartObjectDeity == null)
            {
                Destroy(this);
                return;
            }

            OnDataUpdate(smartObjectDeity);
            await _connector.Subscribe(OnDataUpdate);
        }

        private void OnDataUpdate(SmartObjectDeity value)
        {
            _data = value;
            
            if (_dialogInteractionState.GetCurrentInvoker() == new PublicKey(_connector.EntityPda))
            {
                _dialogInteractionState.SetConversationIndex(_data.NextInteractionTime > Web3Utils.GetNodeTime()
                    ? -1
                    : 0);
            }
        }

        private void OnDestroy()
        {
            _connector.Unsubscribe();
            _interact.Remove(OnInteractionRequest);
            _dialogInteractionState.AnswerSubmitted.Remove(OnAnswerSubmitted);
        }
    }
}