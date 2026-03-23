using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects
{
    public class Spikes : IdleEffect
    {
        private Health healthSystem;
        private GameObject hittingMeGameObj;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            healthSystem = GetComponent<Health>();
            healthSystem.OnDamaged += OnTakeDamage;
            healthSystem.HealthChanged += OnHealthChanged;
        }

        private void OnTakeDamage(GameObject target)
        {
            hittingMeGameObj = target;
        }

        private void OnHealthChanged(float damage)
        {
            if (damage < 0 && hittingMeGameObj != null)
            {
                hittingMeGameObj.GetComponent<Health>().Damage(Mathf.Abs(damage) * EffectInfo.Damage);
            }
        }
    }
}
