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

        public async Task<bool> Build(byte x, byte y, byte type, int worker_index)
        {
            return await ApplySystem(new PublicKey("fkiWK1Wn6ouGcHb3icX4XGKynef5MpsTQ478ZMdgB1g"),
                new { x, y, config_index = type, worker_index });
        }

        public async Task<bool> Wait(int time)
        {
            return await ApplySystem(new PublicKey("9F6qiZPUWN3bCnr5uVBwSmEDf8QcAFHNSVDH8L7AkZe4"),
                new { time }, null, false, _token.GetMintExtraAccounts());
        }

        public async Task<bool> AssignWorker(int worker_index, int building_index)
        {
            return await ApplySystem(new PublicKey("BExuAEwcKxKeqHSN8C1WetUAd6Tm71cZEiP8EBSrH55T"),
                new { worker_index, building_index });
        }


        public async Task<bool> Repair(int index)
        {
            return await ApplySystem(new PublicKey("5xPJt6GDcmGphNAs6qU3hWAvzLwXuSqhTco6RtoAR9aY"), new { index });
        }

        public async Task<bool> Upgrade(int index, int worker_index)
        {
            return await ApplySystem(new PublicKey("76wsz7SjNtvoFK8aUvojEyfjep5pMSaHQihGVxcjc1EA"),
                new { index, worker_index });
        }

        public async Task<bool> ClaimTime()
        {
            return await ApplySystem(new PublicKey("4XXA1mX5aN4Fd62FBgNxCU7FzKDYS3KSxFX3RdJYoWPj"), new { });
        }

        public async Task<bool> Research(int research_type)
        {
            return await ApplySystem(new PublicKey("3ZJ7mgXYhqQf7EsM8q5Ea5YJWA712TFyWGvrj9mRL2gP"),
                new { research_type }, null, false, _token.GetBurnExtraAccounts());
        }

        public async Task<bool> Sacrifice(int index)
        {
            return await ApplySystem(new PublicKey("6JwZJNAtkciXVGenFSoa99VBNcxyb2W8mvzcMK1vTWKs"),
                new { index });
        }

        public async Task<bool> Reset()
        {
            return await ApplySystem(new PublicKey("3VEXJoAZkYxDXigSWso8FnJY8z6C6inpPxU798vqc9um"),
                new { });
        }
    }
}