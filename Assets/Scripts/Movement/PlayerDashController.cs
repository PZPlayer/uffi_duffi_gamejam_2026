using StarterAssets;
using System;
using UnityEngine;

namespace Jam.Movement
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    [RequireComponent(typeof(FirstPersonController))]

    public class PlayerDashController : MonoBehaviour
    {
        public event Action OnDash;

        [Header("Dash Settings")]
        [SerializeField] private float _dashPower = 20f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private float _dashBoofer = 0.2f;

        private FirstPersonController _controller;
        private CharacterController _characterController;
        private StarterAssetsInputs _input;

        private float lastTimeDashTry;
        private bool _thereIsSomethingInBoofer;
        private bool _isDashing = false;
        private float _dashTime;
        private float _lastDashTime;
        private Vector3 _dashDirection;

        private void Awake()
        {
            _controller = GetComponent<FirstPersonController>();
            _characterController = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
        }

        private void OnValidate()
        {
            _dashPower = Mathf.Max(0f, _dashPower);
            _dashDuration = Mathf.Max(0.01f, _dashDuration);
            _dashCooldown = Mathf.Max(_dashDuration + 0.1f, _dashCooldown); //минимальная небольшая задержка после скачка, чтобы не было бесконечного скачка
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTime = _dashDuration;
            _lastDashTime = Time.time;
            _dashDirection = GetDashDirection();
            _controller.CanMove = false;

            OnDash?.Invoke();
        }

        private Vector3 GetDashDirection()
        {
            if (_input.move != Vector2.zero)
            {
                Vector3 moveDirection = new Vector3(_input.move.x, 0, _input.move.y).normalized;
                return transform.TransformDirection(moveDirection);
            }
            else
            {
                return transform.forward;
            }
        }

        public bool TryStartDash()
        {
            if (_isDashing || (Time.time - _lastDashTime < _dashCooldown))
            {
                _input.dash = false;
                return false;
            }
            else
            {
                _thereIsSomethingInBoofer = false;
                StartDash();
                return true;
            }
        }

        private void Update()
        {
            if (_input.dash)
            {
                if(TryStartDash() == false)
                {
                    _thereIsSomethingInBoofer = true;
                    lastTimeDashTry = Time.time;
                }
            }

            CheckBoofer();

            if (_isDashing)
            {
                PerformDash();
            }
        }

        private void CheckBoofer()
        {
            if (!_thereIsSomethingInBoofer) return;
            
            if (Time.time - lastTimeDashTry > _dashBoofer)
            {
                _thereIsSomethingInBoofer = false;
                return;
                
            }

            TryStartDash();
        }

        private void PerformDash()
        {
            if (_dashTime > 0)
            {
                _characterController.Move(_dashDirection * _dashPower * Time.deltaTime);
                _dashTime -= Time.deltaTime;
            }
            else
            {
                _isDashing = false;
                _controller.CanMove = true;
            }
        }

        public bool IsDashing => _isDashing;
        public float CooldownTime
        {
            get => _dashCooldown; 
            set
            {
                _dashCooldown = value;
                _dashCooldown = Mathf.Max(_dashCooldown, _dashDuration + 0.1f);
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
                _controller.CanMove = true;

            _isDashing = false;
        }
    }
}