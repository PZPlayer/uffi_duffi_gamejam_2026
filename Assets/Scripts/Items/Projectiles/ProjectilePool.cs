using System.Collections.Generic;
using UnityEngine;

namespace Jam.Items
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance;

        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _poolSize = 50;
        private Queue<GameObject> _pool = new Queue<GameObject>();

        private void Awake()
        {
            Instance = this;
            for (int i = 0; i < _poolSize; i++)
            {
                GameObject obj = Instantiate(_prefab, transform);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public GameObject Get()
        {
            if (_pool.Count == 0) return null;
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
