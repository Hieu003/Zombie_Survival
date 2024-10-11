using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HQFPSWeapons
{
    public class PostProcessingManager : Singleton<PostProcessingManager>
    {
        [SerializeField]
        private PostProcessVolume m_PPVolume = null;

        [BHeader("DeathAnim")]

        [SerializeField]
        private float m_DeathAnimSpeed = 1f;

        [SerializeField]
        [Range(-1,0)]
        private float m_MinSaturationAmount = -1f;

        private PostProcessProfile m_EditorProfile;
        private PostProcessProfile m_Profile;

        private float m_DefaultSaturation;
        private ColorGrading m_ColorGrading;
        private bool m_PlayerDead;


        public void EnableAimBlur(bool enable)
        {
            var depthOfField = m_Profile.GetSetting<DepthOfField>();
            
            depthOfField.active = enable;
        }

        public void DoDeathAnim() 
        {
            m_PlayerDead = true;

            StartCoroutine(C_DoDeathAnim());
        }

        public void RestoreDefaultProfile() 
        {
            m_PlayerDead = false;
        }

        private void Awake()
        {
            m_EditorProfile = m_PPVolume.profile;
            m_Profile = Instantiate(m_EditorProfile);
            m_PPVolume.profile = m_Profile;

            m_ColorGrading = m_PPVolume.profile.GetSetting<ColorGrading>();
            m_DefaultSaturation = m_ColorGrading.saturation;
        }

        private void OnDestroy()
        {
            m_PPVolume.profile = m_EditorProfile;
        }

        private IEnumerator C_DoDeathAnim() 
        {
            float saturation = m_ColorGrading.saturation.value;
            float requiredSaturation = m_MinSaturationAmount * 100;

            while (m_PlayerDead) 
            {
                saturation = Mathf.Lerp(saturation, requiredSaturation, Time.deltaTime * m_DeathAnimSpeed);

                m_ColorGrading.saturation.value = saturation;

                yield return null;
            }

            if(!m_PlayerDead)
                m_ColorGrading.saturation.value = m_DefaultSaturation;
        }
    }
}