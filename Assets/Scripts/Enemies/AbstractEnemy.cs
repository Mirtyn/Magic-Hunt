using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public abstract class AbstractEnemy : ProjectBehaviour
{
    public Transform ThisTransform { get; private set; }
    public Vector2 Position { get; private set; }
    public bool CanSeePlayer { get; private set; } = false;
    public float DistanceToPlayer { get; private set; } = 1000000f;
    public float MinDistanceToPlayer { get; private set; } = 0.1f;
    public Vector2 PlayerPos { get; private set; }
    public NavMeshAgent NavMeshAgent { get; set; }

    public virtual float Health { get; private set; } = 5f;
    public virtual float MinDistanceToPlayerToAttack { get; private set; } = 0.8f;
    public virtual float ViewDistance { get; private set; } = 8f;
    public virtual float Damage { get; private set; } = 1f;
    public virtual float Speed { get; set; } = 2f;
    public virtual float AccelerateSpeed { get; private set; } = 6f;
    public virtual float Radius { get; private set; } = 0.4f;
    public IElement ActiveElement { get; private set; } = null;

    public virtual void Awake()
    {
        ThisTransform = transform;
        Position = ThisTransform.position;
        NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent.speed = Speed;
        NavMeshAgent.acceleration = AccelerateSpeed;
        NavMeshAgent.radius = Radius;
    }

    public virtual void Update()
    {
        Position = ThisTransform.position;
        PlayerPos = PlayerStats.Instance.GetPositionVector2();
        CheckPlayerDistance();
        UpdateElement();
        TryAttack();
    }

    public virtual void UpdateElement()
    {
        ActiveElement?.Update();
    }

    public virtual void CheckPlayerDistance()
    {
        DistanceToPlayer = Vector2.Distance(Position, PlayerPos);

        if (DistanceToPlayer < ViewDistance)
        {
            if (TryCastRayToPlayer())
            {
                MoveToPlayer();
            }
        }
    }

    public virtual bool TryCastRayToPlayer()
    {
        Vector2 dir = PlayerPos - Position;
        RaycastHit2D hitData = Physics2D.Raycast(Position, dir.normalized, ViewDistance, GameManager.PlayerLayerMask);
        Debug.DrawLine(Position, hitData.point, Color.red);
        if (hitData.transform == null)
        {
            CanSeePlayer = false;
            return CanSeePlayer;
        }

        if (hitData.transform.CompareTag(GameManager.PLAYER_TAG))
        {
            CanSeePlayer = true;
            return CanSeePlayer;
        }
        else
        {
            CanSeePlayer = false;
            return CanSeePlayer;
        }
    }

    public virtual void MoveToPlayer()
    {
        if (!(DistanceToPlayer <= MinDistanceToPlayer))
        {
            NavMeshAgent.SetDestination(PlayerPos);
        }
    }

    public virtual void TryAttack()
    {
        if (DistanceToPlayer < MinDistanceToPlayerToAttack && CanSeePlayer)
        {
            Attack();
        }
    }

    public virtual void Attack()
    {
        PlayerStats.Instance.GetAttacked(Damage);
    }

    public virtual void GetDamaged(float damage)
    {
        Health -= damage;
        CheckForDeath();
    }

    public virtual void SetActiveElement(IElement element)
    {
        if (element == null)
        {
        }
        else if (ActiveElement == null)
        {
            ActiveElement = element;
        }
        else
        {
            ActiveElement.End();
            ActiveElement = element;
        }

        ActiveElement?.ApplyOnEnemy(this);
    }

    public virtual void DeactivateElement()
    {
        ActiveElement = null;
    }

    public virtual void CheckForDeath()
    {
        if (Health <= 0)
        {
            Death();
        }
    }

    public virtual void Death()
    {
        Destroy(this.gameObject);
    }
}
