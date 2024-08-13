using Assets.Models;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInputNode : INodeInput
{
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public virtual string TopBarText { get; set; } = "Input Node:\nUnimplemented Input";
    public virtual string Title { get; set; } = "Unimplemented Input";
    public virtual string Info { get; set; } = "Unimplemented Input";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }

    public virtual void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(new Color(0f, 0.4f, 1f));
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        this.Activate();
    }

    public virtual void Activate()
    {
        StatsHolder stats = StatsHolder.Default;

        List<IProjectile> projectiles = new List<IProjectile>
        {
            ProjectileManager.Instance.CreateProjectile<Projectile>()
        };

        PlayerStats playerStats = PlayerStats.Instance;
        Transform transform = playerStats.transform;

        projectiles = ProjectileManager.Instance.SpreadProjectiles
        (
            projectiles,
            transform.position,
            playerStats.GetSpawnProjectilesDistance(),
                new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y).normalized,
            stats.ProjectilesSpread
        );

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
