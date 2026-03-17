using Jam.HealthSystem;
using System;
using UnityEngine;

namespace Jam.Effects.EffectChildren
{
    public class Poison : IdleEffect, IPassive
    {
        private Health health;
        private Color originalFlashColor;
        private SimpleDamageFlash flash;
        private bool _isMarkedForRemoval = false;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            _startTime = Time.time;

            if (TryGetComponent(out SimpleDamageFlash simple))
            {
                flash = simple;
                originalFlashColor = flash._flashColor;
                flash._flashColor = Color.darkGreen;
            }
            

            health = GetComponent<Health>();
        }

        public override void OnPassiveUpdate()
        {
            if (_isMarkedForRemoval) { print("Destroy"); Destroy(this); }

            if (Time.time - _startTime > _effectInfo.ContinueTime)
            {
                print($"Poison time expired, removing at {Time.time}");
                RemoveSelf();
                return;
            }

            base.OnPassiveUpdate();

            if (health != null)
            {
                health.Damage(_effectInfo.Damage);
            }
        }

        private void RemoveSelf()
        {
            if (_isMarkedForRemoval) return;

            _times -= 1;

            if (_times > 0)
            {
                _startTime = Time.time;
                return;
            }

            _isMarkedForRemoval = true;


            if (flash != null)
                flash._flashColor = originalFlashColor;

            if (handler != null)
            {
                handler.RemoveEffect(this);
            }
            else
            {
                Destroy(this);
            }
        }

        public override void Refresh(IdleEffect newEffectData)
        {
            Poison newPoison = newEffectData as Poison;
            if (newPoison != null)
            {
                _times += 1;
            }
        }

        protected override void OnDestroy()
        {
            print($"Poison OnDestroy at {Time.time}");
            base.OnDestroy();
        }
    }
}