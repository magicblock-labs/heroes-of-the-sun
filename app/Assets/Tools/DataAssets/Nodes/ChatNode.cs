using Tools.DataAssets.xNode_1._8._0.Scripts;
using UnityEngine;

namespace Tools.DataAssets.Nodes
{
    public class ChatNode : Node, IDialogueNode
    {
        [TextArea] public string prompt;

        [SerializeField] [Input] private string @in;

        [SerializeField] [Output(dynamicPortList = true)] [TextArea]
        public string[] answer;

        public void SubmitAnswer(int index)
        {
            if (answer.Length <= index) return;

            var portName = "answer " + index;
            Debug.Log(portName);
            var port = GetOutputPort(portName);

            if (port == null) return;

            ((DialogueGraph)graph).current = null;
            Debug.Log(((DialogueGraph)graph).current);
            Debug.Log(port.ConnectionCount);
            for (var i = 0; i < port.ConnectionCount; i++)
            {
                var connection = port.GetConnection(i);
                if (connection?.node is IDialogueNode node)
                    node.Trigger();
            }
        }

        public void Trigger()
        {
            ((DialogueGraph)graph).current = this;
            Debug.Log(((DialogueGraph)graph).current);
        }
    }

    public interface IDialogueNode
    {
        void Trigger();
    }
}