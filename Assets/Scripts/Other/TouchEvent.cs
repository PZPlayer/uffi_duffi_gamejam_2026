using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Jam.Other
{
    public class TouchEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onTouch;

        [Header("Ёто дл€ спавн поинта")]
        [Tooltip("¬ставл€й точку спавна сюда")]
        [SerializeField] private Transform _spawnPoint;
        [Space(10)]
        [Header("Ёто дл€ смерти. ≈сли тебе надо чтобы убивало включи")]
        [Tooltip("≈сли надо чтоб игрок возвращалс€ на последнию точку спавна")]
        [SerializeField] private bool _isDeadly = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _onTouch.Invoke();
                if (_isDeadly )
                {
                    SendPlayerBackToSpawn(other.gameObject);
                }
                else if (_spawnPoint != false)
                {
                    SetSpawnPoint();
                }
            }
        }

        public void GoToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void SetSpawnPoint()
        {
            LevelManager.MANAGER.CheckPoint(_spawnPoint);
        }

        public void SendPlayerBackToSpawn(GameObject player)
        {
            LevelManager.MANAGER.ThrowPlayerBackToCheckPoint(player);
        }

    }
}
