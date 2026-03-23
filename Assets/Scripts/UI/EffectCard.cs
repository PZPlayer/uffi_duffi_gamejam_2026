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

        private IdleEffect _mineEffect;
        private EffectVisualiator effectVisualiator;

        public void Init(IdleEffect effect, EffectVisualiator visual)
        {
            effectVisualiator = visual;
            _mineEffect = effect;

            if (effect.TryGetComponent(out IPassive passive)) effect.Card = this;
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
            effect.OnInitilizeAction -= OnEffectInitiliaze;
            effect.OnDestroyAction -= OnEffectDestroy;
            effectVisualiator.ExpellEffect(effect);
            try
            {
                Destroy(gameObject);
            }
            catch { }  
        }

        public void OnPassiveUpdate(IdleEffect effect, float startTime, float coolDown = 0)
        {
            if (effect != _mineEffect) return;

            _cardCooldown.fillAmount = coolDown;
            _cardTimesText.text = effect._times != 1 ? effect._times.ToString() : " ";
            if (effect.TryGetComponent(out IActive active))
            {
                _cardTimesText.text = effect.EffectInfo.EffectCallKey; 
            }
        }
    }
}