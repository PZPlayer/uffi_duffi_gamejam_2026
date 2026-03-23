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

        // --- НОВЫЕ МЕТОДЫ ДЛЯ БОССА (Поддержка зажатия кнопок) ---

        public void StartPrimaryAttack()
        {
            // Если это наше заряжаемое оружие, напрямую говорим ему, что кнопка "зажата"
            if (_currentWeapon is ChargedWeapon cw) cw.SetPrimaryPressed(true);
            else Attack(); // Для обычного оружия (например Gun) вызываем стандартный Attack
        }

        public void StopPrimaryAttack()
        {
            if (_currentWeapon is ChargedWeapon cw) cw.SetPrimaryPressed(false);
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