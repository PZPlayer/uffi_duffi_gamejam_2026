using Jam.Items;
using UnityEngine;

namespace Jam.NPCSystem
{
    public class WeaponNPCHeader : MonoBehaviour
    {
        [SerializeField] private Item _currentWeapon;
        [SerializeField] private float _attackRange = 2.0f;

        public float AttackRange => _attackRange;

        private IUsable _usableItem;

        void Start()
        {
            if (_currentWeapon != null)
            {
                _usableItem = _currentWeapon.GetComponent<IUsable>();
                _currentWeapon.Owner = gameObject;
            }
        }

        public void Attack()
        {
            if (_usableItem == null)
            {
                Debug.LogError("Header не нашел IUsable на предмете!");
                return;
            }
            _usableItem.Use();
        }
    }
}