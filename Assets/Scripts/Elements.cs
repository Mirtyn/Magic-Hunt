using UnityEngine;

public interface IElement
{
    public AbstractEnemy Enemy { get; set; }
    public float TimeDelta { get; set; }
    public float MaxTime { get; set; }

    public void ApplyOnEnemy(AbstractEnemy enemy);

    public void Update();

    public void End();
}

public class FireElement : IElement
{
    public AbstractEnemy Enemy { get; set; }
    public float TimeDelta { get; set; } = 0f;
    public float MaxTime { get; set; } = 3f;
    private float interval = 0.75f;
    private float currentInterval;
    private float damage = 0.75f;

    public void ApplyOnEnemy(AbstractEnemy enemy)
    {
        Enemy = enemy;
        currentInterval = MaxTime - interval;
        TimeDelta = MaxTime;
    }

    public void Update()
    {
        TimeDelta -= Time.deltaTime;

        if (TimeDelta < currentInterval )
        {
            currentInterval -= interval;
            Enemy.GetDamaged(damage);
        }

        if (TimeDelta < 0f)
        {
            End();
        }
    }

    public void End()
    {
        Enemy.DeactivateElement();
    }
}

public class IceElement : IElement
{
    public AbstractEnemy Enemy { get; set; }
    public float TimeDelta { get; set; } = 0f;
    public float MaxTime { get; set; } = 4.5f;
    private float originalSpeed;
    private float debuffedSpeed;

    public void ApplyOnEnemy(AbstractEnemy enemy)
    {
        Enemy = enemy;
        originalSpeed = enemy.Speed;
        debuffedSpeed = originalSpeed / 5f;
        enemy.Speed = debuffedSpeed;
        enemy.NavMeshAgent.speed = debuffedSpeed;
        TimeDelta = MaxTime;
    }

    public void Update()
    {
        TimeDelta -= Time.deltaTime;

        if (TimeDelta < 0f)
        {
            End();
        }
    }

    public void End()
    {
        Enemy.Speed = originalSpeed;
        Enemy.NavMeshAgent.speed = originalSpeed;
        Enemy.DeactivateElement();
    }
}
