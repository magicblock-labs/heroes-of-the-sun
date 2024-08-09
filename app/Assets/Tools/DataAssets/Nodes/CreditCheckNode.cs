using Model;
using Tools.DataAssets.xNode_1._8._0.Scripts;
using UnityEngine;

namespace Tools.DataAssets.Nodes
{
    [NodeTint("#992222")]
    public class CreditCheckNode : xNode_1._8._0.Scripts.Node, IDialogueNode
    {
        [Input(ShowBackingValue.Never)] public string @in;

        public int value;
        public bool perBuilding;

        [SerializeField] [Output] private string @pass;

        [SerializeField] [Output] private string @fail;


        public void Trigger()
        {
            var toCheck = value * (perBuilding ? BuildingsModel.Instance.Get()?.Length ?? 0 : 1);
            GoTo(BalanceModel.Instance.Get() < toCheck ? GetPort("fail") : GetPort("pass"));
        }

        void GoTo(NodePort port)
        {
            for (var i = 0; i < port.ConnectionCount; i++)
            {
                var connection = port.GetConnection(i);
                if (connection.node is IDialogueNode node)
                {
                    node.Trigger();
                }
            }
        }
    }
}