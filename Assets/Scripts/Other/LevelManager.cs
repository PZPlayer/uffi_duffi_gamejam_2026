using StarterAssets;
using UnityEngine;

namespace Jam.Other
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager MANAGER;

        [SerializeField] private Transform _startPoint;

        private Transform lastSafePoint;

        private void Start()
        {
            if(MANAGER == null)
            {
                MANAGER = this;
            }

            if(_startPoint == null) return;
            lastSafePoint = _startPoint;
        }

        public void CheckPoint(Transform point)
        {
            lastSafePoint = point;
        }

        public void ThrowPlayerBackToCheckPoint(GameObject player)
        {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = lastSafePoint.position;
            player.GetComponent<CharacterController>().enabled = true;
        }
    }
}
