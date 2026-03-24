using UnityEngine;
using System.Collections.Generic;

namespace Jam.Effects
{
    [RequireComponent(typeof(EffectHandler))]
    public class RandomEffectAdder : MonoBehaviour
    {
        [SerializeField] private List<IdleEffect> _avaivableEffects;

        private EffectHandler effectHandler;

        private void Awake()
        {
            effectHandler = GetComponent<EffectHandler>();
        }

        private void Start()
        {
            AddRandomEffect();
        }

        public void AddRandomEffect()
        {
            if (_avaivableEffects.Count == 0) return;
            int cycleTimes = 0;
            bool newOne = false;

            while (newOne == false)
            {
                int randomNumber = Random.Range(0, _avaivableEffects.Count);

                cycleTimes++;
                newOne = effectHandler.AddEffect(_avaivableEffects[randomNumber]);

                if (cycleTimes > 5)
                {
                    break;
                }
            }
        }
    }
}
