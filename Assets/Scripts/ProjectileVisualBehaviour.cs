using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ProjectileVisualBehaviour : ProjectBehaviour
{
    private IProjectile projectile;
    [SerializeField] private Transform thisTransform;

    public Transform GetTransform() { return thisTransform; }

    public void Init(IProjectile projectile)
    {
        this.projectile = projectile;
    }

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(GameManager.ProjectileHitLayerMask);

        bool canMove;
        bool willBeDestroyed = false;

        float moveDistance = projectile.Stats.Speed * Time.deltaTime;

        RaycastHit2D[] results = new RaycastHit2D[5];
        int numCollisions = Physics2D.CircleCast(thisTransform.position, projectile.Stats.Size, thisTransform.up, contactFilter, results, moveDistance);

        if (numCollisions != 0)
        {
            canMove = false;

            for (int i = 0; i < numCollisions; i++)
            {
                if (results[i].transform.CompareTag(GameManager.WALL_TAG))
                {
                    canMove = false;
                    willBeDestroyed = true;
                }
                else if (results[i].transform.CompareTag(GameManager.ENEMY_TAG))
                {
                    if (projectile.Stats.Piercing > 0)
                    {
                        projectile.Stats.Piercing--;
                        IEnemy enemy = results[i].transform.GetComponent<IEnemy>();
                        enemy.GetDamaged(projectile.Stats.Damage);
                        enemy.SetActiveElement(projectile.Stats.Element);
                        canMove = true;
                    }
                    else
                    {
                        canMove = false;
                        IEnemy enemy = results[i].transform.GetComponent<IEnemy>();
                        enemy.GetDamaged(projectile.Stats.Damage);
                        enemy.SetActiveElement(projectile.Stats.Element);
                        willBeDestroyed = true;
                    }
                }
            }
        }
        else
        {
            canMove = true;
        }

        if (canMove)
        {
            thisTransform.position += thisTransform.up * moveDistance;
        }

        if (willBeDestroyed)
        {
            DisableProjectileVisual();
        }
    }

    private void DisableProjectileVisual()
    {
        projectile.DestroyProjectile();
        projectile = null;
        this.gameObject.SetActive(false);
    }

    public void Activate()
    {
        thisTransform.localScale = new Vector3(projectile.Stats.Size, projectile.Stats.Size, projectile.Stats.Size);
    }
}
