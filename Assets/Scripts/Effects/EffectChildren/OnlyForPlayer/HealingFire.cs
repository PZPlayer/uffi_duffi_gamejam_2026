using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class HealingFire : IdleEffect, IActive, IPassive
    {
        [SerializeField] private GameObject _partcls;

        private Health health;
        public bool IsHealing => _ready;
        private bool _ready = false;
        private bool _waitingForButton = true;
        private ParticleSystem _system;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);
            health = GetComponent<Health>();
            // Создаем систему один раз
            if (_partcls != null)
            {
                _system = Instantiate(_partcls, transform).GetComponent<ParticleSystem>();
                _system.Stop();
            }
        }

        public void OnActiveCall()
        {
            if (_waitingForButton)
            {
                _startTime = Time.time;
                _ready = true;
                _waitingForButton = false; // Важно переключить сразу, чтобы нельзя было "проспамить" вызов

                // Включаем частицы только один раз при активации
                if (_system != null) _system.Play();
            }
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (_waitingForButton)
            {
                _startTime = Time.time + 1f;
                if (Card != null) Card.OnPassiveUpdate(this, _startTime);
            }
            else if (_ready) // Состояние лечения
            {
                if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1);

                // Хил (срабатывает раз в тик корутины Handler'а)
                if (health != null) health.Heal(_effectInfo.Damage);

                // Окончание хила
                if (Time.time - _startTime >= _effectInfo.ContinueTime)
                {
                    if (_system != null) _system.Stop();
                    _ready = false;
                    _startTime = Time.time;
                }
            }
            else // Перезарядка
            {
                if (Time.time - _startTime > _effectInfo.ReloadTime)
                {
                    _waitingForButton = true;
                }
                else
                {
                    if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 - ((Time.time - _startTime) / _effectInfo.ReloadTime));
                }
            }
        }

        protected override void OnDestroy()
        {
            if (_system != null) Destroy(_system.gameObject);
            base.OnDestroy();
        }
    }
}