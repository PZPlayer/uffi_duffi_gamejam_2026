using Jam.HealthSystem;
using Jam.Movement;
using UnityEngine;

namespace Jam.Effects.EffectChildren.OnlyForPlayer
{
    public class PoisonousDash : IdleEffect, ICallable
    {
        [SerializeField] private GameObject _partcl;
        [SerializeField] private IdleEffect _poisonEffect;
        [SerializeField] private LayerMask _layers;
        [SerializeField] private float _radius = 5;
        private PlayerDashController dash;
        private float originalCooldown;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);
            dash = GetComponent<PlayerDashController>();
            originalCooldown = dash.CooldownTime;
            Subsribe();
        }

        public void Subsribe()
        {
            dash.OnDash += DashAddition;
            dash.CooldownTime = _effectInfo.ReloadTime;
        }

        private void DashAddition()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layers);
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<Health>() && collider.TryGetComponent(out EffectHandler handlerr))
                {
                    collider.GetComponent<Health>().Damage(0, transform.gameObject);
                    handlerr.AddEffect(_poisonEffect, JsonUtility.ToJson(_poisonEffect));
                }
            }

            Destroy(Instantiate(_partcl, transform.position, transform.rotation), 3);
        }

        public void UnSubsribe()
        {
            dash.CooldownTime = originalCooldown;
            dash.OnDash -= DashAddition;
        }

        protected override void OnDestroy()
        {
            UnSubsribe();
            base.OnDestroy();
        }
    }
}
