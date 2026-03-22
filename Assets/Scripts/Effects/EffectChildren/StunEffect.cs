using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

namespace Jam.Effects
{
    public class StunEffect : IdleEffect, IPassive
    {
        private FirstPersonController playerController;
        private NPCBehavior npcController;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);
            _startTime = Time.time;

            playerController = GetComponent<FirstPersonController>();
            npcController = GetComponent<NPCBehavior>();

            CondemMovement();
        }

        public override void OnPassiveUpdate()
        {
            base.OnPassiveUpdate();

            if (Time.time - _startTime >= EffectInfo.ContinueTime)
            {
                RemoveSelf();
            }
        }

        private void RemoveSelf()
        {
            _times -= 1;

            if (_times > 0)
            {
                _startTime = Time.time;
                return;
            }

            if (handler != null)
            {
                handler.RemoveEffect(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void CondemMovement()
        {
            if (npcController != null)
            {
                GetComponent<NavMeshAgent>().isStopped = true;
                npcController.enabled = false;
            }
            else if (playerController != null)
            {
                playerController.CanMove = false;
            }
            else
            {
                Debug.LogError("WTF?? WHERE AM I?? " + transform.gameObject);
            }
        }

        private void ReleaseMovement()
        {
            if (npcController != null)
            {
                GetComponent<NavMeshAgent>().isStopped = false;
                npcController.enabled = true;
            }
            else if (playerController != null)
            {
                playerController.CanMove = true;
            }
            else
            {
                Debug.LogError("WTF?? WHERE AM I?? " + transform.gameObject);
            }
        }

        protected override void OnDestroy()
        {
            ReleaseMovement();

            base.OnDestroy();
        }
    }
}