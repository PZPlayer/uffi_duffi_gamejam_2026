using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren.OnlyForPlayer
{
    public class PoisonousDash : IdleEffect, ICallable
    {
        [SerializeField] private GameObject _partcl;
        [SerializeField] private IdleEffect _poisonEffect;
        [SerializeField] private LayerMask _layers;
        [SerializeField] private float _radius = 5;
        [SerializeField] private float _newDashCoolDown = 5;
        private Dash dash;
        private float originalCooldown;

        public void Subsribe()
        {
            dash = GetComponent<Dash>();
            dash.OnDash += DashAddition;
            originalCooldown = dash._dashCooldown;
            dash._dashCooldown = _newDashCoolDown;
        }

        private void DashAddition()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layers);
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<Health>() && collider.TryGetComponent(out EffectHandler handlerr))
                {
                    handlerr.AddEffect(_poisonEffect, JsonUtility.ToJson(_poisonEffect));
                }
            }

            Destroy(Instantiate(_partcl, transform.position, transform.rotation), 3);
        }

        public void UnSubsribe()
        {
            dash._dashCooldown = originalCooldown;
            dash.OnDash -= DashAddition;
        }
    }
}
