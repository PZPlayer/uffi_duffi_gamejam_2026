using Jam.Effects;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectInfo", menuName = "Scriptable Objects/EffectInfo")]
public class EffectInfo : ScriptableObject
{
    public string EffectName;
    public string EffectDescription;
    public string EffectStatus;
    public string EffectCallKey;
    public float ContinueTime;
    public float Damage;
    public Sprite EffectImage;
    public bool IfOnlyForPlayer;
}
