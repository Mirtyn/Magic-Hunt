using UnityEngine;
using UnityEngine.AI;

public interface IEnemy
{
    public Transform ThisTransform { get; }
    public Vector2 Position { get; }
    public float MinDistanceToPlayerToAttack { get; }
    public bool CanSeePlayer { get; }
    public float DistanceToPlayer { get; }
    public float MinDistanceToPlayer { get; }
    public Vector2 PlayerPos { get; }
    public NavMeshAgent NavMeshAgent { get; set; }

    public float Health { get; }
    public float ViewDistance { get; }
    public float HearDistance { get; }
    public float Damage { get; }
    public float Speed { get; set; }
    public float AccelerateSpeed { get; }
    public float Radius { get; }
    public IElement ActiveElement { get; }


    public void MoveToPlayer();

    public void CheckPlayerDistance();

    public bool TryCastRayToPlayer();

    public void TryAttack();

    public void Attack();

    public void GetDamaged(float damage);

    public void UpdateElement();

    public void SetActiveElement(IElement element);

    public void DeactivateElement();

    public void CheckForDeath();

    public void Death();
}
