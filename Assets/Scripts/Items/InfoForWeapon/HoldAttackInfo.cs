using UnityEngine;

namespace Jam.Items
{
    [CreateAssetMenu(fileName = "HoldAttackInfo", menuName = "Scriptable Objects/HoldAttackInfo")]
    public class HoldAttackInfo : ScriptableObject
    {
        [Header("Animation Clips")]
        [SerializeField] private AnimationClip _startClip;
        [SerializeField] private AnimationClip _loopClip;
        [SerializeField] private AnimationClip _endClip;

        [Header("Damage Settings")]
        [SerializeField] private float _minDamage = 10f;
        [SerializeField] private float _maxDamage = 50f;
        [SerializeField] private float _chargeTime = 2.0f;

        [Header("Timing")]
        [SerializeField] private float _postAttackDelay = 0.2f;

        // Свойства для доступа к полям
        public AnimationClip StartClip => _startClip;
        public AnimationClip LoopClip => _loopClip;
        public AnimationClip EndClip => _endClip;
        public float MinDamage => _minDamage;
        public float MaxDamage => _maxDamage;
        public float ChargeTime => _chargeTime;
        public float PostAttackDelay => _postAttackDelay;
    }
}
