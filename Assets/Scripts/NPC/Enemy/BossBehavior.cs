using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Jam.Items;
using Jam.Effects.EffectChildren;
using Jam.Effects;
using Jam.Audio;

namespace Jam.NPCSystem
{
    [RequireComponent(typeof(NPCEffectHandler))]
    public class BossBehavior : NPCBehavior
    {
        public enum HealMovementMode { Stop, Slow, Normal }

        [SerializeField] private IdleAudioBehavior _idleAudioBehavior;

        #region [ Настройки преследования и движения ]
        [Header("Движение и Прицеливание")]
        [Tooltip("Скорость поворота босса к игроку")]
        [SerializeField] private float _rotationSpeed = 5f;
        [Tooltip("Смещение по высоте для прицеливания (чтобы смотрел в грудь/голову)")]
        [SerializeField] private float _aimHeightOffset = 1.2f;
        #endregion

        #region [ Настройки дистанций ]
        [Header("Дистанции")]
        [Tooltip("Дистанция, на которой босс переходит в рукопашный бой")]
        [SerializeField] private float _meleeAttackRange = 3f;
        [Tooltip("Буферная зона, чтобы босс не дергался туда-сюда на границе ближнего боя")]
        [SerializeField] private float _meleeDisengageBuffer = 0.5f;
        #endregion

        #region [ Настройки дальней атаки в движении ]
        [Header("Дальняя атака (во время погони)")]
        [Tooltip("Минимальное время между выстрелами на ходу")]
        [SerializeField] private float _chaseAttackMinTimer = 7f;
        [Tooltip("Максимальное время между выстрелами на ходу")]
        [SerializeField] private float _chaseAttackMaxTimer = 10f;
        [Tooltip("На сколько умножается скорость босса при выстреле (0.85 = минус 15%)")]
        [Range(0.5f, 1f)]
        [SerializeField] private float _chaseAttackSlowdown = 0.85f;
        [Tooltip("Длительность анимации/действия выстрела в движении")]
        [SerializeField] private float _chaseAttackDuration = 1f;
        [Tooltip("Оружие для дальней атаки")]
        [SerializeField] private Gun _acidGun;
        #endregion

        #region [ Настройки ближнего боя ]
        [Header("Ближний бой (Оружие)")]
        [Tooltip("Время перезарядки между атаками ближнего боя")]
        [SerializeField] private float _meleeCooldown = 2.5f;

        [Space]
        [Tooltip("Может ли босс использовать комбо (несколько обычных ударов подряд)?")]
        [SerializeField] private bool _enableComboAttacks = false;
        [Tooltip("Длительность обычного удара (или серии ударов)")]
        [SerializeField] private float _lightAttackTime = 2f;
        [SerializeField] private float _lightWaitAttackTime = 0.3f;

        [Space]
        [Tooltip("Шанс использовать заряженную атаку (0 - никогда, 1 - всегда)")]
        [Range(0f, 1f)]
        [SerializeField] private float _heavyAttackChance = 0f;
        [Tooltip("Время подготовки (зарядки) тяжелой атаки")]
        [SerializeField] private float _heavyChargeTime = 1.5f;
        [Tooltip("Время самого удара после зарядки")]
        [SerializeField] private float _heavyAttackTime = 1.0f;

        [SerializeField] private Animator _anmtr;
        #endregion

        #region [ Настройки лечения ]
        [Header("Фаза лечения")]
        [Tooltip("Процент здоровья (от 0 до 1), при котором босс включает хил")]
        [Range(0f, 1f)]
        [SerializeField] private float _healThresholdPercent = 0.3f;
        [Tooltip("Как босс двигается во время лечения?")]
        [SerializeField] private HealMovementMode _healMovementMode = HealMovementMode.Stop;
        [Tooltip("Множитель скорости, если выбран режим Slow (0.3 = 30% от скорости)")]
        [Range(0f, 1f)]
        [SerializeField] private float _healSpeedMultiplier = 0.3f;

        [Space]
        [Tooltip("Ссылка на эффект огня, чтобы проверять его статус IsHealing")]
        [SerializeField] private HealingFire _healingEffect;
        [Tooltip("Событие, вызываемое при старте лечения (например, для звука)")]
        [SerializeField] private UnityEvent _onHealPhaseTriggered;
        #endregion

        private NPCEffectHandler _effectHandler;
        private bool _hasHealed = false;
        private bool _isAttacking = false;

        private float lasAtck;
        private float _lastMeleeAttackTime;
        private float _chaseAttackTimer;
        private float _nextChaseAttackTarget;
        private float _originalSpeed;
        private bool _isSlowedForRanged = false;

        protected override void Awake()
        {
            base.Awake();
            _effectHandler = GetComponent<NPCEffectHandler>();
            if (_agent != null) _agent.stoppingDistance = 0f;
            SetNextChaseAttackTimer();
        }

        protected virtual void Start()
        {
            if (_agent != null) _originalSpeed = _agent.speed;
            // Ищем эффект, который Handler создал при старте
            _healingEffect = GetComponentInChildren<HealingFire>();
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

            // Если босс стоит во время хила, он даже не поворачивается
            if (_healingEffect != null && _healingEffect.IsHealing && _healMovementMode == HealMovementMode.Stop)
                return;

            if (_aiState != AIState.Idle) RotateTowardsTarget();
        }

