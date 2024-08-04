using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootProjectileInput : INodeInput
{
    //public Vector2 Position { get; set; }
    //public INode ConnectedInput { get; set; }
    //public NodeHook AttachedNodeHook { get; set; }
    public NodeVisualBehaviour NodeVisualBehaviour { get; set; }

    public string TopBarText { get; set; } = "Input Node:\nShoot Projectile";

    public INode ConnectedNode { get; set; }
    public INode PrevConnectedNode { get; set; }


    public void CreateNode(NodeVisualBehaviour nodeVisualBehaviour)
    {
        NodeVisualBehaviour = nodeVisualBehaviour;
        nodeVisualBehaviour.SetColor(new Color(0f, 0.4f, 1f)) ;
        nodeVisualBehaviour.Node = this;
        nodeVisualBehaviour.SetTopBarText(TopBarText);
    }

    public void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {

    }

    public void Activate()
    {
        StatsHolder stats = StatsHolder.Default;

        List<IProjectile> projectiles = new List<IProjectile>
        {
            ProjectileManager.Instance.CreateProjectile<Projectile>(),
            ProjectileManager.Instance.CreateProjectile<Projectile>(),
            ProjectileManager.Instance.CreateProjectile<Projectile>(),
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
            60f
        );

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
