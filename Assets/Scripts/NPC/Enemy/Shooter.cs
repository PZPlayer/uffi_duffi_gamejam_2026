using UnityEngine;

namespace Jam.NPCSystem
{
    public class Shooter : NPCBehavior
    {
        [Header("Shooter Settings")]
        [SerializeField] protected float _stopDistance = 12f;
        [SerializeField] protected float _retreatDistance = 5f;
        [SerializeField] protected float _attackCooldown = 1.5f;

        private float _nextAttackTime;

        protected override void ExecuteFSM()
        {
            // ≈сли цель погибла или исчезла Ч возвращаемс€ в Idle
            if (_target == null)
            {
                _aiState = AIState.Idle;
                if (_agent != null) _agent.isStopped = true;
                return;
            }

            float dist = Vector3.Distance(transform.position, _target.position);

            switch (_aiState)
            {
                case AIState.Idle:
                    // ѕоиск цели уже идет в базовом Update
                    if (_target != null) _aiState = AIState.Chasing;
                    break;

                case AIState.Chasing:
                    _agent.isStopped = false;
                    _agent.SetDestination(_target.position);

                    if (dist <= _stopDistance)
                        _aiState = AIState.Attacking;
                    break;

                case AIState.Attacking:
                    if (_agent != null) _agent.isStopped = true;

                    HandleCombat();

                    if (dist < _retreatDistance)
                        _aiState = AIState.Retreating;
                    else if (dist > _stopDistance + 2f)
                        _aiState = AIState.Chasing;
                    break;

                case AIState.Retreating:
                    _agent.isStopped = false;
                    Vector3 retreatDir = (transform.position - _target.position).normalized;
                    _agent.SetDestination(transform.position + retreatDir * 5f);

                    HandleCombat();

                    if (dist > _stopDistance)
                        _aiState = AIState.Attacking;
                    break;
            }
        }

        private void HandleCombat()
        {
            LookAtTarget();

            if (Time.time >= _nextAttackTime)
            {
                if (_weaponHeader != null)
                {
                    _weaponHeader.Attack();
                }
                _nextAttackTime = Time.time + _attackCooldown;
            }
        }

        private void LookAtTarget()
        {
            Vector3 dir = (_target.position - transform.position).normalized;
            Vector3 bodyDir = new Vector3(dir.x, 0, dir.z);

            if (bodyDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(bodyDir), Time.deltaTime * 10f);
            }
            if (_weaponHeader != null)
            {
                Vector3 targetPoint = _target.position + Vector3.up * 1.2f;
                _weaponHeader.transform.LookAt(targetPoint);
            }
        }
    }
}