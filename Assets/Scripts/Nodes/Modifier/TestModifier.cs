using Assets.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestModifier : AbstractModifierNode
{
    public override string TopBarText { get; set; } = "Modifier Node:\nTest Modifier";
    public override string Title { get; set; } = "Test Modifier";
    public override string Info { get; set; } = "Test Modifier";

    public override void Activate(List<IProjectile> projectiles, StatsHolder stats)
    {
        ConnectedNode.Activate(projectiles, stats);
    }
}
