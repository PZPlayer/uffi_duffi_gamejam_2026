using Jam.HealthSystem;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Jam.Effects.EffectChildren
{
    public class HeavyPerson : IdleEffect, IActive
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
            _controller.OnLandAfterDoubleJump += OnActiveCall;
            health.MaxHealth += _effectInfo.ContinueTime;
            health.CurHealth += _effectInfo.ContinueTime;
        }

        public void OnActiveCall()
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
                    health.Damage((int)(_effectInfo.Damage * result));
                }
            }
        }

        protected override void OnDestroy()
        {
            _controller.OnLandAfterDoubleJump -= OnActiveCall;
            health.MaxHealth -= _effectInfo.ContinueTime;
            health.CurHealth -= _effectInfo.ContinueTime;
            base.OnDestroy();
        }
    }
}