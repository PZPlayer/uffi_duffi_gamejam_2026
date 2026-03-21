using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class SoulScream : IdleEffect, IActive, IPassive
    {
        [SerializeField] private StunEffect _effectStunEffect;
        [SerializeField] private GameObject _visualEffesct;
        [SerializeField] private LayerMask _layers;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            _startTime = -EffectInfo.ReloadTime;
        }

        public void OnActiveCall()
        {
            print("haveBeenActivated");
            if (Time.time - _startTime < EffectInfo.ReloadTime) return;

            Collider[] hittedColliders = Physics.OverlapSphere(transform.position, EffectInfo.ContinueTime, _layers);

            foreach (Collider collider in hittedColliders)
            {
                if (collider.gameObject == transform.gameObject) continue;

                if (collider.TryGetComponent(out Health health))
                {
                    if (collider.TryGetComponent(out EffectHandler handler))
                    {
                        handler.AddEffect(_effectStunEffect, JsonUtility.ToJson(_effectStunEffect));
                    }

                    health.Damage(EffectInfo.Damage);
                }
            }

            _startTime = Time.time;

            Destroy(Instantiate(_visualEffesct, transform.position, transform.rotation), 3);
        }
    }
}