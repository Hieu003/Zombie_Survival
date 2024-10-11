using UnityEngine;
using System;

namespace HQFPSWeapons
{
    [Serializable]
    public class RecoilForce
    {
        public Vector3 RotForce;
        public Vector3 PosForce;

        [Range(0, 20)]
        public int Distribution;

        [Space]

        [Tooltip("max randomness for each axis")]
        [Group]
        public ForceJitter JitterForce;


        public void PlayRecoilForce(float forceMultiplier, Spring rotSpring = null, Spring posSpring = null)
        {
            if (posSpring != null)
                posSpring.AddForce(Vector3Utils.JitterVector(PosForce / 100, JitterForce.xJitter, JitterForce.yJitter, JitterForce.zJitter) * forceMultiplier, Distribution);

            if (rotSpring != null)
                rotSpring.AddForce(Vector3Utils.JitterVector(RotForce, JitterForce.xJitter, JitterForce.yJitter, JitterForce.zJitter) * forceMultiplier, Distribution);
        }

        [Serializable]
        public struct ForceJitter 
        {
            [Range(0,1)]
            public float xJitter;

            [Range(0, 1)]
            public float yJitter;

            [Range(0, 1)]
            public float zJitter;
        }
    }
}

