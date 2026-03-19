using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Items
{
    public class Gun : Item, IUsable
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _damage = 15f;
        [SerializeField] private float _projectileSpeed = 30f;

        public void Use(InputValue input = null)
        {
            GameObject obj = ProjectilePool.Instance.Get();
            if (obj != null)
            {
                obj.transform.position = _muzzle.position;
                obj.transform.rotation = _muzzle.rotation;
                obj.SetActive(true);

                Vector3 shootDirection = _muzzle.forward;

                if (obj.TryGetComponent<Projectile>(out var proj))
                {
                    proj.Initialize(Owner, _damage, _projectileSpeed, shootDirection);
                }
            }
        }
    }
}