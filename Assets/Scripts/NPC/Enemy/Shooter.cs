using UnityEngine;
using Jam.NPCSystem;

namespace Jam.NPCSystem
{
    using UnityEngine;
    using Jam.NPCSystem;

    public class Shooter : NPCBehavior
    {
        [Header("Shooter Settings")]
        [SerializeField] protected float _stopDistance = 12f;
        [SerializeField] protected float _retreatDistance = 5f;

        protected override void ExecuteFSM()
        {
            if (_player == null) return;
            float dist = Vector3.Distance(transform.position, _player.position);

            switch (_aiState)
            {
                case AIState.Idle:
                    if (IsPlayerInVision()) { _aiState = AIState.Chasing; AlertNearbyAllies(); }
                    break;
                case AIState.Chasing:
                    _agent.SetDestination(_player.position);
                    if (dist <= _stopDistance) _aiState = AIState.Attacking;
                    break;
                case AIState.Attacking:
                    _agent.isStopped = true;
                    LookAtPlayer();
                    if (_weaponHeader != null) _weaponHeader.Attack();
                    if (dist < _retreatDistance) _aiState = AIState.Retreating;
                    else if (dist > _stopDistance + 2f) _aiState = AIState.Chasing;
                    break;
                case AIState.Retreating:
                    _agent.SetDestination(transform.position + (transform.position - _player.position).normalized * 5f);
                    if (dist > _stopDistance) _aiState = AIState.Attacking;
                    break;
            }
        }

        private void LookAtPlayer()
        {
            Vector3 dir = (_player.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }
}