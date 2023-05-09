using UnityEngine;
using Enigmaware.Player;

namespace CryptidLand.OverheadMap
{
    public class OverheadCameraMover : MonoBehaviour
    {
        public PlayerMotor Motor;
        
        public float HoverDistance;
        public float MinimumHoverDistance;
        public float MaximumHoverDistance;
        
        public void Update()
        {
            if (Motor != null)
            {
                transform.position = Motor.transform.position + Motor.Up * HoverDistance;
            }
        }
    }
}