using CryptidLand.MapSegment;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CryptidLand.OverheadMap
{
    public class OverheadMapInteractor : MonoBehaviour
    {
        public GameObject MapObject;
        public OverheadCameraMover Map;
        public Camera gridCamera; //Camera that renders to the texture
        
        public RectTransform textureRectTransform; //RawImage RectTransform that shows the RenderTexture on the UI        
        public float HoverDistanceIncrement;
        public void Enable(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MapObject.SetActive(!MapObject.activeSelf);
                Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked);
            }
        }

        public void Scroll(InputAction.CallbackContext context)
        {
            if (MapObject.activeSelf && context.performed)
            {
                int y = (int)context.ReadValue<Vector2>().y;
                if (y > 0)
                {
                    Map.HoverDistance = Mathf.Max(Map.HoverDistance - HoverDistanceIncrement, Map.MinimumHoverDistance);
                    return;
                    
                }
                if (y < 0)
                {
                    
                    Map.HoverDistance = Mathf.Min(Map.HoverDistance + HoverDistanceIncrement, Map.MaximumHoverDistance);
                }
            }
        }
        
        public void Click(InputAction.CallbackContext context)
        {
            if (context.performed && MapObject.activeSelf)
            {
                //I get the point of the RawImage where I click
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textureRectTransform, Mouse.current.position.ReadValue(), null, out Vector2 localClick);
                //My RawImage is 700x700 and the click coordinates are in range (-350,350) so I transform it to (0,700) to then normalize
                Rect rect = textureRectTransform.rect;
                localClick.x = (rect.xMin * -1) - (localClick.x * -1);
                localClick.y = (rect.yMin * -1) - (localClick.y * -1);

                //I normalize the click coordinates so I get the viewport point to cast a Ray
                Vector2 viewportClick = new(localClick.x / rect.size.x, localClick.y / rect.size.y);

                //I have a special layer for the objects I want to detect with my ray

                //I cast the ray from the camera which rends the texture
                Ray ray = gridCamera.ViewportPointToRay(new Vector3(viewportClick.x, viewportClick.y, 0));

                if (Physics.Raycast(ray, out RaycastHit hit, 600))
                {
                    MapSegmentSetter mapSegment = hit.collider.GetComponent<MapSegmentSetter>();
                    PlatformSetter mapSegmentRotation = hit.collider.GetComponent<PlatformSetter>();
                    
                    if (mapSegment)
                    {
                        Debug.Log("Found mapsegment");
                        mapSegment.UpdateMapSegmentRotation(new Vector3(0, mapSegment.transform.rotation.eulerAngles.y - 90, 0));
                    }

                    if (mapSegmentRotation)
                    {
                        Debug.Log("Found mapsegmentrotation");
                        mapSegmentRotation.UpdatePlatformPosition(5);
                    }
                    
                    //Debug.Log(hit.collider.gameObject.name);
                }
            }
        }
    }
}