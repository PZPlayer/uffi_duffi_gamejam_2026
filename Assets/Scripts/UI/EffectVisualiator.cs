using Jam.Effects;
using UnityEngine;
using System.Collections.Generic;

namespace Jam.UI
{
    public class EffectVisualiator : MonoBehaviour
    {
        [SerializeField] private EffectHandler _effectHandler;
        [SerializeField] private GameObject _effectPrefab;
        [SerializeField] private Transform _effectSpawner;

        private List<IdleEffect> visualisations = new List<IdleEffect>();

        private void Awake()
        {
            _effectHandler.OnAddEffect += EffectAddition;
        }

        private void EffectAddition(IdleEffect effect)
        {
            if (visualisations.Contains(effect)) return; 

            GameObject NewVisualisation = Instantiate(_effectPrefab, _effectSpawner);
            visualisations.Add(effect);
            NewVisualisation.GetComponent<EffectCard>().Init(effect, this);
        }

        public void ExpellEffect(IdleEffect effect)
        {
            visualisations.Remove(effect);
        }

        private void OnDestroy()
        {
            _effectHandler.OnAddEffect -= EffectAddition;
        }
    }
}
