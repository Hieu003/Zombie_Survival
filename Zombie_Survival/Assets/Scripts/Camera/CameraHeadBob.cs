using System;
using UnityEngine;

namespace HQFPSWeapons
{
    [Serializable]
    public class CameraHeadBob
    {
        public Vector3 PosAmplitude = new Vector4(0.35f, 0.5f, 0f);

        public Vector3 RotationAmplitude = new Vector4(0.35f, 0.5f, 0f);

        [Range(0.1f, 100f)]
        public float HeadBobSpeed = 1f;
    }
}
