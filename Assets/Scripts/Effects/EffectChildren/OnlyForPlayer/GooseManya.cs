using Jam.HealthSystem;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class GooseManya : IdleEffect, IActive, IPassive
    {
        [SerializeField] private GameObject _goose;
        [SerializeField] private GameObject _effect;
        [SerializeField] private LayerMask _layers;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            _startTime = -EffectInfo.ReloadTime;
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1 - ((Time.time - _startTime) / EffectInfo.ReloadTime));
        }

        public void OnActiveCall()
        {
            if (Time.time - _startTime < EffectInfo.ReloadTime)
            {
                return;
            }

            try
            {
                Destroy(Instantiate(_effect, transform), 3);
            }
            catch { }
            

            _startTime = Time.time;
            if (Card != null) Card.OnPassiveUpdate(this, _startTime, 1);
            Collider[] colliders = Physics.OverlapSphere(transform.position, EffectInfo.ContinueTime, _layers);
            
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;

                if (collider.TryGetComponent(out Health health))
                {
                    health.Damage(health.MaxHealth);
                    Destroy(Instantiate(_goose, collider.transform.position, transform.rotation), 50);
                }
            }
        }
    }
}
