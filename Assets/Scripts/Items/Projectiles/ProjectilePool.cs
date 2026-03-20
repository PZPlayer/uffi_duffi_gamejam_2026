using System.Collections.Generic;
using UnityEngine;

namespace Jam.Items
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private string _poolName = "Global Projectile Pool";
        [SerializeField] private GameObject _projectile;
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private bool _autoExpand = true;

        private Queue<GameObject> _pool = new Queue<GameObject>();

        private Transform _container;

        private void Awake()
        {
            EnsureContainerExists();

            for (int i = 0; i < _poolSize; i++)
            {
                AddProjectile();
            }
        }

        private void EnsureContainerExists()
        {
            if (_container == null)
            {
                _container = new GameObject($"[{_poolName}]").transform;
            }
        }

        public GameObject Get()
        {
            if (_pool.Count == 0)
            {
                if (_autoExpand) AddProjectile();
                else return null;
            }

            GameObject obj = _pool.Dequeue();
            if (obj == null) return Get();

            return obj;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        private void AddProjectile()
        {
            GameObject obj = Instantiate(_projectile, _container);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        private void OnDestroy()
        {
            while (_pool != null && _pool.Count > 0)
            {
                GameObject obj = _pool.Dequeue();
                if (obj != null) Destroy(obj);
            }

            if (_container != null)
            {
                Destroy(_container.gameObject);
            }
        }
    }
}