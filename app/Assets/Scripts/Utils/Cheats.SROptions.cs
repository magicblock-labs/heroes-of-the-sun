using Connectors;
using Model;
using Utils.Injection;
using Utils;

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private SettlementModel _model;

        public SROptions()
        {
            Injector.Instance.Resolve(this);
        }
        
        public async void Airdrop()
        {
            await Web3Utils.Airdrop();
        }
        
        public async void Reset()
        {
            if (await _connector.Reset())
                _model.Set(await _connector.LoadData());
        }
    }
}