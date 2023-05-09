using UnityEditor;
using UnityEngine;

namespace Editor
{
    #if UNITY_EDITOR
   /*[CustomEditor(typeof(MapSegment))]
    public class MapSegmentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MapSegment mapSegment = (MapSegment) target;
            
            if (GUILayout.Button("Rotate"))
            {
                mapSegment.UpdateTargetRotation(new Vector3(0, mapSegment.transform.rotation.eulerAngles.y -90,0));
            }
        }
    }*/
    #endif
}