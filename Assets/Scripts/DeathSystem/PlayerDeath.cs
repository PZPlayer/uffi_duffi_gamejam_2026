using Jam.HealthSystem;
using Jam.UI;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Jam.DeathSystem
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Health _playerHealth;
        [SerializeField] private FirstPersonController _playerController;
        [SerializeField] private EndGamePanel _endGamePanel;
        [SerializeField] private UnityEvent _firstDeath;
        [SerializeField] private UnityEvent _secondDeath;
        [SerializeField] private UnityEvent _thirdDeath;

        private int playersDeathCount;

        public void Die()
        {
            playersDeathCount++;
            _playerController.CanMove = false;
            _playerHealth.gameObject.transform.position = _spawnPoint.position;
            RevivePlayer();

            switch (playersDeathCount)
            {
                case 1:
                    _firstDeath?.Invoke();
                    break;
                case 2:
                    _secondDeath?.Invoke();
                    break;
                case 3:
                    _thirdDeath?.Invoke();
                    break;
                default:
                    break;
            }
        }

        public void Restart()
        {
            playersDeathCount = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void RevivePlayer()
        {
            _playerHealth.Heal(_playerHealth.MaxHealth);
            _playerController.CanMove = true;
        }

        private void OnEnable()
        {
            if (_playerController != null)
                _playerController.CanMove = true;
        }
    }
}
