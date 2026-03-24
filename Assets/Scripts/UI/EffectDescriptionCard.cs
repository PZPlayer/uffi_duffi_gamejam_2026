using Jam.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    public class EffectDescriptionCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _button;

        private IdleEffect _effect;
        private EffectPickManager _pickManager;

        public void Initialize(IdleEffect effect, EffectPickManager pickManager)
        {
            _effect = effect;
            _pickManager = pickManager;

            if (effect != null && effect.EffectInfo != null)
            {
                _titleText.text = effect.EffectInfo.EffectName;
                _typeText.text = "“ËÔ: " + effect.EffectInfo.EffectStatus;
                _descriptionText.text = effect.EffectInfo.EffectDescription;
                if (_iconImage != null && effect.EffectInfo.EffectImage != null)
                    _iconImage.sprite = effect.EffectInfo.EffectImage;
            }

            _button.onClick.AddListener(Pick);
        }

        public void Pick()
        {
            _pickManager.PickedVariantSend(_effect);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Pick);
        }
    }
}