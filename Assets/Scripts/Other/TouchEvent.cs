using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Jam.Other
{
    public class TouchEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onTouch;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _onTouch.Invoke();
            }
        }

        public void GoToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

    }
}
