using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
    public class EffectAudio : MonoBehaviour
    {
        [SerializeField]
        private string m_EffectName = string.Empty;

        [SerializeField]
        private SoundPlayer m_SoundPlayer = null;

        private static Dictionary<string, SoundPlayer> SOUND_PLAYERS = new Dictionary<string, SoundPlayer>();


        public void PlayAudio3D(float volume)
        {
            SOUND_PLAYERS[m_EffectName].PlayAtPosition(ItemSelection.Method.RandomExcludeLast, transform.position, volume);
        }

        public void PlayAudio2D(float volume)
        {
            SOUND_PLAYERS[m_EffectName].Play2D(ItemSelection.Method.RandomExcludeLast, volume);
        }

        private void Awake()
        {
            if(!SOUND_PLAYERS.ContainsKey(m_EffectName))
                SOUND_PLAYERS.Add(m_EffectName, m_SoundPlayer);
        }
    }
}