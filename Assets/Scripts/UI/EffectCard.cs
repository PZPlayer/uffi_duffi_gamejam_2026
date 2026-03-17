using Jam.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    public class EffectCard : MonoBehaviour
    {
        [SerializeField] private Image _cardEffectImage;
        [SerializeField] private Image _cardCooldown;
        [SerializeField] private TextMeshProUGUI _cardTimesText;
        private EffectVisualiator effectVisualiator;

        public void Init(IdleEffect effect, EffectVisualiator visual)
        {
            effectVisualiator = visual; 

            if (effect.TryGetComponent(out IPassive passive)) effect.OnPassiveAction += OnPassiveUpdate;
            _cardEffectImage.sprite = effect.EffectInfo.EffectImage;
            _cardCooldown.sprite = _cardEffectImage.sprite;
            effect.OnInitilizeAction += OnEffectInitiliaze;
            effect.OnDestroyAction += OnEffectDestroy;
        }

        private void OnEffectInitiliaze(IdleEffect effect)
        {
            _cardEffectImage.sprite = effect.EffectInfo.EffectImage;
            _cardTimesText.text = effect._times != 1 ? effect._times.ToString() : " ";
        }

        private void OnEffectDestroy(IdleEffect effect)
        {
            effect.OnPassiveAction -= OnPassiveUpdate;
            effect.OnInitilizeAction -= OnEffectInitiliaze;
            effect.OnDestroyAction -= OnEffectDestroy;
            effectVisualiator.ExpellEffect(effect);
            Destroy(gameObject);
        }

        private void OnPassiveUpdate(IdleEffect effect, float startTime)
        {
            _cardCooldown.fillAmount = (Time.time - startTime) / effect.EffectInfo.ContinueTime;
            _cardTimesText.text = effect._times != 1 ? effect._times.ToString() : " ";
        }
    }
}