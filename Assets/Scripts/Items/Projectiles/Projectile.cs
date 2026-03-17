using UnityEngine;
using Jam.HealthSystem;

namespace Jam.Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rb;
        public GameObject Owner { get; private set; }
        public float Damage { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(GameObject owner, float damage, float speed, Vector3 direction)
        {
            Owner = owner;
            Damage = damage;

            _rb.linearVelocity = direction * speed;

            Invoke(nameof(ReturnToPool), 3f);
        }

        private void OnDisable()
        {
            CancelInvoke();
            if (_rb != null) _rb.linearVelocity = Vector3.zero;
        }

        private void ReturnToPool()
        {
            if (ProjectilePool.Instance != null)
                ProjectilePool.Instance.Return(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Пуля столкнулась с: {other.name}"); 
            if (IsFriendlyFire(other)) return;

            if (other.TryGetComponent<Health>(out var health))
            {
                health.Damage(Damage, Owner);
            }

            ReturnToPool();
        }

        private bool IsFriendlyFire(Collider other)
        {
            if (Owner == null) return false;
            if (other.gameObject.layer == Owner.layer) return true;
            if (other.transform.root.gameObject == Owner.transform.root.gameObject) return true;
            return false;
        }
    }
}
