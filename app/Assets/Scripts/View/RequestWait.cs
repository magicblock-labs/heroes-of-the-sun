using Model;
using Service;
using Utils.Injection;

namespace View
{
    public class RequestWait : InjectableBehaviour
    {
        [Inject] private ProgramConnector _connector;
        [Inject] private SettlementModel _model;

        public async void Wait()
        {
            if (_model.HasData && _model.Get().TimeUnits > 0 && await _connector.Wait(1))
                await _connector.ReloadData();
        }
    }
}