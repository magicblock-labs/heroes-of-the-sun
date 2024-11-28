using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Model;
using Solana.Unity.Wallet;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    public class PlayerSettlementConnector : SettlementConnector
    {
        [Inject] private TokenConnector _token;

        public async Task<bool> PlaceBuilding(byte x, byte y, byte type, int worker_index)
        {
            return await ApplySystem(new PublicKey("AoKVKur4mczZtuzeMQwydkMe6ZSrJGxTWqZU6grPnd9c"),
                new { x, y, config_index = type, worker_index });
        }

        public async Task<bool> Wait(int time)
        {
            return await ApplySystem(new PublicKey("5LiZ8jP6fqAWT5V6B3C13H9VCwiQoqdyPwUYzWDfMUSy"),
                new { time }, null, false, _token.GetMintExtraAccounts());
        }

        public async Task<bool> AssignWorker(int worker_index, int building_index)
        {
            return await ApplySystem(new PublicKey("BExuAEwcKxKeqHSN8C1WetUAd6Tm71cZEiP8EBSrH55T"),
                new { worker_index, building_index });
        }


        public async Task<bool> Repair(int index)
        {
            return await ApplySystem(new PublicKey("4MA6KhwEUsLbZJqJK9rqwVjdZgdxy7vbebuD2MeLKm5j"), new { index });
        }

        public async Task<bool> Upgrade(int index, int worker_index)
        {
            return await ApplySystem(new PublicKey("J3evfUppPdgjTzWhhAhuhKBVM23UU8iCU9j9r7sTHCTB"),
                new { index, worker_index });
        }

        public async Task<bool> ClaimTime()
        {
            return await ApplySystem(new PublicKey("HFx2weMbr8CrAEAPfPtgw9zzgHgUFzSz7qiTyhTHGSF"), new { });
        }

        public async Task<bool> Research(int research_type)
        {
            return await ApplySystem(new PublicKey("GnVJxqk8dExpXhVidSEFNQcjTY1sCAYWcwM1GGVKKVHb"),
                new { research_type }, null, false, _token.GetBurnExtraAccounts());
        }

        public async Task<bool> Sacrifice(int index)
        {
            return await ApplySystem(new PublicKey("4Cvjz6qrVakbSg3dqBMA8vv8XL8KD3UCTbRVM8g8WkoW"),
                new { index });
        }

        public async Task<bool> Reset()
        {
            return await ApplySystem(new PublicKey("J2HTjpKDf317Q7Pg9kUVFDregE2Ld34P61M5m4XnVSh2"),
                new { });
        }
    }
}