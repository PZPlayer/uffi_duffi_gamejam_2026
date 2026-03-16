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
    [SerializeField] protected LayerMask _playerMask;

    [Header("Social Settings")]
    [SerializeField] protected float _shoutRadius = 10f; 
    [SerializeField] protected LayerMask _npcMask;

    protected NavMeshAgent _agent;
    protected Transform _player;
    protected Health _health;
    protected WeaponNPCHeader _weaponHeader;
    private readonly Collider[] _allyBuffer = new Collider[10];

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();
        _weaponHeader = GetComponent<WeaponNPCHeader>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

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
        ExecuteFSM();
    }

    private void ReactToHit(GameObject attacker)
    {
        if (_bodyState == BodyState.Dead || attacker == null) return;

        if (_aiState == AIState.Idle)
        {
            _aiState = AIState.Chasing;
            AlertNearbyAllies();
        }
        Vector3 lookDir = (attacker.transform.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);
    }

    protected abstract void ExecuteFSM();

    protected bool IsPlayerInVision()
    {
        if (_player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer > _detectionRadius) return false;

        Vector3 dirToPlayer = (_player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleToPlayer < _viewAngle / 2f)
        {
            if (!Physics.Linecast(transform.position + Vector3.up, _player.position + Vector3.up, _obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    protected void AlertNearbyAllies()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _shoutRadius, _allyBuffer, _npcMask);

        for (int i = 0; i < count; i++)
        {
            Collider allyCollider = _allyBuffer[i];

            if (allyCollider.gameObject == gameObject) continue;

            if (allyCollider.TryGetComponent<NPCBehavior>(out var allyAI))
            {
                allyAI.OnAlertedByAlly(_player);
            }
        }
    }

    public void OnAlertedByAlly(Transform target)
    {
        if (_bodyState == BodyState.Dead) return;

        if (_aiState == AIState.Idle)
        {
            _player = target;
            _aiState = AIState.Chasing;
            Debug.Log($"{gameObject.name}: 'Понял, иду на помощь к {name}!'");
        }
    }

    private void OnDeath()
    {
        _bodyState = BodyState.Dead;
        _agent.isStopped = true;
        Debug.Log($"{gameObject.name} умер.");
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Отрисовка радиуса обнаружения игрока (желтым)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        // Отрисовка углов обзорас(красным)
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftBoundary * _detectionRadius);
        Gizmos.DrawRay(transform.position, rightBoundary * _detectionRadius);

        // Отрисовка радиус крика (синим)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _shoutRadius);
    }
}