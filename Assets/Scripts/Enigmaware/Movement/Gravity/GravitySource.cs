#region

using UnityEngine;

#endregion

namespace Enigmaware.Movement.Gravity
{
    public class GravitySource : MonoBehaviour
    {
        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        private void OnEnable()
        {
            GravityFinder.Register(this);
        }

        private void OnDisable()
        {
            GravityFinder.Unregister(this);
        }
    }
}