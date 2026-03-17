using UnityEngine;
using UnityEngine.AI;
using Jam.HealthSystem;
using Jam.NPCSystem;

public abstract class NPCBehavior : MonoBehaviour
{
    public enum BodyState { Alive, Dead }
    public enum AIState { Idle, Chasing, Attacking, Retreating }

    [Header("AI State")]
    [SerializeField] protected BodyState _bodyState = BodyState.Alive;
    [SerializeField] protected AIState _aiState = AIState.Idle;

    [Header("Sensors")]
    [SerializeField] protected float _detectionRadius = 15f;
    [Range(0, 360)][SerializeField] protected float _viewAngle = 90f;
    [SerializeField] protected LayerMask _obstacleMask;
    [SerializeField] protected LayerMask _targetMask; // Теперь ищем любые цели на этих слоях

    [Header("Social Settings")]
    [SerializeField] protected float _shoutRadius = 10f;
    [SerializeField] protected LayerMask _npcMask;

    protected NavMeshAgent _agent;
    protected Transform _target; // Вместо _player используем универсальный _target
    protected Health _health;
    protected WeaponNPCHeader _weaponHeader;
    private readonly Collider[] _allyBuffer = new Collider[10];
    private readonly Collider[] _targetBuffer = new Collider[5];

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();
        _weaponHeader = GetComponent<WeaponNPCHeader>();

        if (_health != null)
        {
            _health.Death += OnDeath;
            _health.OnDamaged += ReactToHit;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_health != null)
        {
            _health.Death -= OnDeath;
            _health.OnDamaged -= ReactToHit;
        }
    }

    protected virtual void Update()
    {
        if (_bodyState == BodyState.Dead) return;

        // Если цели нет — пытаемся найти её каждый кадр (или через таймер для оптимизации)
        if (_target == null) SearchForTarget();

        ExecuteFSM();
    }

    protected void SearchForTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _detectionRadius, _targetBuffer, _targetMask);

        for (int i = 0; i < count; i++)
        {
            Transform potentialTarget = _targetBuffer[i].transform;
            if (potentialTarget == transform) continue;

            if (IsTargetInVision(potentialTarget))
            {
                _target = potentialTarget;
                break;
            }
        }
    }

    protected bool IsTargetInVision(Transform target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > _detectionRadius) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);

        if (angle < _viewAngle / 2f)
        {
            // Проверка на препятствия
            if (!Physics.Linecast(transform.position + Vector3.up, target.position + Vector3.up, _obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    private void ReactToHit(GameObject attacker)
    {
        if (_bodyState == BodyState.Dead || attacker == null) return;

        // Если нас ударили, атакующий автоматически становится нашей целью
        _target = attacker.transform;

        if (_aiState == AIState.Idle)
        {
            _aiState = AIState.Chasing;
            AlertNearbyAllies();
        }
    }

    protected abstract void ExecuteFSM();

    protected void AlertNearbyAllies()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _shoutRadius, _allyBuffer, _npcMask);
        for (int i = 0; i < count; i++)
        {
            if (_allyBuffer[i].gameObject == gameObject) continue;
            if (_allyBuffer[i].TryGetComponent<NPCBehavior>(out var allyAI))
            {
                allyAI.OnAlertedByAlly(_target);
            }
        }
    }

    public void OnAlertedByAlly(Transform sharedTarget)
    {
        if (_bodyState == BodyState.Dead || sharedTarget == null) return;
        if (_target == null)
        {
            _target = sharedTarget;
            _aiState = AIState.Chasing;
        }
    }

    protected virtual void OnDeath()
    {
        _bodyState = BodyState.Dead;
        if (_agent != null) _agent.isStopped = true;
        _target = null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Желтый шар — радиус зрения
        Gizmos.color = new Color(1, 0.92f, 0, 0.2f); // Прозрачно-желтый
        Gizmos.DrawSphere(transform.position, _detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        // Голубой шар — радиус зова о помощи
        Gizmos.color = new Color(0, 1, 1, 0.1f); // Очень прозрачный циан
        Gizmos.DrawSphere(transform.position, _shoutRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _shoutRadius);

        // Отрисовка "конуса" зрения
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * _detectionRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * _detectionRadius);
    }

}