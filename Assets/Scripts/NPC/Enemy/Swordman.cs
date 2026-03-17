using UnityEngine;

namespace Jam.NPCSystem
{
    public class Swordman : NPCBehavior
    {
        protected override void ExecuteFSM()
        {
            if (_target == null)
            {
                if (_aiState != AIState.Idle)
                {
                    _aiState = AIState.Idle;
                    if (_agent != null) _agent.isStopped = true;
                }
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, _target.position);
            float range = _weaponHeader != null ? _weaponHeader.AttackRange : 2.0f;

            switch (_aiState)
            {
                case AIState.Idle:
                    if (_target != null)
                    {
                        _aiState = AIState.Chasing;
                        AlertNearbyAllies();
                    }
                    break;

                case AIState.Chasing:
                    if (_agent != null)
                    {
                        _agent.isStopped = false;
                        _agent.SetDestination(_target.position);
                    }

                    if (distanceToTarget <= range)
                    {
                        _aiState = AIState.Attacking;
                    }
                    break;

                case AIState.Attacking:
                    if (_agent != null) _agent.isStopped = true;

                    LookAtTarget();

                    if (_weaponHeader != null)
                    {
                        _weaponHeader.Attack();
                    }

                    if (distanceToTarget > range + 0.5f)
                    {
                        _aiState = AIState.Chasing;
                    }
                    break;
            }
        }

        private void LookAtTarget()
        {
            if (_target == null) return;

            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 10f
                );
            }
        }
    }
}