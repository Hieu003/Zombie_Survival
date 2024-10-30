using UnityEngine;

namespace HQFPSWeapons
{
    public class SurfaceEffect : MonoBehaviour
    {
        [SerializeField]
        private SoundPlayer m_Audio = null;

        //private ParticleSystem[] m_Particles;
        //private AudioSource m_AudioSource;


        //public void Play(float audioVolume)
        //{
        //    m_Audio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource, audioVolume);

        //    for(int i = 0;i < m_Particles.Length;i++)
        //        m_Particles[i].Play();
        //}

        //private void Awake()
        //{
        //    m_Particles = GetComponentsInChildren<ParticleSystem>();
        //    m_AudioSource = GetComponent<AudioSource>();
        //}

        [SerializeField]
        [HideInInspector]
        private ParticleSystem[] m_Particles;

        [SerializeField]
        [HideInInspector]
        private AudioSource m_AudioSource;

        [HideInInspector]
        [SerializeField]
        private bool m_Initialized;


        public void Init(SoundPlayer audioEffect, GameObject visualEffect, bool spatializeAudio)
        {
            if(m_Initialized)
                return;

            m_Audio = audioEffect;

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1f;

            if(spatializeAudio)
                m_AudioSource.spatialize = true;

            if(visualEffect != null)
            {
                Instantiate(visualEffect, transform.position, transform.rotation, transform);
                m_Particles = GetComponentsInChildren<ParticleSystem>();
            }

            m_Initialized = true;
        }

        public void Play(float audioVolume)
        {
            if(!m_Initialized)
            {
                Debug.LogError("Trying to play a surface effect, but it's not initialized!");
                return;
            }

            if(m_Audio != null)
                m_Audio.Play(m_AudioSource, audioVolume);

            for(int i = 0;i < m_Particles.Length;i++)
                m_Particles[i].Play();
        }
    }
}