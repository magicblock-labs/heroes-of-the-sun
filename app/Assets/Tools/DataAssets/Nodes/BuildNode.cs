using Model;
using Service;
using Tools.DataAssets.xNode_1._8._0.Scripts;

namespace Tools.DataAssets.Nodes
{
    [NodeTint("#994422")]
    public class BuildNode : Node, IDialogueNode
    {
        
        [Input(ShowBackingValue.Never)] public string @in;
        public BuildingType building;

        public async void Trigger()
        {
            InteractionStateModel.Instance.StartPlacement(building);
        }
    }
}