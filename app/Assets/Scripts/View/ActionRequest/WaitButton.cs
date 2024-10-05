using Model;
using Service;
using Utils.Injection;

namespace View.ActionRequest
{
    public class WaitButton : InjectableBehaviour
    {
        [Inject] private ProgramConnector _connector;
        [Inject] private SettlementModel _model;
        [Inject] private InteractionStateModel _interaction;

        public async void Wait()
        {
            _interaction.OnActionRequested();
            
            if (_model.HasData && _model.Get().TimeUnits > 0 && await _connector.Wait(1))
                await _connector.ReloadData();
        }
    }
}