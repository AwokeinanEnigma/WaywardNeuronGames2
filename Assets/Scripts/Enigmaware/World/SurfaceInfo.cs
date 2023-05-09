#region

using UnityEngine;

#endregion

namespace Enigmaware.World
{
    [CreateAssetMenu(fileName = "SurfaceInfo", menuName = "Movement/SurfaceInfo", order = 0)]
    /// <summary>
    /// Holds information about a surface.
    /// </summary>
    public class SurfaceInfo : ScriptableObject
    {
        public bool IsWallrunable;
        public bool IsGrappable;
        public bool IsMoving;
        public float Drag;

        public static SurfaceInfo FindSurfaceInfo(Collider collider)
        {
            SurfaceHolder holder = collider.GetComponent<SurfaceHolder>();
            if (holder)
            {
                return holder.SurfaceInfo;
            }

            return null;
        }
    }
}