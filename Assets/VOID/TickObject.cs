using UnityEngine;
using UnityEngine.InputSystem;

namespace VOID
{
    public class TickObject : MonoBehaviour
    {
        public GameObject MapObject;
        public Camera MapCamera;
        public void Check(InputAction.CallbackContext context)
        {
            Debug.Log("hai");
            if (context.performed)
            {
                Debug.Log("perfed");
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
                {
                    
                    Debug.Log("p22erfed");
                    Vector3 normal = hit.textureCoord;
                    Ray ray = MapCamera.ScreenPointToRay(normal);
                    RaycastHit screenHit;
 
                    //Get the point of where it was clicked on the plane
                    if(Physics.Raycast(ray, out screenHit))
                    {
                        Debug.DrawRay(ray.origin, ray.direction * 5000, Color.cyan, 250);
                        //If that point was an item (another ray determined there was an item)...
                        {
                            Debug.Log(screenHit.transform);
                            print("hit");
                        }
                    }
                    /*
                    if (hit.collider.gameObject == MapObject)
                    {
                        if (Physics.Raycast(MapCamera.ViewportPointToRay(hit.textureCoord), out hit, 5000))
                        {
                            
                            Debug.Log($"hello nigga: {hit.distance}" );

                        }

                        Debug.Log("Clicked on " + gameObject.name);
                    }*/
                }
            }
        }
    }
}