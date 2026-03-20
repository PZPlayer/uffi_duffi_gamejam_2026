using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool dash;

        public event Action<InputValue> OnUseClick;
        public event Action OnSecUseClick;
        public event Action OnActiveEffect;
        public event Action OnGamePause;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private PlayerInput _playerInput;
        private InputAction jumpAction;

#if ENABLE_INPUT_SYSTEM

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            jumpAction = _playerInput.actions["Jump"];
        }

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnDash(InputValue value)
        {
            dash = value.isPressed;
        }

        public void OnActivateEffect(InputValue value)
        {
            OnActiveEffect?.Invoke();
        }

        public void OnUse(InputValue value)
        {
            OnUseClick?.Invoke(value);
        }

        public void OnSecUse(InputValue value)
        {
            OnSecUseClick?.Invoke();
        }

        public void OnPause(InputValue value)
        {
            if (value.isPressed)
            {
                OnGamePause?.Invoke();
            }
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput()
        {
            if (jumpAction.WasPressedThisFrame())
            {
                jump = true;
            }

            if (jumpAction.WasReleasedThisFrame())
            {
                jump = false;
            }
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void Update()
        {
            JumpInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}