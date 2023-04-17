using TestAudioMix.UnityEngine.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace TestAudioMix
{
    public class TestAudioMixMgr : MonoBehaviour
    {
        public AudioMixer audioMixer; // 进行控制的Mixer变量

        [SerializeField] private Slider m_MasterVolumeSlider = null;
        [SerializeField] private Slider m_BGMVolumeSlider = null;
        [SerializeField] private Slider m_EffectVolumeSlider = null;

        private void Start()
        {
            m_MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            m_BGMVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            m_EffectVolumeSlider.onValueChanged.AddListener(SetSoundEffectVolume);
        }

        public void SetMasterVolume(float val) // 控制主音量的函数
        {
            audioMixer.SetVolume("MasterVolume", val);
        }

        public void SetMusicVolume(float volume) // 控制背景音乐音量的函数
        {
            audioMixer.SetVolume("BGMVolume", volume);
        }

        public void SetSoundEffectVolume(float volume) // 控制音效音量的函数
        {
            audioMixer.SetVolume("EffectVolume", volume);
        }
    }
    namespace UnityEngine.Audio
    {
        public static class AudioExtensions
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="exposedName">The name of 'The Exposed to Script' variable</param>
            /// <param name="value">value must be between 0 and 1</param>
            public static void SetVolume(this AudioMixer mixer, string exposedName, float value)
            {
                mixer.SetFloat(exposedName, Mathf.Lerp(-80.0f, 0.0f, Mathf.Clamp01(value)));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="exposedName">The name of 'The Exposed to Script' variable</param>
            /// <returns></returns>
            public static float GetVolume(this AudioMixer mixer, string exposedName)
            {
                if (mixer.GetFloat(exposedName, out float volume))
                {
                    return Mathf.InverseLerp(-80.0f, 0.0f, volume);
                }

                return 0f;
            }
        }
    }
}

