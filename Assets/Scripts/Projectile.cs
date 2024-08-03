using Assets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : IProjectile
{
    public ProjectileVisualBehaviour projectileVisualBehaviour { get; set; }

    public StatsHolder Stats { get; set; } = new StatsHolder();

    //public Vector2 direction { get; set; }
    //public float speed { get; set; }
    //public Vector3 position { get; set; }

    public void CreateProjectile(ProjectileVisualBehaviour projectileVisualBehaviour)
    {
        this.projectileVisualBehaviour = projectileVisualBehaviour;
    }

    public void DestroyProjectile()
    {
        projectileVisualBehaviour = null;
        Stats = null;
    }
}
