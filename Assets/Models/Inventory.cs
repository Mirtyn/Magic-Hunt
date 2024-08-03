using System.Collections.Generic;

namespace Assets.Models
{
    public class Inventory
    {
        public List<InventoryCollection<INode>> NodeCollection = new List<InventoryCollection<INode>>();

        public void StoreNode<T>(T node) where T : INode
        {
            for (int i = 0; i < NodeCollection.Count; i++)
            {
                if (NodeCollection[i].List[0] is T)
                {
                    NodeCollection[i].AddNode(node);

                    return;
                }
            }

            InventoryCollection<INode> nodeCollection = new InventoryCollection<INode>(node);
            NodeCollection.Add(nodeCollection);
        }
    }

    public class InventoryCollection<T> where T : INode
    {
        public List<INode> List = new List<INode>();

        public void AddNode(T node)
        {
            List.Add(node);
        }

        public InventoryCollection(T node)
        {
            AddNode(node);
        }
    }
}
