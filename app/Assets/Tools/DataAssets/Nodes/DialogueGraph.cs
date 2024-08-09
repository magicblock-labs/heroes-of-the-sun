using Tools.DataAssets.xNode_1._8._0.Scripts;
using UnityEngine;

namespace Tools.DataAssets.Nodes
{
    [CreateAssetMenu(menuName = "Game/DialogueGraph", order = 0)]
    public class DialogueGraph : NodeGraph
    {
        [HideInInspector] public ChatNode current;

        public ChatNode SubmitAnswer(int i)
        {
            Debug.Log(i);

            current.SubmitAnswer(i);
            return current;
        }

        public void Start()
        {
            var startingNode = nodes.Find(x => x is StartNode) as StartNode;
            if (startingNode == null) return;

            var port = startingNode.GetPort("out");
            if (port == null) return;

            current = null;

            for (var i = 0; i < port.ConnectionCount; i++)
            {
                var connection = port.GetConnection(i);
                if (connection.node is ChatNode chat)
                {
                    current = chat;
                    Debug.Log(current);
                }
            }
        }
    }
}