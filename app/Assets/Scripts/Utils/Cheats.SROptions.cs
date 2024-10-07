using Service;
using Utils.Injection;

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        [Inject] private ProgramConnector _connector;

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