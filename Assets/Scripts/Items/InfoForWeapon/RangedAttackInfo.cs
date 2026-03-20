using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttackInfo", menuName = "Scriptable Objects/RangedAttackInfo")]
public class RangedAttackInfo : AttackInfo
{
    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpeed = 30f;
    [SerializeField] private float _lifeTime = 3f; // Время жизни снаряда до возврата в пул

    public float ProjectileSpeed => _projectileSpeed;
    public float LifeTime => _lifeTime;
}
