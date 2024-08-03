using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestModifier : INodeModifier
{
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public string TopBarText { get; set; } = "Modifier Node:\nTest Modifier";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(Color.green);
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        ConnectedNode.Activate(projectiles, stats);
    }

    public bool SendHeartBeat()
    {
        if (ConnectedNode != null)
        {
            return ConnectedNode.SendHeartBeat();
        }

        return false;
    }
}
