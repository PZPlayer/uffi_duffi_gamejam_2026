using Jam.NPCSystem;
using UnityEngine;

public class Swordman : NPCBehavior
{
    protected override void ExecuteFSM()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        float range = _weaponHeader.AttackRange;

        switch (_aiState)
        {
            case AIState.Idle:
                if (IsPlayerInVision())
                {
                    _aiState = AIState.Chasing;
                    AlertNearbyAllies(); // Звеним в колокол!
                }
                break;

            case AIState.Chasing:
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);

                if (distanceToPlayer <= range)
                {
                    _aiState = AIState.Attacking;
                }
                break;

            case AIState.Attacking:
                _agent.isStopped = true;
                LookAtPlayer();
                _weaponHeader.Attack();

                if (distanceToPlayer > range + 0.5f)
                {
                    _aiState = AIState.Chasing;
                }
                break;
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
    }
}