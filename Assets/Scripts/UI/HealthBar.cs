using System.Collections;
using Jam.HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _healthChangeImage;
        [SerializeField] private TextMeshProUGUI _healthMaxCur;
        [SerializeField] private Health _health;
        [SerializeField] private float _changeSpeed;

        private Coroutine _changeRoutine;
        private float _targetFill;

        private void Start()
        {
            _health.HealthChanged += OnChangeHealth;
            _targetFill = _health.CurHealth / _health.MaxHealth;
            _healthChangeImage.fillAmount = _targetFill;
            _healthMaxCur.text = _health.CurHealth + "/" + _health.MaxHealth;
        }

        private void OnChangeHealth(float value)
        {
            _targetFill = _health.CurHealth / _health.MaxHealth;

            if (_changeRoutine != null)
                StopCoroutine(_changeRoutine);

            _changeRoutine = StartCoroutine(ChangeHealthBar());
        }

        private IEnumerator ChangeHealthBar()
        {
            float startFill = _healthChangeImage.fillAmount;
            float elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * _changeSpeed;
                _healthChangeImage.fillAmount = Mathf.Lerp(startFill, _targetFill, elapsed);
                _healthMaxCur.text = _health.CurHealth + "/" + _health.MaxHealth;
                yield return null;
            }

            _healthChangeImage.fillAmount = _targetFill;
            _changeRoutine = null;
        }

        private void OnDestroy()
        {
            _health.HealthChanged -= OnChangeHealth;
        }
    }
}