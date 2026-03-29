using Jam.HealthSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class Stick : Item, IUsable
    {
        [Header("Attacks")]
        [SerializeField] protected List<AttackInfo> _attacks;

        [Header("Components")]
        [SerializeField] protected Animator _animator;

        [Header("Settings")]
        [SerializeField] private float _comboResetTime = 2.0f;

        private int _currentAttackIndex = 0;
        private float _currentDamage = 0;
        private float _lastAttackTime = -999f;
        private bool _isPressed = false;

        public bool IsAttacking => _isAttacking;
        private bool _isAttacking = false;
        private Coroutine _attackCoroutine;
        private Coroutine _loopCorutine;

        public void SetPressed(bool pressed)
        {
            _isPressed = pressed;
            if (_isPressed && _loopCorutine == null)
            {
                _loopCorutine = StartCoroutine(LoopWhilePressed());
            }
        }

        public void Use(InputValue value = null)
        {
            if (value != null)
            {
                SetPressed(value.isPressed);
            }
            else
            {
                if (!_isPressed) StartCoroutine(SingleUseRoutine());
            }
        }

        private IEnumerator SingleUseRoutine()
        {
            _isPressed = true;
            if (_loopCorutine == null) _loopCorutine = StartCoroutine(LoopWhilePressed());
            yield return new WaitForSeconds(0.1f); // Имитируем короткое нажатие
            _isPressed = false;
        }

        private IEnumerator LoopWhilePressed()
        {
            while (_isPressed)
            {
                if (Time.time - _lastAttackTime > _comboResetTime)
                {
                    _currentAttackIndex = 0;
                }

                if (_attacks == null || _attacks.Count == 0) yield break;


                _isPressed = false;

                yield return PerformAttack();
                yield return null;
            }

            _loopCorutine = null;
        }

        private IEnumerator PerformAttack()
        {
            _isAttacking = true;

            AttackInfo currentAttack = _attacks[_currentAttackIndex];
            _currentDamage = currentAttack.Damage;

            _lastAttackTime = Time.time;

            if (_animator != null && currentAttack.AttackAnimation != null)
            {
                _animator.Play(currentAttack.AttackAnimation.name);
            }

            yield return new WaitForSeconds(currentAttack.ReloadTime);

            _currentAttackIndex++;

            if (_currentAttackIndex >= _attacks.Count)
            {
                _currentAttackIndex = 0;
            }

            _isAttacking = false;
        }

        public void ResetCombo()
        {
            _currentAttackIndex = 0;
            _lastAttackTime = -999f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isAttacking) return;
            if (!other.TryGetComponent<Health>(out var targetHealth)) return;
            if (IsFriendlyFire(other)) return;

            targetHealth.Damage(_currentDamage, Owner);
        }

        private bool IsFriendlyFire(Collider other)
        {
            if (Owner == null) return false;

            if (other.transform.root.gameObject == Owner.transform.root.gameObject)
                return true;

            if (other.gameObject.layer == Owner.layer)
                return true;

            if (other.CompareTag(Owner.tag))
                return true;

            return false;
        }

        private void OnDisable()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
            _isAttacking = false;
        }
    }
}