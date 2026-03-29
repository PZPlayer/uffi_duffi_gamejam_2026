using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class Gun : Item, IUsable
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private RangedAttackInfo _attackInfo;
        [SerializeField] private ProjectilePool _projectilePool;
        [SerializeField] private float _waitTime;

        // Стандартный метод для игрока (через Input)
        public void Use(InputValue input = null)
        {
            if (input != null && !input.isPressed) return;

            // Если стреляет игрок, используем просто направление ствола
            StartCoroutine(WaitBeforeShooting(_muzzle.forward));
        }

        // Новый метод специально для Босса/NPC
        public void ShootAt(Vector3 targetPoint)
        {
            // Вычисляем направление от дула до точки цели
            Vector3 direction = (targetPoint - _muzzle.position).normalized;
            StartCoroutine(WaitBeforeShooting(direction));
        }

        private IEnumerator WaitBeforeShooting(Vector3 direction)
        {
            yield return new WaitForSeconds(_waitTime);
            Shoot(direction);
        }

        private void Shoot(Vector3 direction)
        {
            GameObject obj = _projectilePool.Get();
            if (obj != null)
            {
                obj.transform.position = _muzzle.position;

                // Важно: пуля должна смотреть в сторону полета
                obj.transform.rotation = Quaternion.LookRotation(direction);
                obj.SetActive(true);

                if (obj.TryGetComponent<Projectile>(out var proj))
                {
                    proj.Initialize(Owner, direction, _projectilePool, _attackInfo);
                }
            }
        }
    }
}