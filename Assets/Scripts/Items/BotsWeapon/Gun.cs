using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class Gun : Item, IUsable
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private RangedAttackInfo _attackInfo;
        [SerializeField] private ProjectilePool _projectilePool;

        public void Use(InputValue input = null)
        {
            // Если кнопка только нажата (чтобы не спамить 60 раз в секунду без корутины)
            if (input != null && !input.isPressed) return;

            GameObject obj = _projectilePool.Get();
            if (obj != null)
            {
                obj.transform.position = _muzzle.position;
                obj.transform.rotation = _muzzle.rotation;
                obj.SetActive(true);

                if (obj.TryGetComponent<Projectile>(out var proj))
                {
                    // Передаем пул и инфо в снаряд
                    proj.Initialize(Owner, _muzzle.forward, _projectilePool, _attackInfo);
                }
            }
        }
    }
}