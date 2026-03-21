using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

namespace Jam.Effects
{
    public class PlayerEffectHandler : EffectHandler
    {
        [SerializeField] private StarterAssetsInputs _input;
        [SerializeField] private int _maxSize;

        private List<IdleEffect> activeEffects = new List<IdleEffect>();

        protected override void Start()
        {
            base.Start();
            _input.OnActiveEffect += ActivateActiveEffects;
        }

        private void ActivateActiveEffects()
        {
            foreach (var effect in activeEffects)
            {
                effect.GetComponent<IActive>().OnActiveCall();
                print("2323" + effect);
            }
        }

        public override bool AddEffect(IdleEffect effect, string jsonDatta = null)
        {
            if (currentEffects.Count >= _maxSize) return false;
            return base.AddEffect(effect);
        }

        public override bool RemoveEffect(IdleEffect effect)
        {
            if (effect.TryGetComponent(out IActive active)) activeEffects.Remove(effect);
            return base.RemoveEffect(effect);
        }

        protected override void ManageEffectStatus(IdleEffect effect)
        {
            if (effect.TryGetComponent(out IActive active))
            {
                print("222" + effect.ToString());
                if (activeEffects.Contains(effect)) return;
                print("221" + effect.ToString());
                activeEffects.Add(effect);
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
