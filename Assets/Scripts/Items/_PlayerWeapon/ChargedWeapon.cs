using Jam.HealthSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class ChargedWeapon : Item, IUsable, ISecondUsable
    {
        [Header("Primary Attacks (Combo)")]
        [SerializeField] private List<AttackInfo> _attacks;
        [SerializeField] private float _comboResetTime = 2.0f;

        [Header("Secondary Attack (Hold)")]
        [SerializeField] private HoldAttackInfo _holdAttack;

        [Header("Components")]
        [SerializeField] private Animator _animator;

        private int _currentAttackIndex = 0;
        private float _currentDamage = 0;
        private float _lastAttackTime = -999f;

        private bool _isPrimaryPressed = false;
        private bool _isSecondaryPressed = false;
        private bool _isBusy = false;

        private Coroutine _primaryRoutine;
        private Coroutine _secondaryRoutine;
        private Coroutine _secondaryLoopCallTry;

        #region Primary Attack (IUsable)
        public void Use(InputValue value = null)
        {
            if (value != null) _isPrimaryPressed = value.isPressed;

            // Запускаем цикл обычных атак, если нажато и мы не заняты тяжёлой атакой
            if (_isPrimaryPressed && !_isBusy && _primaryRoutine == null)
            {
                _primaryRoutine = StartCoroutine(PrimaryAttackLoop());
            }
        }

        private IEnumerator PrimaryAttackLoop()
        {
            while (_isPrimaryPressed)
            {
                // Сброс комбо по времени
                if (Time.time - _lastAttackTime > _comboResetTime)
                    _currentAttackIndex = 0;

                if (_attacks == null || _attacks.Count == 0) yield break;

                // Выполняем одну атаку из списка
                yield return StartCoroutine(PerformNormalAttack());
            }
            _primaryRoutine = null;
        }

        private IEnumerator PerformNormalAttack()
        {
            _isBusy = true;
            AttackInfo current = _attacks[_currentAttackIndex];

            _currentDamage = current.Damage;
            _lastAttackTime = Time.time;

            if (_animator != null && current.AttackAnimation != null)
                _animator.Play(current.AttackAnimation.name);

            // Ждем время перезарядки из AttackInfo
            yield return new WaitForSeconds(current.ReloadTime);

            _currentAttackIndex = (_currentAttackIndex + 1) % _attacks.Count;
            _isBusy = false;
        }
        #endregion

        #region Secondary Attack (ISecondUsable)
        public void UseSecond(InputValue value = null)
        {
            if (value != null) _isSecondaryPressed = value.isPressed;

            // Запускаем зарядку, если нажато и мы ничем не заняты
            if (_isSecondaryPressed)
            {
                if (!_isBusy && _secondaryRoutine == null)
                {
                    _secondaryRoutine = StartCoroutine(HoldAttackRoutine());
                }
                else
                {
                    if (_secondaryLoopCallTry == null)
                        _secondaryLoopCallTry = StartCoroutine(TryCallSecondAttack());
                }
            }
        }

        private IEnumerator TryCallSecondAttack()
        {
            while (_isSecondaryPressed)
            {
                if (!_isBusy && _secondaryRoutine == null)
                {
                    _secondaryRoutine = StartCoroutine(HoldAttackRoutine());
                    break;
                }

                yield return null;
            }

            _secondaryLoopCallTry = null;
        }

        private IEnumerator HoldAttackRoutine()
        {
            _isBusy = true;

            // 1. Анимация начала замаха
            if (_holdAttack.StartClip != null)
            {
                _animator.Play(_holdAttack.StartClip.name);
                yield return new WaitForSeconds(_holdAttack.StartClip.length);
            }

            // 2. Анимация ожидания (Loop) и накопление заряда
            float chargeStartTime = Time.time;
            if (_holdAttack.LoopClip != null)
                _animator.Play(_holdAttack.LoopClip.name);

            // Ждем, пока игрок отпустит кнопку (состояние пришло через UseSecond)
            while (_isSecondaryPressed)
            {
                yield return null;
            }

            // 3. Расчет урона
            // Линейная зависимость: урон растет от Min до Max за время ChargeTime
            float holdDuration = Time.time - chargeStartTime;
            float chargeFactor = Mathf.Clamp01(holdDuration / _holdAttack.ChargeTime);
            _currentDamage = Mathf.Lerp(_holdAttack.MinDamage, _holdAttack.MaxDamage, chargeFactor);

            // 4. Анимация самого удара (Release)
            if (_holdAttack.EndClip != null)
            {
                _animator.Play(_holdAttack.EndClip.name);
                yield return new WaitForSeconds(_holdAttack.EndClip.length);
            }

            // Небольшая задержка перед следующей возможностью атаковать
            yield return new WaitForSeconds(_holdAttack.PostAttackDelay);

            _isBusy = false;
            _secondaryRoutine = null;
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (!_isBusy) return;
            if (!other.TryGetComponent<Health>(out var targetHealth)) return;
            if (IsFriendlyFire(other)) return;

            targetHealth.Damage(_currentDamage, Owner);
        }

        private bool IsFriendlyFire(Collider other)
        {
            if (Owner == null) return false;
            return other.transform.root.gameObject == Owner.transform.root.gameObject ||
                   other.gameObject.layer == Owner.layer ||
                   other.CompareTag(Owner.tag);
        }

        private void OnDisable()
        {
            // Прерываем всё при выключении объекта
            if (_primaryRoutine != null) StopCoroutine(_primaryRoutine);
            if (_secondaryRoutine != null) StopCoroutine(_secondaryRoutine);

            _primaryRoutine = null;
            _secondaryRoutine = null;
            _isBusy = false;
        }
    }
}