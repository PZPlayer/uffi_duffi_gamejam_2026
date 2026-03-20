using UnityEngine;

namespace Jam.Movement
{
    public class CursorController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private CursorLockMode _defaultLockMode = CursorLockMode.Locked;
        [SerializeField] private bool _defaultVisibility = false;

        private CursorLockMode _currentLockMode;
        private bool _isCursorVisible;

        private void Awake()
        {
            InitializeCursor();
        }

        private void InitializeCursor()
        {
            SetCursorLock(_defaultLockMode);
            SetCursorVisibility(_defaultVisibility);
        }

        public void SetCursorLock(CursorLockMode lockMode)
        {
            _currentLockMode = lockMode;
            Cursor.lockState = _currentLockMode;
        }

        public void LockCursor()
        {
            SetCursorLock(CursorLockMode.Locked);
            SetCursorVisibility(false);
        }

        public void UnlockCursor()
        {
            SetCursorLock(CursorLockMode.None);
            SetCursorVisibility(true);
        }

        public void ConfineCursor()
        {
            SetCursorLock(CursorLockMode.Confined);
            SetCursorVisibility(true);
        }

        public void SetCursorVisibility(bool isVisible)
        {
            _isCursorVisible = isVisible;
            Cursor.visible = _isCursorVisible;
        }

        public void ShowCursor()
        {
            SetCursorVisibility(true);
        }

        public void HideCursor()
        {
            SetCursorVisibility(false);
        }

        public CursorLockMode GetCurrentLockMode()
        {
            return _currentLockMode;
        }

        public bool IsCursorVisible()
        {
            return _isCursorVisible;
        }

        public void ToggleCursorLock()
        {
            if (_currentLockMode == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        public void ToggleCursorVisibility()
        {
            SetCursorVisibility(!_isCursorVisible);
        }
    }
}