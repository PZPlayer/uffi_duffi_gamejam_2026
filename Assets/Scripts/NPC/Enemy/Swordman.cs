using UnityEngine;

namespace Jam.NPCSystem
{
    public class Swordman : NPCBehavior
    {
        [SerializeField] private Animator _anmtr;

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
                    _anmtr.SetBool("Walking", false);
                    if (_target != null)
                    {
                        _aiState = AIState.Chasing;
                        AlertNearbyAllies();
                    }
                break;

                case AIState.Chasing:
                    _anmtr.SetBool("Walking", true);

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
                        // ¬место Attack() используем StartPrimaryAttack()
                        _anmtr.SetTrigger("Atck");
                        _weaponHeader.StartPrimaryAttack();
                    }

                    if (distanceToTarget > range + 0.5f)
                    {
                        // ѕеред уходом в погоню Ч ќЅя«ј“≈Ћ№Ќќ "отпускаем" палку
                        if (_weaponHeader != null) _weaponHeader.StopPrimaryAttack();
                        _aiState = AIState.Chasing;
                    }
                break;
            }
        }

        private void LookAtTarget()
        {
            if (_target == null) return;

            Vector3 direction = (_target.position - transform.position).normalized;
            Vector3 bodyDir = new Vector3(direction.x, 0, direction.z);

            if (bodyDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(bodyDir), Time.deltaTime * 10f);
            }

            if (_weaponHeader != null)
            {
                _weaponHeader.transform.LookAt(_target.position + Vector3.up * 1f);
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            // ќб€зательно вызываем базу, чтобы видеть радиус зрени€ (желтый)
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.red; //  расный дл€ ближнего бо€

            if (TryGetComponent<WeaponNPCHeader>(out var header))
            {
                // –исуем сферу радиуса атаки
                Gizmos.DrawWireSphere(transform.position, header.AttackRange);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 2.0f);
            }
        }
    }
}