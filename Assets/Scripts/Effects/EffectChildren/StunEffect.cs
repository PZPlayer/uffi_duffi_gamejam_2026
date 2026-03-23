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

            print("I HAVE " + _times + " times. I am on the " + gameObject);
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

        public override void Refresh(IdleEffect newEffectData)
        {
            _times += 1;
        }

        private void RemoveSelf()
        {
            _times -= 1;

            if (_times > 0)
            {
                _startTime = Time.time;
                return;
            }

            print(_times);

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
                Destroy(gameObject);
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
                Debug.LogError("WTF?? WHERE AM I?? " + transform.gameObject.name);
            }
        }

        protected override void OnDestroy()
        {
            ReleaseMovement();

            base.OnDestroy();
        }
    }
}