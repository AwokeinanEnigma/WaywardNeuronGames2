#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class LedgeDetectionSettings
{
    public float MinLedgeWidth = 1f;
    public int MaxSurfaceRaycastSteps = 5;
    public float MaxSurfaceRaycastStepInterval = 2f;
    public float OverhangCheckHeight = 4f;
    public float MinDistanceToGround = 2f;
    public float ClearanceHeight = 4f;
    public float ObstructionCheckSize = .5f;
    public LayerMask GroundLayers;
}