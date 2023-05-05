using UnityEngine;
using VOID.Player;

namespace VOID.Scripts
{
    public class OverheadCameraManager : MonoBehaviour
    {
        public PlayerMotor Motor;
        public float HoverDistance;
        
        public void Update()
        {
            if (Motor != null)
            {
                transform.position = Motor.transform.position + Motor.Up * HoverDistance;
            }
        }
    }
}