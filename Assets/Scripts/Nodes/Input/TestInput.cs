using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestInput : AbstractInputNode
{
    public override string TopBarText { get; set; } = "Input Node:\nTest Input";
    public override string Title { get; set; } = "Test Input";
    public override string Info { get; set; } = "Test Input";


    public override void Activate()
    {
        StatsHolder stats = StatsHolder.Default;
        stats.Element = new IceElement();

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
            60f
        );

        ConnectedNode.Activate(projectiles, stats);
    }
}
