using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
    public class EquipmentAnimation : MonoBehaviour
    {
        [BHeader("Animation Controller & Clips", true)]

        [SerializeField]
        private AnimationOverrideClips m_Clips = null;


        public void AssignAnimations(Animator animator)
        {
            if(animator != null && m_Clips.Controller != null)
            {
                var overrideController = new AnimatorOverrideController(m_Clips.Controller);
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                foreach(var clipPair in m_Clips.Clips)
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clipPair.Original, clipPair.Override));

                overrideController.ApplyOverrides(overrides);

                animator.runtimeAnimatorController = overrideController;
            }
        }
    }
}