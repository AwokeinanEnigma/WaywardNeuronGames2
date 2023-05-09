﻿#region

using UnityEngine;

#endregion

namespace Enigmaware.World
{
    [ExecuteInEditMode]
    public class FPS : MonoBehaviour
    {
        public int CurrentFramerateTarget;

        public void OnEnable()
        {
            Application.targetFrameRate = CurrentFramerateTarget;
        }

        public void Awake()
        {
            Application.targetFrameRate = CurrentFramerateTarget;
        }

        public void SetFPS(int fps)
        {
            CurrentFramerateTarget = fps;
            Application.targetFrameRate = CurrentFramerateTarget;
        }
    }
}