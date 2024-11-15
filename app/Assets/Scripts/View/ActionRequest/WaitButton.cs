using Connectors;
using Model;
using Utils.Injection;

namespace View.ActionRequest
{
    public class WaitButton : InjectableBehaviour
    {
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private SettlementModel _model;
        [Inject] private InteractionStateModel _interaction;

        public async void Wait()
        {
            _interaction.LockInteraction();
            
            if (_model.HasData && _model.Get().TimeUnits > 0 )
                await _connector.Wait(1);
        }
    }
}