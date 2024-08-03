using Assets.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOutput : INodeOutput
{
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public string TopBarText { get; set; } = "Output Node:\nTest Output";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(Color.cyan);
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            var projectile = projectiles[i];
            projectile.Stats = stats;
            projectile.projectileVisualBehaviour.Activate();
        }
    }

    public bool SendHeartBeat()
    {
        return true;
    }
}
