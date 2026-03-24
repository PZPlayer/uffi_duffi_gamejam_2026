using System.Collections.Generic;
using UnityEngine;

namespace Jam.Effects
{
    public class NPCEffectHandler : EffectHandler
    {
        // Список активных эффектов, которые NPC может "нажать"
        private List<IActive> activeEffects = new List<IActive>();

        // Метод для ИИ: Босс вызывает это, когда хочет поджечь хил
        public void TriggerActiveEffects()
        {
            foreach (var effect in activeEffects)
            {
                effect.OnActiveCall();
            }
        }

        protected override void ManageEffectStatus(IdleEffect effect)
        {
            // Если эффект умеет активироваться, запоминаем его
            if (effect is IActive active)
            {
                if (!activeEffects.Contains(active))
                    activeEffects.Add(active);
            }

            base.ManageEffectStatus(effect);
        }

        public override bool RemoveEffect(IdleEffect effect)
        {
            if (effect is IActive active) activeEffects.Remove(active);
            return base.RemoveEffect(effect);
        }

        protected override bool CheckIfItsSuitable(IdleEffect effect)
        {
            // NPC может носить всё, что не помечено "OnlyForPlayer"
            // Но если мы хотим, чтобы Босс игнорировал это правило — возвращаем false
            return false;
        }
    }
}