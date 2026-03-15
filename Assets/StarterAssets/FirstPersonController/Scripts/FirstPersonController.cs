using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public bool CanMove = true;
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] private float _moveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] private float _sprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        [SerializeField] private float _rotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float _speedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float _gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] private float _jumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float _fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool _grounded = true;
        [Tooltip("Useful for rough ground")]
        [SerializeField] private float _groundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float _groundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        [SerializeField] private LayerMask _groundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject _cinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float _topClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float _bottomClamp = -90.0f;

        [Header("Double Jump")]
        [Tooltip("Enable/disable double jump")]
        [SerializeField] private bool _hasDoubleJump = true;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private bool _canDoubleJump = false;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = _jumpTimeout;
            _fallTimeoutDelta = _fallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            if(CanMove) Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
            _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * _rotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * _rotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

                // Update Cinemachine camera target pitch
                _cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? _sprintSpeed : _moveSpeed;

            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * _speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (_grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = _fallTimeout;

                // reset double jump when grounded
                _hasDoubleJump = true;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = _jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
            }

            // DOUBLE JUMP - проверяем по нажатию
            if (_input.jump) // jump = true только в кадр нажатия
            {
                if (_grounded && _jumpTimeoutDelta <= 0.0f)
                {
                    // Обычный прыжок
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                    _canDoubleJump = false;
                }
                else if (!_grounded && _canDoubleJump && _hasDoubleJump)
                {
                    // Двойной прыжок
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                    _hasDoubleJump = false;
                    _canDoubleJump = false;
                }
            }
            else
            {
                _canDoubleJump = true;
                if (_verticalVelocity > 0)
                {
                    _verticalVelocity = 0.0f;
                }
            }

            // apply gravity over time
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (_grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z), _groundedRadius);
        }
    }
}