using StarterAssets;
using System;
using UnityEngine;

public class Dash : MonoBehaviour
{
    public event Action OnDash;

    [Header("Dash Settings")]
    [SerializeField] private float _dashPower = 20f;
    [SerializeField] private float _dashDuration = 0.2f;
    public float _dashCooldown = 0.5f;
    [SerializeField] private int _maxCharges = 3;
    [SerializeField] private float _chargeRestoreTime = 1.5f; // время восстановления одного заряда

    [Header("Debug")]
    [SerializeField] private int _currentCharges;
    [SerializeField] private bool _isDashing = false;

    private FirstPersonController _controller;
    private CharacterController _characterController;
    private StarterAssetsInputs _input;

    private float _dashTime;
    private float _cooldownTime;
    private float _chargeRestoreTimer;
    private Vector3 _dashDirection;

    private void Start()
    {
        _controller = GetComponent<FirstPersonController>();
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();

        _currentCharges = _maxCharges;
        _chargeRestoreTimer = _chargeRestoreTime;
    }

    private void Update()
    {
        HandleDashInput();
        HandleChargeRestore();
    }

    private void HandleDashInput()
    {
        // Нельзя делать рывок во время другого рывка
        if (_isDashing) return;

        // Проверяем нажатие клавиши и наличие зарядов
        if (_input.dash && _cooldownTime <= 0)
        {
            StartDash();
        }

        _input.dash = false;

        // Обновляем кулдаун
        if (_cooldownTime > 0)
        {
            _cooldownTime -= Time.deltaTime;
        }
    }

    private void StartDash()
    {
        OnDash?.Invoke();
        _isDashing = true;
        _currentCharges--;
        _cooldownTime = _dashCooldown;
        _dashTime = _dashDuration;

        // Определяем направление рывка
        _dashDirection = GetDashDirection();

        // Временно отключаем обычное управление (опционально)
        _controller.CanMove = false;
        
    }

    private Vector3 GetDashDirection()
    {
        // Приоритет: направление движения > направление взгляда
        if (_input.move != Vector2.zero)
        {
            // Рывок в сторону движения
            Vector3 moveDirection = new Vector3(_input.move.x, 0, _input.move.y).normalized;
            return transform.TransformDirection(moveDirection);
        }
        else
        {
            // Рывок вперед по взгляду
            return transform.forward;
        }
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            PerformDash();
        }
    }

    private void PerformDash()
    {
        if (_dashTime > 0)
        {
            // Выполняем рывок
            _characterController.Move(_dashDirection * _dashPower * Time.fixedDeltaTime);
            _dashTime -= Time.fixedDeltaTime;
        }
        else
        {
            // Завершаем рывок
            _isDashing = false;
            _controller.CanMove = true;
        }
    }

    private void HandleChargeRestore()
    {
        if (_currentCharges >= _maxCharges) return;

        _chargeRestoreTimer -= Time.deltaTime;

        if (_chargeRestoreTimer <= 0)
        {
            _currentCharges++;
            _chargeRestoreTimer = _chargeRestoreTime;
            Debug.Log($"Dash charge restored! Charges: {_currentCharges}");
        }
    }

    // Публичные методы для доступа из других скриптов
    public bool IsDashing() => _isDashing;
    public int GetCurrentCharges() => _currentCharges;
    public int GetMaxCharges() => _maxCharges;
    public float GetChargeRestoreProgress() => 1 - (_chargeRestoreTimer / _chargeRestoreTime);

    // Метод для принудительного добавления зарядов (например, за подбираемые предметы)
    public void AddCharge(int amount)
    {
        _currentCharges = Mathf.Min(_currentCharges + amount, _maxCharges);
    }
}