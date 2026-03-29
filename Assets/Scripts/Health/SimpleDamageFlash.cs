using UnityEngine;
using System.Collections;

namespace Jam.HealthSystem
{
    public class SimpleDamageFlash : MonoBehaviour
    {
        [SerializeField] private Health _health;

        [Header("Flash Settings")]
        public Color _flashColor = Color.red;
        [SerializeField] private float _flashInDuration = 0.1f;
        [SerializeField] private float _flashOutDuration = 0.3f;
        [SerializeField] private float _holdTime = 0.1f;
        [SerializeField] private bool _flashWhenDamage = true;

        [SerializeField] private SkinnedMeshRenderer _renderer;
        private Material _material;
        private Color _originalColor;
        private Coroutine _currentFlashCoroutine;

        private void Awake()
        {
            _health.HealthChanged += Flash;
            if (_renderer == null) _renderer = GetComponent<SkinnedMeshRenderer>();
            _material = _renderer.material;
            _originalColor = _material.color;
        }

        public void Flash(float health)
        {
            if (health >= 0 && _flashWhenDamage) return;

            if (_currentFlashCoroutine != null)
            {
                StopCoroutine(_currentFlashCoroutine);
            }
            
            if (gameObject.activeInHierarchy) _currentFlashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float elapsedTime = 0f;

            // Плавно краснеем
            while (elapsedTime < _flashInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / _flashInDuration;
                _material.color = Color.Lerp(_originalColor, _flashColor, t);
                yield return null;
            }

            _material.color = _flashColor;
            yield return new WaitForSeconds(_holdTime);

            elapsedTime = 0f;

            // Плавно возвращаемся
            while (elapsedTime < _flashOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / _flashOutDuration;
                _material.color = Color.Lerp(_flashColor, _originalColor, t);
                yield return null;
            }

            _material.color = _originalColor;
            _currentFlashCoroutine = null;
        }

        private void OnDisable()
        {
            if (_material != null)
            {
                _material.color = _originalColor;
            }
        }
    }
}
