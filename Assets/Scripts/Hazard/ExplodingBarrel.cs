using Jam.HealthSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ExplodingBarrel : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private float _maxDamage = 100f;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _explosionDelay = 0f;

    [Header("Настройки физики")]
    [SerializeField] private float _explosionForce = 700f;
    [Tooltip("Подбрасывание вверх для эффекта 'подлета' объектов")]
    [SerializeField] private float _upwardsModifier = 1f;

    [Header("Оптимизация и фильтрация")]
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private int _maxTargets = 20;

    [Header("Падение урона")]
    [SerializeField] private AnimationCurve _damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("События")]
    [SerializeField] private UnityEvent _onExplode;

    private Collider[] _hitColliders;
    private bool _isExploding = false;

    private void Awake()
    {
        _hitColliders = new Collider[_maxTargets];
    }

    public void TriggerExplosion()
    {
        if (_isExploding) return;
        _isExploding = true;

        if (_explosionDelay > 0)
        {
            StartCoroutine(ExplosionRoutine());
        }
        else
        {
            ExecuteExplosion();
        }
    }

    private IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(_explosionDelay);
        ExecuteExplosion();
    }

    private void ExecuteExplosion()
    {
        _onExplode?.Invoke();

        int hitsCount = Physics.OverlapSphereNonAlloc(transform.position, _explosionRadius, _hitColliders, _targetLayers);

        for (int i = 0; i < hitsCount; i++)
        {
            Collider targetCollider = _hitColliders[i];

            ApplyDamage(targetCollider);

            ApplyPhysics(targetCollider);
        }

        FinalizeExplosion();
    }

    private void ApplyDamage(Collider target)
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            float distanceNormalized = Mathf.Clamp01(distance / _explosionRadius);

            float damageMultiplier = _damageFalloff.Evaluate(distanceNormalized);
            float finalDamage = _maxDamage * damageMultiplier;

            health.Damage(finalDamage);
        }
    }

    private void ApplyPhysics(Collider target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _upwardsModifier, ForceMode.Impulse);
        }
    }

    private void FinalizeExplosion()
    {
        // Отключаем визуальную и физическую часть бочки
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        if (TryGetComponent<MeshRenderer>(out var mesh)) mesh.enabled = false;

        // Удаляем объект через время, чтобы успели доиграть эффекты (если они в дочерних объектах)
        Destroy(gameObject, 3f); //Можно заменить на выключение
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}