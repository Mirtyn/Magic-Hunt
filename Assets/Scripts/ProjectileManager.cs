using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ProjectBehaviour
{
    public static ProjectileManager Instance { get; private set; }
    private ObjectPooler pooler;

    private void Awake()
    {
        Instance = this;
        pooler = new ObjectPooler(ProjectBehaviour.GameManager.ProjectilePrefab, 200, true, this.transform);
    }

    public T CreateProjectile<T>() where T : IProjectile, new()
    {
        GameObject projectileObject = pooler.GetPooledObject();
        projectileObject.SetActive(true);
        T projectile = new T();
        ProjectileVisualBehaviour projectileVisualBehaviour = projectileObject.GetComponent<ProjectileVisualBehaviour>();
        projectileVisualBehaviour.Init(projectile);
        projectile.CreateProjectile(projectileVisualBehaviour);
        projectileVisualBehaviour.GetTransform().position = Vector3.zero;
        projectileVisualBehaviour.GetTransform().rotation = Quaternion.identity;
        return projectile;
    }

    public List<IProjectile> SpreadProjectiles(List<IProjectile> projectiles, Vector3 origin, float distance, Vector3 direction, float betweenAngle)
    {
        if (projectiles.Count == 0) return projectiles;

        if (projectiles.Count == 1)
        {
            var transform = projectiles[0].projectileVisualBehaviour.GetTransform();
            transform.position = origin;
            transform.up = direction.normalized;
            transform.position += transform.up * distance;
            return projectiles;
        }

        float angleStep = betweenAngle / (projectiles.Count - 1);
        float halfAngle = -(betweenAngle / 2);

        for (int i = 0; i < projectiles.Count; i++)
        {
            var transform = projectiles[i].projectileVisualBehaviour.GetTransform();
            transform.position = origin;
            transform.up = direction.normalized;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (halfAngle + angleStep * i));
            transform.position += transform.up * distance;
        }

        return projectiles;
    }
}