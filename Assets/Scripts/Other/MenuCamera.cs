using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Other
{
    public class MenuCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 2f;
        [SerializeField] private float _maxAngle = 15f;
        [SerializeField] private Camera _targetCamera;

        private Vector3 _initialRotation;
        private Vector3 _targetRotation;

        private void Start()
        {
            if (_targetCamera == null)
                _targetCamera = Camera.main;

            _initialRotation = _targetCamera.transform.localEulerAngles;
            _targetRotation = _initialRotation;
        }

        private void Update()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float normalizedX = mousePosition.x / Screen.width;
            float normalizedY = mousePosition.y / Screen.height;

            float targetX = Mathf.Lerp(-_maxAngle, _maxAngle, normalizedX);
            float targetY = Mathf.Lerp(-_maxAngle, _maxAngle, normalizedY);

            _targetRotation = new Vector3(
                Mathf.Lerp(_targetRotation.x, targetY, Time.deltaTime * _rotationSpeed),
                Mathf.Lerp(_targetRotation.y, targetX, Time.deltaTime * _rotationSpeed),
                _initialRotation.z
            );

            _targetCamera.transform.localEulerAngles = _targetRotation;
        }

        public void ResetCamera()
        {
            _targetRotation = _initialRotation;
            _targetCamera.transform.localEulerAngles = _initialRotation;
        }
    }
}
