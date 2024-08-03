using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Assets.Models
{
    public class RayMarching
    {
        private float Length(Vector2 vector)
        {
            return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
        }

        private float SignedDstToCircle(Vector2 point, Vector2 origin, float radius)
        {
            return Length(origin - point) - radius;
        }

        //private float signedDstToBox(Vector2 point, Vector2 origin, Vector2 size)
        //{
        //    Vector2 offset = new Vector2(Mathf.Abs(point.x - origin.x), Mathf.Abs(point.y - origin.y)) - size;
        //    //dst from point outside box to edge(0 if inside)
        //    float unsignedDst = Length(Mathf.Max(offset, 0));
        //    // -dst from point inside box to edge (0 if outside box)
        //    float dstInsideBox = Length(Mathf.Min(offset, 0));
        //    return unsignedDst + dstInsideBox;
        //}
    }
}
