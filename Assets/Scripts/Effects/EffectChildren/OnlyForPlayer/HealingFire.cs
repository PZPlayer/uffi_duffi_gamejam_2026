using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class HealingFire : IdleEffect, IActive, IPassive
    {
        [SerializeField] private GameObject _partcls;

        private Health health;
        private bool _ready = false;
        private bool _waitingForButton = true;
        private ParticleSystem _system;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);
            health = GetComponent<Health>();
            _system = Instantiate(_partcls, transform).GetComponent<ParticleSystem>();
        }

        public void OnActiveCall()
        {
            print("Pressed");
            if (_waitingForButton) // Проверка если либо таймер перезарядки прошел или игра токо запустилась
            {
                _startTime = Time.time;
                _ready = true;
            }
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (_waitingForButton)
            {
                _startTime = Time.time + 1f; // чтобы иконка эффекта была полностью светлой из-за строчи _startTime/Time.time = fillAmount
            }

            if (_ready)
            {
                _waitingForButton = false;
                _system.Play();
                health.Heal(_effectInfo.Damage);

                if (Time.time - _startTime >= _effectInfo.ContinueTime)
                {
                    _system.Stop();
                    _ready = false;
                    _startTime = Time.time;
                }
            }
            else
            {
                if (Time.time - _startTime > _effectInfo.ReloadTime)
                {
                    _waitingForButton = true;
                }
            }
        }

        protected override void OnDestroy()
        {
            Destroy(_system.gameObject);
            base.OnDestroy();
        }
    }
}
