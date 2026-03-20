using Jam.HealthSystem;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Jam.Effects.EffectChildren
{
    public class HeavyPerson : IdleEffect, ICallable
    {
        [SerializeField] private GameObject _fractures;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _radius;

        private FirstPersonController _controller;
        private Health health;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);
            health = GetComponent<Health>();
            _controller = GetComponent<FirstPersonController>();
            health.MaxHealth += _effectInfo.ContinueTime;
            health.Heal(health.MaxHealth);
            Subsribe();
        }

        public void OnFallCall()
        {
            GameObject f = Instantiate(_fractures, transform);
            f.transform.parent = null;
            Destroy(f, 3f);
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layerMask);
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out Health health))
                {
                    float t = Mathf.Abs(_controller._verticalVelocity) / Mathf.Abs(_controller._terminalVelocity);
                    float result = Mathf.Lerp(1f, 5f, t);

                    result = Mathf.Clamp(result, 1f, 5f);
                    float resultForDecal = Mathf.Clamp(result, 1f, 3f);
                    f.GetComponentInChildren<DecalProjector>().size *= resultForDecal;
                    health.Damage((int)(_effectInfo.Damage * result), transform.gameObject);
                }
            }
        }

        public void Subsribe()
        {
            _controller.OnLandAfterDoubleJump += OnFallCall;
        }

        public void UnSubsribe()
        {
            _controller.OnLandAfterDoubleJump -= OnFallCall;
        }

        protected override void OnDestroy()
        {
            UnSubsribe();
            print("Returning health" + (health.MaxHealth - _effectInfo.ContinueTime) + "  " + _effectInfo.ContinueTime);
            health.MaxHealth = (health.MaxHealth - _effectInfo.ContinueTime);
            health.Heal(health.MaxHealth);
            base.OnDestroy();
        }
    }
}