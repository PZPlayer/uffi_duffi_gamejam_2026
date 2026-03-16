using UnityEngine;
using Jam.HealthSystem;

namespace Jam.Items
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 25f;
        public GameObject Owner { get; set; }
        public float Damage { get; set; }

        private void OnEnable() => Invoke(nameof(Return), 3f);

        private void Update()
        {
            float step = _speed * Time.deltaTime;
            transform.position += transform.forward * step;
        }

        private void Return() => ProjectilePool.Instance.Return(gameObject);

        private void OnTriggerEnter(Collider other)
        {
            if (IsFriendlyFire(other)) return;

            if (other.TryGetComponent<Health>(out var health))
                health.Damage(Damage, Owner);
                
            Return();
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
