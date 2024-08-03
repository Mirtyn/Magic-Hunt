using Assets.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    public ProjectileVisualBehaviour projectileVisualBehaviour { get; set; }

    public StatsHolder Stats { get; set; }
    public void CreateProjectile(ProjectileVisualBehaviour projectileVisualBehaviour);
    public void DestroyProjectile();
}
