using UnityEngine;

[CreateAssetMenu(fileName = "AttackInfo", menuName = "Scriptable Objects/AttackInfo")]
public class AttackInfo : ScriptableObject
{
    [Header("Animation")]
    [SerializeField] protected AnimationClip _attackAnimation;
    public AnimationClip AttackAnimation => _attackAnimation;

    [Header("Damage")]
    [SerializeField] protected float _damage = 10f;
    public float Damage => _damage;

    [Header("Timing")]
    [SerializeField] protected float _reloadTime = 0.5f;
    public float ReloadTime => _reloadTime;

    [Header("Optional Effects")]
    [SerializeField] protected AudioClip _attackSound;
    public AudioClip AttackSound => _attackSound;

    [SerializeField] protected GameObject _hitEffect;
    public GameObject HitEffect => _hitEffect;
}