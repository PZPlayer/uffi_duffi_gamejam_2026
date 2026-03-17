using UnityEngine;

namespace Jam.UI
{
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private bool _invert = false;

        private void Start()
        {
            if (_targetCamera == null)
                _targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_targetCamera == null) return;

            transform.LookAt(transform.position + _targetCamera.transform.rotation * Vector3.forward,
                             _targetCamera.transform.rotation * Vector3.up);

            if (_invert)
                transform.Rotate(0, 180, 0);
        }
    }
}