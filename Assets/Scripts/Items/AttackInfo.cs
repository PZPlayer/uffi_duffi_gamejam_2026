using UnityEngine;

[CreateAssetMenu(fileName = "AttackInfo", menuName = "Scriptable Objects/AttackInfo")]
public class AttackInfo : ScriptableObject
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _attackAnimation;
    public AnimationClip AttackAnimation => _attackAnimation;

    [Header("Damage")]
    [SerializeField] private float _damage = 10f;
    public float Damage => _damage;

    [Header("Timing")]
    [SerializeField] private float _reloadTime = 0.5f;
    public float ReloadTime => _reloadTime;

    [Header("Optional Effects")]
    [SerializeField] private AudioClip _attackSound;
    public AudioClip AttackSound => _attackSound;

    [SerializeField] private GameObject _hitEffect;
    public GameObject HitEffect => _hitEffect;
}