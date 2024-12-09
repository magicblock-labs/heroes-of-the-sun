using System;
using Solana.Unity.Wallet;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Serializable]
    public class ChatNode
    {
        public string prompt;
        public string[] answers;
    }


    [Singleton]
    public class DialogInteractionStateModel
    {
        private ChatNode[] _nodes =
        {
            new()
            {
                prompt = "You approached a tall totem..",
                answers = new[]
                {
                    "Look around",
                }
            },
            new()
            {
                prompt = "Would you like to interact with this totem?",
                answers = new[]
                {
                    "Hm, shake it a bit?",
                    "Yes please!",
                    "Nevermind",
                }
            },
            new()
            {
                prompt = "Nothing happens...",
                answers = new[]
                {
                    "Walk away.",
                }
            },
            new()
            {
                prompt = "Ok, give me one unit of food!",
                answers = new[]
                {
                    "Ignore and walk away",
                    "Sure, here you go!",
                }
            },
        };

        public Signal CurrentChatIndexUpdated = new();
        public Signal<int> AnswerSubmitted = new();

        private int _index = -1;
        private PublicKey _invoker;

        public void SetConversationIndex(int value, PublicKey invoker = null)
        {
            if (invoker != null)
                _invoker = invoker;
            
            _index = value;
            CurrentChatIndexUpdated.Dispatch();
        }

        public PublicKey GetCurrentInvoker()
        {
            return _invoker;
        }

        public ChatNode GetCurrentChatNode()
        {
            if (_index >= 0 && _index < _nodes.Length)
                return _nodes[_index];
            return null;
        }

        public void SubmitAnswer(int index)
        {
            AnswerSubmitted.Dispatch(index);
        }
    }
}