using Connectors;
using Utils.Injection;

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        [Inject] private SettlementConnector _connector;

        public SROptions()
        {
            Injector.Instance.Resolve(this);
        }
        
        public async void Airdrop()
        {
            await _connector.Airdrop();
        }
        
        public async void Reset()
        {
            if (await _connector.Reset())
                await _connector.ReloadData();
        }
    }
}