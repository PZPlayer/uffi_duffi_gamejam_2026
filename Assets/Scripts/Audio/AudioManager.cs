using UnityEngine;
using System.Collections.Generic;

namespace Jam.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager SOUND_MANAGER;

        [SerializeField] private AudioSource _fxAudioSource;
        [SerializeField] private AudioSource _melodiesAudioSource;

        [SerializeField] private float mainVolume = 1.0f;
        [SerializeField] private float melodieVolume = 0.5f;
        [SerializeField] private float effectsVolume = 0.5f;

        private List<AudioSource> _effectsAudioSources = new List<AudioSource>();
        private List<AudioSource> _melodiesAudioSources = new List<AudioSource>();

        

        private void Awake()
        {
            if (SOUND_MANAGER == null)
            {
                SOUND_MANAGER = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(this);
        }

        private void OnValidate()
        {
            mainVolume = Mathf.Clamp01(mainVolume);
            melodieVolume = Mathf.Clamp01(melodieVolume);
            effectsVolume = Mathf.Clamp01(effectsVolume);

            UpdateVolume();
        }

        public void AddSelfToEffectsList(AudioSource source)
        {
            if (_effectsAudioSources.Contains(source)) return;

            _effectsAudioSources.Add(source);
        }

        public void AddSelfToMelodiesList(AudioSource source)
        {
            if (_melodiesAudioSources.Contains(source)) return;

            _melodiesAudioSources.Add(source);
        }

        public void RemoveSelfFromEffectsList(AudioSource source)
        {
            if (!_effectsAudioSources.Contains(source)) return;

            _effectsAudioSources.Remove(source);
        }

        public void RemoveSelfFromMelodiesList(AudioSource source)
        {
            if (!_melodiesAudioSources.Contains(source)) return;

            _melodiesAudioSources.Remove(source);
        }

        public void PlaySoundEffect(AudioClip clip)
        {
            _fxAudioSource.PlayOneShot(clip);
        }
        public void PlaySoundMeoldie(AudioClip clip)
        {
            _melodiesAudioSource.PlayOneShot(clip);
        }

        public void UpdateVolume()
        {
            _fxAudioSource.volume = mainVolume * effectsVolume;
            _melodiesAudioSource.volume = mainVolume * melodieVolume;

            foreach (AudioSource source in _effectsAudioSources)
            {
                source.volume = _fxAudioSource.volume;
            }

            foreach (AudioSource source in _melodiesAudioSources)
            {
                source.volume = _melodiesAudioSource.volume;
            }
        }

        public float MainVolume
        {
            get { return mainVolume; }
            set { mainVolume = Mathf.Clamp01(value); UpdateVolume(); }
        }

        public float MelodieVolume
        {
            get { return melodieVolume; }
            set { melodieVolume = Mathf.Clamp01(value); UpdateVolume(); }
        }

        public float EffectVolume
        {
            get { return effectsVolume; }
            set { effectsVolume = Mathf.Clamp01(value); UpdateVolume(); }
        }

    }
}
