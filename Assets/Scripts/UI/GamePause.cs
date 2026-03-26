using UnityEngine;
using Jam.Movement;
using System.Collections;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.Events;

namespace Jam.UI
{
    [RequireComponent (typeof (Animator))]
    public class GamePause : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CursorController _cursorController;
        [SerializeField] private StarterAssetsInputs _input;
        [SerializeField] private Animator _pauseAnimator;
        [SerializeField] private UnityEvent _onPause;
        [SerializeField] private UnityEvent _onResume;

        [Header("Settings")]
        [SerializeField] private float _pauseTransitionDelay = 0.3f;

        private bool _isPaused = false;

        private bool _isTransitioning = false;
        private float _targetTimeScale = 1f;
        private Coroutine _pauseCoroutine;

        public bool IsPaused => _isPaused;

        private void Awake()
        {
            ValidateReferences();

            _input.OnGamePause += TogglePause;
        }

        private void ValidateReferences()
        {
            if (_cursorController == null)
            {
                Debug.LogError("[GamePause] CursorController not found! Please assign it in inspector.");
            }

            if (_input == null)
            {
                Debug.LogError("[GamePause] No Input");
            }

            if (_pauseAnimator == null)
            {
                _pauseAnimator = GetComponent<Animator>();
            }
        }

        

        public void TogglePause()
        {
            if (_isTransitioning) return;

            if (_isPaused)
            {
                _onResume?.Invoke();
                ResumeGame();
            }
            else
            {
                _onPause?.Invoke();
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (_isPaused || _isTransitioning) return;

            _isTransitioning = true;
            _targetTimeScale = 0f;

            SetPauseAnimationState(true);

            if (_pauseCoroutine != null) StopCoroutine(_pauseCoroutine);
            _pauseCoroutine = StartCoroutine(ApplyPauseAfterDelay());
        }

        public void PauseGameForOther(bool Paused)
        {
            if (Paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void ResumeGame()
        {
            if (!_isPaused || _isTransitioning) return;

            _isTransitioning = true;
            _targetTimeScale = 1f;

            SetPauseAnimationState(false);

            if (_pauseCoroutine != null) StopCoroutine(_pauseCoroutine);
            _pauseCoroutine = StartCoroutine(ApplyResumeAfterDelay());
        }

        private IEnumerator ApplyPauseAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_pauseTransitionDelay);

            Time.timeScale = 0f;

            if (_cursorController != null)
            {
                _cursorController.UnlockCursor();
            }

            _isPaused = true;
            _isTransitioning = false;
            _pauseCoroutine = null;
        }

        private IEnumerator ApplyResumeAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_pauseTransitionDelay);


            if (_cursorController != null)
            {
                _cursorController.LockCursor();
            }

            Time.timeScale = 1f;

            _isPaused = false;
            _isTransitioning = false;
            _pauseCoroutine = null;
        }

        private void SetPauseAnimationState(bool isOpen)
        {
            if (_pauseAnimator != null)
            {
                _pauseAnimator.SetBool("Open", isOpen);
            }
        }

        public void ForceResume()
        {
            if (_pauseCoroutine != null)
            {
                StopCoroutine(_pauseCoroutine);
                _pauseCoroutine = null;
            }

            _isTransitioning = false;
            _targetTimeScale = 1f;
            Time.timeScale = 1f;

            if (_cursorController != null)
            {
                _cursorController.LockCursor();
            }

            SetPauseAnimationState(false);
            _isPaused = false;
        }

        private void OnDestroy()
        {
            _input.OnGamePause -= TogglePause;

            if (_pauseCoroutine != null)
            {
                StopCoroutine(_pauseCoroutine);
                _pauseCoroutine = null;
            }

            Time.timeScale = 1f;
        }

        private void OnApplicationQuit()
        {
            Time.timeScale = 1f;
        }

        public void SetCursorController(CursorController controller)
        {
            _cursorController = controller;
        }

        public void SetPauseAnimator(Animator animator)
        {
            _pauseAnimator = animator;
        }
    }
}