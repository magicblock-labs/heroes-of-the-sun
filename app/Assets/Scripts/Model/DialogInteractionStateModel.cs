using Connectors;
using Solana.Unity.Wallet;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class DialogInteractionStateModel
    {
        public Signal<int> AnswerSubmitted = new();
        public Signal ChatUpdated = new();

        private PublicKey _invoker;
        private ChatNode _chatNode;

        public PublicKey GetCurrentInvoker()
        {
            return _invoker;
        }

        public void SubmitAnswer(int index, PublicKey invoker = null)
        {
            if (invoker != null)
                _invoker = invoker;
            AnswerSubmitted.Dispatch(index);
        }
        
        public void SetCurrentChat(ChatNode value)
        {
            _chatNode = value;
            ChatUpdated.Dispatch();
        }
        
        public ChatNode GetCurrentChat()
        {
            return _chatNode;
        }
    }
}