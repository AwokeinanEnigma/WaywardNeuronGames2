using UnityEngine;

namespace CryptidLand.MapSegment
{
    public class PlatformSetter : MonoBehaviour
    {
        public Platform platform;

        public void UpdatePlatformPosition(float newRotation)
        {
            if (!platform.Risen)
            {
                platform.UpdateTargetPosition(newRotation);
            }
            else 
            {
                platform.Reset();
            }

        }
        
    }
}