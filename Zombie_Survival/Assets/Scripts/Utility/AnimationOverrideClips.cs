using System;
using UnityEngine;

namespace HQFPSWeapons
{
    [Serializable]
    public class AnimationOverrideClips
    {
        [Serializable]
        public struct AnimationClipPair
        {
            public AnimationClip Original;
            public AnimationClip Override;
        }

        public RuntimeAnimatorController Controller { get => m_Controller; }
        public AnimationClipPair[] Clips { get => m_Clips; }

        [SerializeField]
        private RuntimeAnimatorController m_Controller = null;

        [SerializeField]
        private AnimationClipPair[] m_Clips = null;
    }
}