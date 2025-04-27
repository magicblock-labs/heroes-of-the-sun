using Connectors;
using Model;
using Utils.Injection;

namespace View.ActionRequest
{
    public class WaitButton : InjectableBehaviour
    {
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private SettlementModel _model;
        [Inject] private GridInteractionStateModel _gridInteraction;

        public async void Wait()
        {
            _gridInteraction.LockInteraction();

            if (_model.HasData && _model.Get().TimeUnits > 0)
                await _connector.Wait(1);
        }
    }
}