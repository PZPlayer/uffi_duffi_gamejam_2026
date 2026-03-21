using Jam.Items;
using UnityEngine;

namespace Jam.Effects
{
    public class Chi : IdleEffect, ICallable
    {
        [SerializeField] private GameObject _projectile;

        private ItemsHandler _handler;
        private ProjectilePool _pool;
        private float _lastTimeFire;

        public override void Initilize(EffectHandler handlerEffect)
        {
            base.Initilize(handlerEffect);

            Subsribe();
        }

        public void Subsribe()
        {
            _handler = Camera.main.GetComponent<ItemsHandler>();

            if( _handler == null )
            {
                Debug.LogError("No handler found!");
                return;
            }
            _handler.OnLeftMouseAction += Shoot;
        }

        private void Shoot()
        {
            if (_projectile == null || (Time.time - _lastTimeFire) < EffectInfo.ReloadTime) return;

            if (_pool == null)
                _pool = Instantiate(_projectile).GetComponent<ProjectilePool>();

            GameObject projctl = _pool.Get();
            projctl.transform.position = Camera.main.transform.position;
            projctl.transform.rotation = Camera.main.transform.rotation;
            projctl.GetComponent<Projectile>().Initialize(gameObject, Camera.main.transform.forward, _pool);
            projctl.SetActive(true);
            _lastTimeFire = Time.time;
        }

        public void UnSubsribe()
        {
            _handler.OnLeftMouseAction -= Shoot;
        }
    }
}
