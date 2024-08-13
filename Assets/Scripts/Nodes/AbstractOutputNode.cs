using Assets.Models;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractOutputNode : INodeOutput
{
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public virtual string TopBarText { get; set; } = "Input Node:\nUnimplemented Output";
    public virtual string Title { get; set; } = "Unimplemented Output";
    public virtual string Info { get; set; } = "Unimplemented Output";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public virtual void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(Color.cyan);
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public virtual void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            var projectile = projectiles[i];
            projectile.Stats = stats;
            projectile.projectileVisualBehaviour.Activate();
        }
    }

    public virtual bool SendHeartBeat()
    {
        return true;
    }
}