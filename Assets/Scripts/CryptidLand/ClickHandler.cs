using UnityEngine;
using UnityEngine.InputSystem;
using Enigmaware.Player;

namespace CryptidLand.Scripts
{
    public class ClickHandler : MonoBehaviour
    {
        public LayerMask IgnoredLayers;
        public CameraController CameraController;

        public float HoldForce;
        public float ThrowForce;

        [SerializeField]
        private bool _grabbing;
        private GameObject _grabObject;
        private Rigidbody _grabBody;
        private float _timer;

        public float distance;
        public float distanceDelta;
        public float distanceMin;
        public float distanceMax;
        
        public void ReadClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CheckClick();
            }
            else if (context.canceled)
            {

                if (_grabbing)
                {
                    EndGrab();
                }
            }
            /*if (context.performed)
            {
                if (!_grabbing)
                {
                    Grab();
                }

            }
            else if (context.canceled)
            {

                if (_grabbing)
                {
                    EndGrab();
                }
            }*/
        }

        private void CheckClick()
        {    
            if (Physics.Raycast(new Ray(CameraController.transform.position, CameraController.transform.forward), out RaycastHit hit, 15, ~IgnoredLayers))
            {
                if (!_grabbing)
                {
                    Rigidbody package = hit.collider.gameObject.GetComponent<Rigidbody>();
                    if (package)
                    {
                        _grabbing = true;
                        _grabObject = hit.collider.gameObject;
                        _grabBody = _grabObject.GetComponent<Rigidbody>();
                        _grabBody.velocity = Vector3.zero;
                        _grabBody.useGravity = false;
                        _grabBody.constraints = RigidbodyConstraints.FreezeRotation;
                        _grabBody.freezeRotation = true;
                        return;
                    }
                }
            }
        }
              
        public void Scroll(InputAction.CallbackContext context)
        {
            int y = (int)context.ReadValue<Vector2>().y;
            
            if (y > 0)
            {
                if (distance < distanceMax && distance + distanceDelta < distanceMax)
                {
                    distance += distanceDelta;
                }
            }

            else if (y < 0)
            {
                if (distance > distanceMin && distance - distanceDelta > distanceMin)
                {
                    distance -= distanceDelta;
                }        
            }
        }
    
    public void FixedUpdate()
        {
            if (_grabbing)
            {
                if (!_grabObject)
                {
                    EndGrab();
                    return;
                    
                }

                _timer += Time.deltaTime;

                Vector3 targetPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                targetPoint += Camera.main.transform.forward * distance;
                Vector3 force = targetPoint - _grabObject.transform.position;

                _grabBody.velocity *= Mathf.Min(1.0f, force.magnitude / 2);
                _grabBody.AddForce(force * HoldForce);
                _grabBody.velocity = force.normalized * _grabBody.velocity.magnitude;
            }
        }
    
        
        public void EndGrab()
        {
            _grabbing = false;
            _grabBody.velocity /= _grabBody.velocity.magnitude/ 2;
            _grabBody.AddForce(CameraController.transform.forward * ThrowForce, ForceMode.VelocityChange);
            _grabBody.useGravity = true;
            _grabBody.freezeRotation = false;

            _grabBody = null;
            _grabObject = null;
        }

    }
}