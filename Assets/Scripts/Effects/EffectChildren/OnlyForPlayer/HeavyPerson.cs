using Jam.HealthSystem;
using StarterAssets;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class HeavyPerson : IdleEffect, IActive
    {
        [SerializeField] private GameObject _fractures;
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
            Destroy(Instantiate(_fractures, transform), 3f);
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