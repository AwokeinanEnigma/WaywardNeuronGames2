using UnityEngine;

namespace Enigmaware.World
{


    public class Bobble : MonoBehaviour
    {
        public float bobbleHeight = 0.5f;
        public float bobbleSpeed = 1f;

        private float startY;

        private void Start()
        {
            // Remember the object's starting y-position
            startY = transform.position.y;
        }

        private void Update()
        {
            // Calculate the y-position offset based on sine wave
            float y = startY + Mathf.Sin(Time.time * bobbleSpeed) * bobbleHeight;

            // Update the object's position
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }
}