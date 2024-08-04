using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : ProjectBehaviour, IEnemy
{
    public Transform ThisTransform { get; private set; }
    public Vector2 Position { get; private set; }
    public bool CanSeePlayer { get; private set; } = false;
    public float DistanceToPlayer { get; private set; } = 1000000f;
    public float MinDistanceToPlayer { get; private set; } = 0.1f;
    public Vector2 PlayerPos { get; private set; }
    public NavMeshAgent NavMeshAgent { get; set; }

    public float Health { get; private set; } = 5f;
    public float MinDistanceToPlayerToAttack { get; private set; } = 0.8f;
    public float ViewDistance { get; private set; } = 16f;
    public float HearDistance { get; private set; } = 4f;
    public float Damage { get; private set; } = 1f;
    public float Speed { get; set; } = 2f;
    public float AccelerateSpeed { get; private set; } = 6f;
    public float Radius { get; private set; } = 0.4f;
    public IElement ActiveElement { get; private set; } = null;


    private void Awake()
    {
        ThisTransform = transform;
        Position = ThisTransform.position;
        NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent.speed = Speed;
        NavMeshAgent.acceleration = AccelerateSpeed;
        NavMeshAgent.radius = Radius;
    }

    private void Update()
    {
        Position = ThisTransform.position;
        PlayerPos = PlayerStats.Instance.GetPositionVector2();
        CheckPlayerDistance();
        UpdateElement();
        TryAttack();
    }

    public void UpdateElement()
    {
        ActiveElement?.Update();
    }

    public void CheckPlayerDistance()
    {
        DistanceToPlayer = Vector2.Distance(Position, PlayerPos);

        if (DistanceToPlayer < HearDistance)
        {
            TryCastRayToPlayer();
            MoveToPlayer();
        }
        else if (DistanceToPlayer < ViewDistance)
        {
            if (TryCastRayToPlayer())
            {
                MoveToPlayer();
            }
        }
    }

    public void MoveToPlayer()
    {
        if (!(DistanceToPlayer <= MinDistanceToPlayer))
        {
            NavMeshAgent.SetDestination(PlayerPos);
        }
    }

    public bool TryCastRayToPlayer()
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

    public void TryAttack()
    {
        if (DistanceToPlayer < MinDistanceToPlayerToAttack && CanSeePlayer)
        {
            Attack();
        }
    }

    public void Attack()
    {
        PlayerStats.Instance.GetAttacked(Damage);
    }

    public void GetDamaged(float damage)
    {
        Health -= damage;
        CheckForDeath();
    }

    public void SetActiveElement(IElement element)
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

    public void DeactivateElement()
    {
        ActiveElement = null;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Destroy(this.gameObject);
    }
}