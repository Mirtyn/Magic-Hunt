using Assets.Models;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractModifierNode : INodeModifier
{
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public virtual string TopBarText { get; set; } = "Input Node:\nUnimplemented Modifier";
    public virtual string Title { get; set; } = "Unimplemented Modifier";
    public virtual string Info { get; set; } = "Unimplemented Modifier";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public virtual void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(Color.green);
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public virtual void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        ConnectedNode.Activate(projectiles, stats);
    }

    public virtual bool SendHeartBeat()
    {
        if (ConnectedNode != null)
        {
            return ConnectedNode.SendHeartBeat();
        }

        return false;
    }
}