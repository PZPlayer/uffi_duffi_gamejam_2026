using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

namespace Jam.Effects
{
    public class PlayerEffectHandler : EffectHandler
    {
        [SerializeField] private StarterAssetsInputs _input;
        [SerializeField] private int _maxSize;

        private List<IActive> activeEffects = new List<IActive>();

        protected override void Start()
        {
            base.Start();
            _input.OnActiveEffect += ActivateActiveEffects;
        }

        private void ActivateActiveEffects()
        {
            foreach (var effect in activeEffects)
            {
                effect.OnActiveCall();
            }
        }

        public override bool AddEffect(IdleEffect effect, string jsonDatta = null)
        {
            if (currentEffects.Count >= _maxSize) return false;
            return base.AddEffect(effect);
        }

        public override bool RemoveEffect(IdleEffect effect)
        {
            if (effect.TryGetComponent(out IActive active)) activeEffects.Remove(active);
            return base.RemoveEffect(effect);
        }

        protected override void ManageEffectStatus(IdleEffect effect)
        {
            if (effect.TryGetComponent(out IActive active))
            {
                activeEffects.Add(active);
            }

            base.ManageEffectStatus(effect);
        }

        protected override bool CheckIfItsSuitable(IdleEffect effect)
        {
            return false;
        }

        protected override void OnDestroy()
        {
            _input.OnActiveEffect -= ActivateActiveEffects;
            base.OnDestroy();
        }
    }
}
