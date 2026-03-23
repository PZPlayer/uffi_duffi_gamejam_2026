using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Jam.Items;
using Jam.Effects.EffectChildren;

namespace Jam.NPCSystem
{
    public class BossBehavior : NPCBehavior
    {
        [Header("Movement & Aiming Settings")]
        [Tooltip("Скорость поворота тела босса к игроку")]
        [SerializeField] private float _rotationSpeed = 5f;
        [Tooltip("Смещение точки прицеливания вверх (чтобы стрелять в грудь, а не в пятки)")]
        [SerializeField] private float _aimHeightOffset = 1.2f;

        [Header("Combat Distances")]
        [SerializeField] private float _meleeAttackRange = 3f;
        [Tooltip("Запас дистанции, чтобы босс не дергался туда-сюда на границе ближнего боя")]
        [SerializeField] private float _meleeDisengageBuffer = 0.5f;
        [SerializeField] private float _rangedAttackRange = 12f;

        [Header("Melee Combat (Axe)")]
        [Tooltip("Общая пауза между любыми сериями атак")]
        [SerializeField] private float _attackCooldown = 2.5f;
        [Tooltip("Шанс тяжелой атаки от 0 до 1 (0.4 = 40%)")]
        [Range(0f, 1f)]
        [SerializeField] private float _heavyAttackChance = 0.4f;
        [Tooltip("Время зарядки тяжелого удара")]
        [SerializeField] private float _heavyChargeTime = 2.5f;
        [Tooltip("Длительность обычного комбо")]
        [SerializeField] private float _lightComboTime = 1.5f;
        [Tooltip("Пауза (передышка) сразу после завершения удара")]
        [SerializeField] private float _meleeRecoveryTime = 0.5f;

        [Header("Ranged Attack (Acid)")]
        [Tooltip("Ссылка на объект пушки для плевков")]
        [SerializeField] private Gun _acidGun;
        [Tooltip("Количество выстрелов за одну серию")]
        [SerializeField] private int _numberOfAttacks = 3;
        [Tooltip("Пауза между выстрелами внутри серии")]
        [SerializeField] private float _timeBetweenShots = 0.3f;
        [Tooltip("Множитель скорости передвижения во время стрельбы (например, 0.5)")]
        [Range(0f, 1f)]
        [SerializeField] private float _rangedSpeedMultiplier = 0.5f;
        [Tooltip("Пауза после серии выстрелов")]
        [SerializeField] private float _rangedRecoveryTime = 1.0f;

        [Header("Healing Phase")]
        [Range(0f, 1f)]
        [SerializeField] private float _healThresholdPercent = 0.3f;
        [SerializeField] private HealingFire _healingEffect;
        [SerializeField] private UnityEvent _onHealPhaseTriggered;

        [Header("Debug")]
        [SerializeField] private float _gizmoHeightOffset = 0.1f;

        private bool _hasHealed = false;
        private bool _isAttacking = false;
        private float _lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            if (_agent != null) _agent.stoppingDistance = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (_bodyState == BodyState.Alive && !_hasHealed)
            {
                CheckHealPhase();
            }
        }

        private void LateUpdate()
        {
            if (_bodyState == BodyState.Dead || _target == null) return;

            if (_healingEffect != null && _healingEffect.IsHealing) return;

            if (_aiState != AIState.Idle)
            {
                RotateTowardsTarget();
            }
        }

        protected override void ExecuteFSM()
        {
            if (_target == null || _bodyState == BodyState.Dead) return;

            if (_healingEffect != null && _healingEffect.IsHealing)
            {
                _agent.isStopped = true;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, _target.position);

            switch (_aiState)
            {
                case AIState.Idle:
                    if (distanceToTarget <= _detectionRadius) _aiState = AIState.Chasing;
                    break;

                case AIState.Chasing:
                    ChaseLogic(distanceToTarget);
                    break;

                case AIState.Attacking:
                    MeleeCombatLogic(distanceToTarget);
                    break;
            }
        }

        private void ChaseLogic(float distance)
        {
            if (_isAttacking) return;

            _agent.isStopped = false;
            _agent.SetDestination(_target.position);

            if (distance > _meleeAttackRange && distance <= _rangedAttackRange)
            {
                TryRangedAttack();
            }

            if (distance <= _meleeAttackRange)
            {
                _aiState = AIState.Attacking;
            }
        }

        private void MeleeCombatLogic(float distance)
        {
            if (distance > _meleeAttackRange + _meleeDisengageBuffer)
            {
                _aiState = AIState.Chasing;
                return;
            }

            _agent.isStopped = true;

            if (Time.time - _lastAttackTime >= _attackCooldown && !_isAttacking)
            {
                bool useHeavyAttack = Random.value <= _heavyAttackChance;
                StartCoroutine(MeleeAttackRoutine(useHeavyAttack));
            }
        }

        private void TryRangedAttack()
        {
            if (Time.time - _lastAttackTime >= _attackCooldown && !_isAttacking)
            {
                StartCoroutine(RangedAttackRoutine());
            }
        }

        private void RotateTowardsTarget()
        {
            Vector3 targetPos = _target.position;
            Vector3 dir = (targetPos - transform.position).normalized;

            // 1. Поворот тела (только Y - горизонталь)
            Vector3 bodyDir = new Vector3(dir.x, 0, dir.z);
            if (bodyDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(bodyDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _rotationSpeed);
            }

            // 2. Поворот оружия (полный прицел в aimPoint)
            Vector3 aimPoint = targetPos + Vector3.up * _aimHeightOffset;

            if (_weaponHeader != null) _weaponHeader.transform.LookAt(aimPoint);
            if (_acidGun != null) _acidGun.transform.LookAt(aimPoint);
        }

        private IEnumerator MeleeAttackRoutine(bool isHeavy)
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            if (isHeavy)
            {
                _weaponHeader.StartSecondaryAttack();
                yield return new WaitForSeconds(_heavyChargeTime);
                _weaponHeader.StopSecondaryAttack();
            }
            else
            {
                _weaponHeader.StartPrimaryAttack();
                yield return new WaitForSeconds(_lightComboTime);
                _weaponHeader.StopPrimaryAttack();
            }

            yield return new WaitForSeconds(_meleeRecoveryTime);
            _isAttacking = false;
        }

        private IEnumerator RangedAttackRoutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            float originalSpeed = _agent.speed;
            _agent.speed = originalSpeed * _rangedSpeedMultiplier;

            if (_acidGun != null)
            {
                for (int i = 0; i < _numberOfAttacks; i++)
                {
                    _acidGun.Use(null);
                    yield return new WaitForSeconds(_timeBetweenShots);
                }
            }

            _agent.speed = originalSpeed;
            yield return new WaitForSeconds(_rangedRecoveryTime);
            _isAttacking = false;
        }

        private void CheckHealPhase()
        {
            if (_hasHealed || _bodyState == BodyState.Dead || _health == null) return;
            float currentHealthPercent = _health.CurHealth / _health.MaxHealth;
            if (currentHealthPercent <= _healThresholdPercent)
            {
                _hasHealed = true;
                _onHealPhaseTriggered?.Invoke();
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Vector3 gizmoPos = transform.position + Vector3.up * _gizmoHeightOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoPos, _meleeAttackRange);
            Gizmos.color = new Color(0.7f, 0f, 1f);
            Gizmos.DrawWireSphere(gizmoPos, _rangedAttackRange);
        }
    }
}