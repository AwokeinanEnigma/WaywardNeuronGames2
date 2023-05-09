using UnityEngine;

namespace CryptidLand.MapSegment
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