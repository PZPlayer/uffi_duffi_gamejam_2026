using UnityEngine;

namespace Jam.Audio
{
    public class IdleAudioBehavior : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private float _smallDelay;

        private AudioManager manager;
        private AudioSource _oneShotAudio;
        private float _lastTimePlay;

        private void Awake()
        {
            if (_source == null) _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                enabled = false;
                return;
            }

            _lastTimePlay = -_smallDelay;
        }

        private void Start()
        {
            manager = AudioManager.SOUND_MANAGER;

            if (manager != null)
            {
                manager.AddSelfToEffectsList(_source);
            }
            else
            {
                enabled = false;
                return;
            }

            // Создаем отдельный AudioSource для одноразовых звуков
            _oneShotAudio = gameObject.AddComponent<AudioSource>();
            _oneShotAudio.playOnAwake = false;
            _oneShotAudio.volume = _source.volume;
            _oneShotAudio.pitch = _source.pitch;
            _oneShotAudio.outputAudioMixerGroup = _source.outputAudioMixerGroup;
        }

        public void PlayAfterASmallCoolDown(AudioClip clip)
        {
            if (clip == null) return;
            if (Time.time - _lastTimePlay < _smallDelay) return;

            _oneShotAudio.volume = _source.volume;

            _lastTimePlay = Time.time;
            _oneShotAudio.PlayOneShot(clip);
        }

        public void PlayEffect(bool continuePlaying)
        {

            if (continuePlaying)
            {
                if (!_source.isPlaying)
                    _source?.Play();
            }
            else
            {
                _source?.Pause();
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (manager != null)
                    manager.RemoveSelfFromEffectsList(_source);
            }
            catch { }
        }
    }
}