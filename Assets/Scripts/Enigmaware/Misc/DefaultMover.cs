using System;
using KinematicCharacterController;
using UnityEngine;

namespace Enigmaware.Scripts
{
    public class DefaultMover: MonoBehaviour, IMoverController
    {
        private Vector3 _velocity;
        private MoveSetting _setting;

        public Vector3 Gravity;
        
        public enum MoveSetting
        {
            Default,
            Deferred,
        }

        public void SetGravity(Vector3 gravity)
        {
            Gravity = gravity;
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }
        
        public void SetMoveSetting(MoveSetting setting)
        {
            _setting = setting;
        }

        public void FixedUpdate()
        {
            if (_setting != MoveSetting.Deferred)
            {
                _velocity = Vector3.zero;
            }
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = base.transform.position + _velocity * deltaTime;
            goalPosition += Gravity * deltaTime;
            
            goalRotation = base.transform.rotation;
        }
    }
}