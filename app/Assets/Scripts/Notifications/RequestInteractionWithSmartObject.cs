using Solana.Unity.Wallet;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    [Singleton]
    public class RequestInteractionWithSmartObject : Signal<PublicKey>
    {
    }
}