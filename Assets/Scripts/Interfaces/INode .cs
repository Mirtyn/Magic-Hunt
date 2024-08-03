using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    //public Vector2 Position { get; set; }
    //public INode ConnectedInput { get; set; }
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public string TopBarText { get; set; }

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public bool TryAttachNode(INode node)
    {
        if (ConnectedNode == null)
        {
            ConnectedNode = node;
            node.PrevConnectedNode = this;

            return true;
        }

        return false;
    }

    public void TryDetachNode()
    {
        if (PrevConnectedNode != null)
        {
            PrevConnectedNode.ConnectedNode = null;
            PrevConnectedNode = null;
        }
    }

    public void CreateNode(NodeVisualBehaviour nodeVisualBehaviour);

    //public void ReceiveInput(List<IProjectile> projectiles);

    //public void SendOutput(List<IProjectile> projectiles);

    public void Activate(List<IProjectile> projectiles, StatsHolder stats);

    public bool SendHeartBeat();
}
