using Jam.Effects;
using Jam.HealthSystem;
using Jam.Effects.EffectChildren;
using UnityEngine;

namespace Jam.Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private RangedAttackInfo _attackInfo;
        [SerializeField] private GameObject _explousionEffect;

        [Header("Настройки взрыва (AoE)")]
        [SerializeField] private bool _isExplosive = false; // Включаем для ядовитого плевка
        [SerializeField] private float _explosionRadius = 3f;
        [SerializeField] private LayerMask _targetLayers;

        [Header("Эффекты при взрыве")]
        [SerializeField] private IdleEffect _poisonEffectTemplate;

        private Rigidbody _rb;
        private ProjectilePool _myPool;
        private bool _isDeactivated = false; // Защита от двойного срабатывания

        public GameObject Owner { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(GameObject owner, Vector3 direction, ProjectilePool pool, RangedAttackInfo info = null)
        {
            Owner = owner;
            if (info != null) _attackInfo = info;
            _myPool = pool;
            _isDeactivated = false;

            _rb.linearVelocity = direction * _attackInfo.ProjectileSpeed;

            CancelInvoke();
            // Если время вышло, пуля взрывается прямо в воздухе (или просто исчезает)
            Invoke(nameof(HandleProjectileDestruction), _attackInfo.LifeTime);
        }

        private void OnDisable()
        {
            CancelInvoke();
            if (_rb != null) _rb.linearVelocity = Vector3.zero;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isDeactivated || IsFriendlyFire(other)) return;

            if (!_isExplosive)
            {
                // СТАРАЯ ЛОГИКА: Обычная пуля наносит урон только одной цели
                if (other.TryGetComponent<Health>(out var health))
                {
                    health.Damage(_attackInfo.Damage, Owner);
                }
            }

            // Вызываем уничтожение/взрыв
            HandleProjectileDestruction();
        }

        private void HandleProjectileDestruction()
        {
            if (_isDeactivated) return;
            _isDeactivated = true;

            CancelInvoke(); // Отменяем таймер жизни, если попали в стену/врага

            // Спавн визуала взрыва
            if (_explousionEffect != null)
                Destroy(Instantiate(_explousionEffect, transform.position, Quaternion.identity), 3f);

            // НОВАЯ ЛОГИКА: Взрыв по площади
            if (_isExplosive)
            {
                ExecuteExplosion();
            }

            gameObject.SetActive(false);
            ReturnToPool();
        }

        private void ExecuteExplosion()
        {
            // Ищем всех в радиусе взрыва
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _targetLayers);

            foreach (Collider hitCollider in hits)
            {
                if (IsFriendlyFire(hitCollider)) continue;

                // 1. Наносим мгновенный урон от взрыва (если нужно)
                if (hitCollider.TryGetComponent<Health>(out var health))
                {
                    health.Damage(_attackInfo.Damage, Owner);
                }

                // 2. Накладываем яд через твой EffectHandler
                if (_poisonEffectTemplate != null && hitCollider.TryGetComponent<EffectHandler>(out var handler))
                {
                    // Твой метод AddEffect сам создаст компонент, скопирует данные из шаблона 
                    // и запустит периодический урон.
                    handler.AddEffect(_poisonEffectTemplate);
                }
            }
        }

        private void ReturnToPool()
        {
            if (_myPool != null)
                _myPool.Return(gameObject);
            else
                Destroy(gameObject);
        }

        private bool IsFriendlyFire(Collider other)
        {
            if (Owner == null) return false;
            if (other.gameObject.layer == Owner.layer) return true;
            if (other.transform.root.gameObject == Owner.transform.root.gameObject) return true;
            return false;
        }

        // Отрисовка радиуса взрыва в редакторе для удобства
        #region Editor Tools (Gizmos)
        private void OnDrawGizmosSelected()
        {
            if (_isExplosive)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, _explosionRadius);
                Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                Gizmos.DrawSphere(transform.position, _explosionRadius);
            }
        }
        #endregion
    }
}