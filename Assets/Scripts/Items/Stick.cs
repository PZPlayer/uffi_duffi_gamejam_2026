using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Jam.HealthSystem;
using StarterAssets;

namespace Jam.Items
{
    public class Stick : Item, IUsable
    {
        [Header("GunAttacks")]
        [SerializeField] protected List<AttackInfo> _attacks;

        [Header("Components")]
        [SerializeField] protected Animator _animator;

        [Header("Settings")]
        [SerializeField] private float _comboResetTime = 2.0f;

        private int _currentAttackIndex = 0;
        private float currentDamage = 0;
        private float _lastAttackTime = -999f;
        private bool _isAttacking = false;
        private Coroutine _attackCoroutine;

        public void Use()
        {
            if (_isAttacking) return;

            if (_attacks == null || _attacks.Count == 0)
            {
                Debug.LogWarning("No attacks assigned to Stick!");
                return;
            }

            float timeSinceLastAttack = Time.time - _lastAttackTime;
            if (timeSinceLastAttack > _comboResetTime)
            {
                _currentAttackIndex = 0;
            }

            _attackCoroutine = StartCoroutine(PerformAttack());
        }

        private IEnumerator PerformAttack()
        {
            _isAttacking = true;

            AttackInfo currentAttack = _attacks[_currentAttackIndex];
            currentDamage = currentAttack.Damage;

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
            if (other.TryGetComponent<Health>(out Health health) && !health.gameObject.GetComponent<FirstPersonController>())
            {
                if (_isAttacking) health.Damage(currentDamage);
            }
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