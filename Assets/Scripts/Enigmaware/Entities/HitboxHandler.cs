#region

using UnityEngine;

#endregion

namespace Enigmaware.Entities
{
    public class HitboxHandler : MonoBehaviour
    {
        public enum HitboxType
        {
            Head,
            Body,
            WeakPoint,
            Package
        }

        public BoxCollider hitboxCollider;
        public HitboxType type;
        public HealthComponent healthComponent;
    }
}