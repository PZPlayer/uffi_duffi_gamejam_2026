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

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 - ((Time.time - _startTime) / _effectInfo.ReloadTime));
        }

        public void OnActiveCall()
        {
            try
            {
                if (Time.time - _startTime < EffectInfo.ReloadTime) return;

                Collider[] hittedColliders = Physics.OverlapSphere(transform.position, EffectInfo.ContinueTime, _layers);

                foreach (Collider collider in hittedColliders)
                {
                    if (collider.gameObject == transform.gameObject) continue;

                    if (collider.TryGetComponent(out Health health))
                    {
                        if (collider.TryGetComponent(out EffectHandler handler))
                        {
                            if (handler == gameObject.GetComponent<EffectHandler>()) continue;
                            handler.AddEffect(_effectStunEffect, JsonUtility.ToJson(_effectStunEffect));
                            handler.AddEffect(_effectStunEffect, JsonUtility.ToJson(_effectStunEffect));
                        }

                        health.Damage(EffectInfo.Damage, gameObject);
                    }
                }

                _startTime = Time.time;

                Destroy(Instantiate(_visualEffesct, transform.position, transform.rotation), 3);
            }
            catch
            {
                Debug.Log("I alredy died");
            }
            
        }
    }
}