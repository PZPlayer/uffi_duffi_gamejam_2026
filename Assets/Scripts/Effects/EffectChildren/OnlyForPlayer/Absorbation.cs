using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class Absorbation : IdleEffect, IPassive
    {
        [SerializeField] private LayerMask _layers;
        [SerializeField] private StunEffect _stun;
        [SerializeField] private GameObject _effect;
        private Health healthSystem;
        private float activateHealth;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            healthSystem = GetComponent<Health>();
            healthSystem.OnDamaged += OnChangeHealth;

            _startTime = -EffectInfo.ReloadTime;
        }

        private void OnChangeHealth(GameObject owner)
        {
            activateHealth = healthSystem.MaxHealth * 0.4f;

            if (healthSystem.CurHealth <= activateHealth)
            {
                TryToActivate();
            }
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 - ((Time.time - _startTime) / EffectInfo.ReloadTime));
        }

        private void TryToActivate()
        {
            if (Time.time - _startTime < EffectInfo.ReloadTime)
            {
                return;
            }

            float consumedHealth = 0;

            Destroy(Instantiate(_effect, transform), 3);

            _startTime = Time.time;
            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 );
            Collider[] colliders = Physics.OverlapSphere(transform.position, EffectInfo.ContinueTime, _layers);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;

                if (collider.TryGetComponent(out Health health))
                {
                    consumedHealth += EffectInfo.Damage;
                    health.Damage(EffectInfo.Damage);
                }

                if (collider.TryGetComponent(out EffectHandler handler))
                {
                    handler.AddEffect(_stun);
                }
            }

            healthSystem.Heal(consumedHealth);
        }
    }
}
