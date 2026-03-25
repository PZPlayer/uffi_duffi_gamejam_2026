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
        private ISecondUsable _secondUsableItem; // Добавили поддержку второй атаки

        void Start()
        {
            if (_currentWeapon != null)
            {
                _usableItem = _currentWeapon.GetComponent<IUsable>();
                _secondUsableItem = _currentWeapon.GetComponent<ISecondUsable>();
                _currentWeapon.Owner = gameObject;
            }
        }

        // Старый метод оставлен БЕЗ ИЗМЕНЕНИЙ для обратной совместимости
        public void Attack()
        {
            if (_usableItem == null)
            {
                Debug.LogError("Header не нашел IUsable на предмете!");
                return;
            }
            _usableItem.Use();
        }

        public void StartPrimaryAttack()
        {
            switch (_currentWeapon)
            {
                case Stick stick:
                    stick.SetPressed(true);
                    break;
                case ChargedWeapon cw:
                    cw.SetPrimaryPressed(true);
                    break;
                default:
                    Attack();
                    break;
            }
        }

        public void StopPrimaryAttack()
        {
            switch (_currentWeapon)
            {
                case Stick stick:
                    stick.SetPressed(false);
                    break;
                case ChargedWeapon cw:
                    cw.SetPrimaryPressed(false);
                    break;
            }
        }

        public void StartSecondaryAttack()
        {
            if (_currentWeapon is ChargedWeapon cw) cw.SetSecondaryPressed(true);
            else if (_secondUsableItem != null) _secondUsableItem.UseSecond();
        }

        public void StopSecondaryAttack()
        {
            if (_currentWeapon is ChargedWeapon cw) cw.SetSecondaryPressed(false);
        }
    }
}