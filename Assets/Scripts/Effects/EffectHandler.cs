using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Jam.Effects
{
    public class EffectHandler : MonoBehaviour
    {
        public event Action<IdleEffect> OnAddEffect;

        [SerializeField] private List<IdleEffect> _startEffects;
        [SerializeField] private float _passiveEffectsTick = 0.25f; // base tick
        
        protected List<IdleEffect> currentEffects = new List<IdleEffect>();
        protected List<ICallable> callableEffects = new List<ICallable>();
        private List<IPassive> passiveEffects = new List<IPassive>();

        private Coroutine passiveEffectCycle;

        protected virtual void Start()
        {
            foreach (var effect in _startEffects)
            {
                AddEffect(effect);
            }

            passiveEffectCycle = StartCoroutine(PassiveEffectsCall());
        }

        public virtual bool AddEffect(IdleEffect effect, string jsonDatta = null)
        {
            // Проверка на существование такого же типа
            foreach (var existing in currentEffects)
            {
                if (existing.GetType() == effect.GetType())
                {
                    existing.Refresh(effect);
                    return false;
                }
            }

            IdleEffect newEffect = gameObject.AddComponent(effect.GetType()) as IdleEffect;
            string jsonData = jsonDatta;
            if (jsonData == null) jsonData = JsonUtility.ToJson(effect);
            JsonUtility.FromJsonOverwrite(jsonData, newEffect);

            if (CheckIfItsSuitable(newEffect))
            {
                Destroy(newEffect);
                return false;
            }

            if (newEffect == null)
            {
                print("Failed to create component!");
                return false;
            }

            newEffect.Initilize(this);
            currentEffects.Add(newEffect);
            ManageEffectStatus(newEffect);
            return true;
        }

        public virtual bool RemoveEffect(IdleEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("RemoveEffect called with null effect");
                return false;
            }

            // Убираем из всех списков
            if (effect is IPassive passive)
            {
                bool removed = passiveEffects.Remove(passive);
                print($"Removed from passiveEffects: {removed}");
            }

            if (effect is ICallable callable)
            {
                callableEffects.Remove(callable);
                callable.UnSubsribe();
            }

            bool removedFromCurrent = currentEffects.Remove(effect);
            Destroy(effect);
            return true;
        }

        protected virtual void ManageEffectStatus(IdleEffect effect)
        {
            if (effect is ICallable callable && !callableEffects.Contains(callable))
            {
                callableEffects.Add(callable);
            }

            if (effect is IPassive passive && !passiveEffects.Contains(passive))
            {
                passiveEffects.Add(passive);
            }

            OnAddEffect?.Invoke(effect);
        }

        private IEnumerator PassiveEffectsCall()
        {
            while (true)
            {
                if (passiveEffects.Count > 0)
                {
                    print(401 + "  " + passiveEffects);
                    for (int i = passiveEffects.Count - 1; i >= 0; i--)
                    {
                        var effect = passiveEffects[i];
                        if (effect == null)
                        {
                            passiveEffects.RemoveAt(i);
                            continue;
                        }

                        effect.OnPassiveUpdate();
                    }
                }

                yield return new WaitForSeconds(_passiveEffectsTick);
            }
        }

        protected virtual bool CheckIfItsSuitable(IdleEffect effect)
        {
            return effect.EffectInfo.IfOnlyForPlayer;
        }

        public virtual void ClearAll()
        {
            var effectsToRemove = new List<IdleEffect>(currentEffects);

            foreach (var effect in effectsToRemove)
            {
                RemoveEffect(effect);
            }

            callableEffects.Clear();
            passiveEffects.Clear();
        }

        protected virtual void OnDestroy()
        {
            ClearAll();
            StopCoroutine(passiveEffectCycle);
            passiveEffectCycle = null;
        }
    }
}
