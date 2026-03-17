using System;
using UnityEngine;

namespace Jam.Effects
{
    public abstract class IdleEffect : MonoBehaviour
    {
        public event Action<IdleEffect> OnInitilizeAction;
        public event Action<IdleEffect, float> OnPassiveAction;
        public event Action<IdleEffect> OnDestroyAction;

        public int _times = 1;
        public EffectInfo EffectInfo { get => _effectInfo; }

        [SerializeField] protected EffectInfo _effectInfo;

        protected EffectHandler handler;
        protected float _startTime;

        public virtual void Initilize(EffectHandler handlerEffect)
        {
            handler = handlerEffect;
            OnInitilizeAction?.Invoke(this);
        }

        public virtual void OnPassiveUpdate()
        {
            OnPassiveAction?.Invoke(this, _startTime);
        }

        public virtual void Refresh(IdleEffect newEffectData)
        {

        }

        protected virtual void OnDestroy()
        {
            OnDestroyAction?.Invoke(this);
        }
    }
}