        protected override void ExecuteFSM()
        {
            if (_target == null || _bodyState == BodyState.Dead) return;

            ApplyMovementLogic();

            // Если хилимся и стоим на месте - блокируем остальную логику
            if (_healingEffect != null && _healingEffect.IsHealing && _healMovementMode == HealMovementMode.Stop)
                return;

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

        private void ApplyMovementLogic()
        {
            if (_agent == null) return;

            bool isHealing = _healingEffect != null && _healingEffect.IsHealing;

            // Приоритет 1: Движение при лечении
            if (isHealing)
            {
                if (_healMovementMode == HealMovementMode.Stop)
                {
                    _agent.isStopped = true;
                    return;
                }
                if (_healMovementMode == HealMovementMode.Slow)
                {
                    _agent.isStopped = false;
                    _agent.speed = _originalSpeed * _healSpeedMultiplier;
                    return;
                }
            }

            // Приоритет 2: Замедление при выстреле на ходу
            if (_isSlowedForRanged && _aiState == AIState.Chasing)
            {
                _agent.isStopped = false;
                _agent.speed = _originalSpeed * _chaseAttackSlowdown;
                return;
            }
            _agent.speed = _originalSpeed;
        }

        private void ChaseLogic(float distance)
        {
            _agent.SetDestination(_target.position);

            _anmtr.SetBool("Walking", true);

            if (distance <= _meleeAttackRange)
            {
                _aiState = AIState.Attacking;
                return;
            }

            // Логика стрельбы на ходу раз в 7-10 секунд
            if (!_isAttacking && !_isSlowedForRanged)
            {
                _chaseAttackTimer += Time.deltaTime;
                if (_chaseAttackTimer >= _nextChaseAttackTarget)
                {
                    StartCoroutine(ChaseAttackRoutine());
                }
            }
        }

        private IEnumerator ChaseAttackRoutine()
        {
            _isSlowedForRanged = true;

            if (_acidGun != null)
            {
                _acidGun.ShootAt(GetAimPoint());
            }

            yield return new WaitForSeconds(_chaseAttackDuration);

            _isSlowedForRanged = false;
            SetNextChaseAttackTimer();
        }

        private void SetNextChaseAttackTimer()
        {
            _chaseAttackTimer = 0f;
            _nextChaseAttackTarget = Random.Range(_chaseAttackMinTimer, _chaseAttackMaxTimer);
        }

        private void MeleeCombatLogic(float distance)
        {
            _idleAudioBehavior.PlayEffect(true);

            // Выход из ближнего боя, если игрок убежал далеко
            if (distance > _meleeAttackRange + _meleeDisengageBuffer)
            {
                _aiState = AIState.Chasing;
                _idleAudioBehavior.PlayEffect(true);
                return;
            }
            _idleAudioBehavior.PlayEffect(false);

            if (_isAttacking) return;
            if (_healingEffect != null && _healingEffect.IsHealing) return;
            
            bool useHeavy = Random.value <= _heavyAttackChance;
            StartCoroutine(MeleeAttackRoutine(useHeavy));
        }

        private IEnumerator MeleeAttackRoutine(bool isHeavy)
        {
            _isAttacking = true;
            _lastMeleeAttackTime = Time.time;

            if (_weaponHeader != null)
            {
                if (isHeavy)
                {
                    // Подготовка тяжелой атаки
                    _weaponHeader.StartSecondaryAttack();
                    yield return new WaitForSeconds(_heavyChargeTime);
                    // Сам удар
                    _weaponHeader.StopSecondaryAttack();
                    yield return new WaitForSeconds(_heavyAttackTime);
                }
                else
                {
                    if (Time.time - lasAtck > _meleeCooldown)
                    {
                        _agent.isStopped = true;
                        StartCoroutine(ReturnWalkAbility(1));
                        lasAtck = Time.time;
                        _anmtr.SetTrigger("Atck");
                        print("ATACKIIIINg");

                        yield return new WaitForSeconds(_lightWaitAttackTime);
                        // Легкая атака (или комбо)
                        _weaponHeader.StartPrimaryAttack();
                    }
                }
            }

            _isAttacking = false;
        }

        private IEnumerator ReturnWalkAbility(float after)
        {
            yield return new WaitForSeconds(after);
            _agent.isStopped = false;
        }

        private void RotateTowardsTarget()
        {
            Vector3 targetPos = _target.position;
            Vector3 dir = (targetPos - transform.position).normalized;

            Vector3 bodyDir = new Vector3(dir.x, 0, dir.z);
            if (bodyDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(bodyDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _rotationSpeed);
            }

            Vector3 aimPoint = targetPos + Vector3.up * _aimHeightOffset;
            if (_weaponHeader != null) _weaponHeader.transform.LookAt(aimPoint);
            if (_acidGun != null) _acidGun.transform.LookAt(aimPoint);
        }

        private Vector3 GetAimPoint()
        {
            if (_target == null) return transform.position + transform.forward;
            // Возвращаем позицию игрока + смещение по высоте
            return _target.position + Vector3.up * _aimHeightOffset;
        }

        private void CheckHealPhase()
        {
            if (_health == null) return;

            float currentHealthPercent = _health.CurHealth / _health.MaxHealth;
            if (currentHealthPercent <= _healThresholdPercent)
            {
                _hasHealed = true;
                _onHealPhaseTriggered?.Invoke();

                // Симулируем нажатие кнопки боссом!
                if (_effectHandler != null)
                {
                    _effectHandler.TriggerActiveEffects();
                }
            }
        }

        private void OnEnable()
        {
            _idleAudioBehavior.PlayEffect(false);
        }
    }
}