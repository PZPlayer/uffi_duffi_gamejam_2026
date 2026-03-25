using Jam.HealthSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class ThrowingWeapon : Item, IUsable, ISecondUsable
    {
        [Header("Melee Settings")]
        [SerializeField] private List<AttackInfo> _meleeAttacks;
        [SerializeField] private float _meleeComboReset = 1.5f;

        [Header("Ranged Settings")]
        [SerializeField] private List<RangedAttackInfo> _rangedAttacks;
        [SerializeField] private float _rangedComboReset = 1.5f;
        [SerializeField] private float _waitBeforeAttack = 1.5f;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ProjectilePool _projectilePool; // Локальный пул

        [Header("Components")]
        [SerializeField] private Animator _animator;
        [SerializeField] private UnityEvent _onSwing;

        private int _currentMeleeIndex = 0;
        private int _currentRangedIndex = 0;

        private float _lastMeleeTime = -999f;
        private float _lastRangedTime = -999f;
        private float _currentDamage = 0f;

        private bool _isPrimaryPressed = false;
        private bool _isSecondaryPressed = false;
        private bool _isBusy = false;

        private Coroutine _activeRoutine;

        #region Primary Melee (IUsable)
        public void Use(InputValue value = null)
        {
            if (value != null) _isPrimaryPressed = value.isPressed;

            if (_isPrimaryPressed && !_isBusy && _activeRoutine == null)
            {
                _activeRoutine = StartCoroutine(MeleeLoop());
            }
        }

        private IEnumerator MeleeLoop()
        {
            while (_isPrimaryPressed)
            {
                if (Time.time - _lastMeleeTime > _meleeComboReset) _currentMeleeIndex = 0;
                if (_meleeAttacks == null || _meleeAttacks.Count == 0 || _isBusy) yield break;

                yield return StartCoroutine(PerformMeleeAttack());
            }
            _activeRoutine = null;
        }

        private IEnumerator PerformMeleeAttack()
        {
            if (_isBusy == false) _onSwing?.Invoke();

            _isBusy = true;
            AttackInfo info = _meleeAttacks[_currentMeleeIndex];

            _currentDamage = info.Damage;
            _lastMeleeTime = Time.time;

            if (_animator != null && info.AttackAnimation != null)
                _animator.Play(info.AttackAnimation.name);

            yield return new WaitForSeconds(info.ReloadTime);

            _currentMeleeIndex = (_currentMeleeIndex + 1) % _meleeAttacks.Count;
            _isBusy = false;
        }
        #endregion

        #region Secondary Ranged (ISecondUsable)
        public void UseSecond(InputValue value = null)
        {
            if (value != null) _isSecondaryPressed = value.isPressed;

            if (_isSecondaryPressed && !_isBusy && _activeRoutine == null)
            {
                _activeRoutine = StartCoroutine(RangedLoop());
            }
        }

        private IEnumerator RangedLoop()
        {
            while (_isSecondaryPressed)
            {
                if (Time.time - _lastRangedTime > _rangedComboReset) _currentRangedIndex = 0;
                if (_rangedAttacks == null || _rangedAttacks.Count == 0) yield break;

                yield return StartCoroutine(PerformRangedAttack());
            }
            _activeRoutine = null;
        }

        private IEnumerator PerformRangedAttack()
        {
            _onSwing?.Invoke();
            _isBusy = true;
            RangedAttackInfo info = _rangedAttacks[_currentRangedIndex];
            _lastRangedTime = Time.time;

            if (_animator != null && info.AttackAnimation != null)
                _animator.Play(info.AttackAnimation.name);

            StartCoroutine(WaitBeforeShooting(info));

            yield return new WaitForSeconds(info.ReloadTime);

            _currentRangedIndex = (_currentRangedIndex + 1) % _rangedAttacks.Count;
            _isBusy = false;
        }

        private IEnumerator WaitBeforeShooting(RangedAttackInfo info)
        {
            yield return new WaitForSeconds(_waitBeforeAttack);

            Shoot(info);
        }

        private void Shoot(RangedAttackInfo info)
        {
            if (_projectilePool == null)
            {
                Debug.LogError("ProjectilePool не назначен в ThrowingWeapon!");
                return;
            }

            GameObject obj = _projectilePool.Get();
            if (obj != null)
            {
                obj.transform.position = _muzzle.position;
                obj.transform.rotation = _muzzle.rotation;
                obj.SetActive(true);

                if (obj.TryGetComponent<Projectile>(out var proj))
                {
                    // Передаем ссылку на пул из инспектора оружия
                    proj.Initialize(Owner, _muzzle.forward, _projectilePool, info);
                }
            }
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
            return other.transform.root == Owner.transform.root ||
                   other.gameObject.layer == Owner.layer ||
                   other.CompareTag(Owner.tag);
        }

        private void OnDisable()
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = null;
            _isBusy = false;
        }
    }
}