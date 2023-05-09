using System;
using UnityEngine;

namespace Enigmaware.World
{
    public class RotateObject : MonoBehaviour
    {
        public Vector3 RotationAxis;
        public void Update()
        {
            transform.Rotate(RotationAxis.x, RotationAxis.y, RotationAxis.z);
        }
    }
}