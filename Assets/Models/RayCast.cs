using System;
using UnityEngine;

namespace Assets.Models
{
    public class RayCast
    {
        public static Ray CreateRay(Vector2 origin, Vector2 direction)
        {
            Ray ray = new Ray(origin, direction);
            return ray;
        }

        public static bool CastRayForRectTransforms(Ray ray, float stepSize, float maxDistance, out RayCastHitData rayCastHitData, RectTransform[] rectTransformColliders)
        {
            rayCastHitData = new RayCastHitData();

            if (ray.Direction == Vector2.zero || ray.Direction.normalized == Vector2.zero)
            {
                return false; 
            }

            if (stepSize <= 0.01f)
            {
                stepSize = 0.01f;
            }

            float timesToLoop = Mathf.Round(maxDistance / stepSize);

            Vector2 point = ray.Origin;

            for (float timesAlreadyLooped = 0; timesAlreadyLooped <= timesToLoop; timesToLoop++)
            {
                point += ray.Direction.normalized * stepSize;

                for (int i = 0; i < rectTransformColliders.Length; i++)
                {
                    Vector2 minCorner = new Vector2(rectTransformColliders[i].position.x, rectTransformColliders[i].position.y) + rectTransformColliders[i].rect.min;
                    Vector2 maxCorner = new Vector2(rectTransformColliders[i].position.x, rectTransformColliders[i].position.y) + rectTransformColliders[i].rect.max;

                    if (minCorner.x < point.x && maxCorner.x > point.x &&
                        minCorner.y < point.y && maxCorner.y > point.y)
                    {
                        rayCastHitData = new RayCastHitData(rectTransformColliders[i], Vector2.Distance(ray.Origin, point));
                        return true;
                    }
                }
            }

            return false;
        }

        //private static void Cast(Ray ray, float stepSize, float maxDistance, Rect[] rectColliders)
        //{

        //}
    }

    public struct Ray
    {
        public Vector2 Origin;
        public Vector2 Direction;

        public Ray(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }

    public struct RayCastHitData
    {
        public RectTransform HitTransform;
        public float Distance;

        public RayCastHitData(RectTransform hitTransform, float distance)
        {
            HitTransform = hitTransform;
            Distance = distance;
        }
    }
}