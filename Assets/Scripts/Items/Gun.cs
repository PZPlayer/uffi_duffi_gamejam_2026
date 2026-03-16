using Jam.Items;
using UnityEngine;

namespace Jam.Items
{
    public class Gun : Item, IUsable
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _damage = 15f;

        public void Use()
        {
            GameObject obj = ProjectilePool.Instance.Get();
            if (obj != null)
            {
                obj.transform.position = _muzzle.position;
                obj.transform.rotation = _muzzle.rotation;

                var proj = obj.GetComponent<Projectile>();
                proj.Owner = this.Owner;
                proj.Damage = _damage;
            }
        }
    }
}
