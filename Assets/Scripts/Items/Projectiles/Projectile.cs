using UnityEngine;
using Jam.HealthSystem;

namespace Jam.Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rb;
        private RangedAttackInfo _attackInfo;
        private ProjectilePool _myPool;

        public GameObject Owner { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(GameObject owner, RangedAttackInfo info, Vector3 direction, ProjectilePool pool)
        {
            Owner = owner;
            _attackInfo = info;
            _myPool = pool;

            _rb.linearVelocity = direction * _attackInfo.ProjectileSpeed;

            CancelInvoke(); // Сброс на случай повторного использования из пула
            Invoke(nameof(ReturnToPool), _attackInfo.LifeTime);
        }

        private void OnDisable()
        {
            CancelInvoke();
            if (_rb != null) _rb.linearVelocity = Vector3.zero;
        }

        private void ReturnToPool()
        {
            // Если пул всё еще жив — возвращаемся в него
            if (_myPool != null)
            {
                _myPool.Return(gameObject);
            }
            else
            {
                // Если оружие (и пул) удалены — снаряд просто уничтожается навсегда
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsFriendlyFire(other)) return;

            if (other.TryGetComponent<Health>(out var health))
            {
                health.Damage(_attackInfo.Damage, Owner);
            }

            ReturnToPool();
        }

        private bool IsFriendlyFire(Collider other)
        {
            if (Owner == null) return false;
            // Проверка по слоям и руту персонажа
            if (other.gameObject.layer == Owner.layer) return true;
            if (other.transform.root.gameObject == Owner.transform.root.gameObject) return true;
            return false;
        }
    }
}