using UnityEngine;

namespace VOID.Scripts
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