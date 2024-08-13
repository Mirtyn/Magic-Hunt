using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootProjectileInput : AbstractInputNode
{
    public override string TopBarText { get; set; } = "Input Node:\nCold Blast";
    public override string Title { get; set; } = "Cold Blast";
    public override string Info { get; set; } = "4 projectiles\nDamage 1\nSpeed 5\nFrozen Projectiles";

    public override void Activate()
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
            stats.ProjectilesSpread
        );

        ConnectedNode.Activate(projectiles, stats);
    }
}
