using UnityEngine;

namespace VOID.Scripts
{
    public class MapSegmentSetter : MonoBehaviour
    {
        public MapSegment MapSegment;
        
        public void UpdateMapSegmentRotation(Vector3 newRotation)
        {
            
            MapSegment.UpdateTargetRotation(newRotation);
        }
    }
}