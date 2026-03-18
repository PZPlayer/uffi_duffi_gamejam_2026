using System;
using UnityEngine;
using UnityEngine.Events;

namespace Jam.HealthSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _curHealth;

        public float MaxHealth { get => _maxHealth; set => _maxHealth = Mathf.Max(value, 0); }
        public float CurHealth { get => _curHealth; set => _curHealth = Mathf.Max(value, 0); }

        public event Action<float> HealthChanged;
        public event Action<GameObject> OnDamaged;
        public event Action Death;
        public UnityEvent OnDeath;


        public void Damage(float damage, GameObject attacker = null)
        {
            _curHealth = Mathf.Max(0, _curHealth - damage);
            HealthChanged?.Invoke(damage * -1);
            OnDamaged?.Invoke(attacker);

            if (_curHealth <= 0)
            {
                Death?.Invoke();
                OnDeath?.Invoke();
            }
        }

        public void Heal(float value)
        {
            _curHealth = Mathf.Min(_maxHealth, _curHealth + value);
            HealthChanged?.Invoke(value);
        }
    }
}

