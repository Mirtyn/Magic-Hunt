using Assets.Models;
using System.Collections.Generic;

public class TestOutput : AbstractOutputNode
{
    public override string TopBarText { get; set; } = "Output Node:\nTest Output";
    public override string Title { get; set; } = "Test Output";
    public override string Info { get; set; } = "Test Output";

    public override void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            var projectile = projectiles[i];
            projectile.Stats = stats;
            projectile.projectileVisualBehaviour.Activate();
        }
    }
}
