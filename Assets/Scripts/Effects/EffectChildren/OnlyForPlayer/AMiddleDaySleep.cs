using Jam.HealthSystem;
using StarterAssets;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class AMiddleDaySleep : IdleEffect, IPassive
    {
        [SerializeField] private GameObject _effect;

        private StarterAssetsInputs _input;
        private Health healthSystem;
        private float lastTimeStopped;
        private bool isStopped;
        private GameObject effect;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            healthSystem = GetComponent<Health>();
            _input = GetComponent<StarterAssetsInputs>();
            healthSystem.OnDamaged += OnReset;
            healthSystem.MaxHealth = healthSystem.MaxHealth + EffectInfo.ContinueTime;
            healthSystem.Heal(healthSystem.MaxHealth);
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (_input.move == Vector2.zero)
            {
                if (Card != null) Card.OnPassiveUpdate(this, _startTime, 0.5f);
                isStopped = true;
            }
            else
            {
                if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1);
                
                OnReset();
                isStopped = false;
            }

            CheckIfCanHeal();
        }

        private void OnReset(GameObject owner = null)
        {
            lastTimeStopped = Time.time;
            if (effect != null) Destroy(effect);
        }

        private void CheckIfCanHeal()
        {
            if (!isStopped) return;
            if ((Time.time - lastTimeStopped) < EffectInfo.ReloadTime)
            {
                if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 - ((Time.time - lastTimeStopped) / EffectInfo.ReloadTime));
                return;
            }

            if (effect == null) effect = Instantiate(_effect, transform);
            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 0);
            healthSystem.Heal(EffectInfo.Damage);
        }

        protected override void OnDestroy()
        {
            healthSystem.MaxHealth = healthSystem.MaxHealth - EffectInfo.ContinueTime;

            base.OnDestroy();
        }
    }
}
