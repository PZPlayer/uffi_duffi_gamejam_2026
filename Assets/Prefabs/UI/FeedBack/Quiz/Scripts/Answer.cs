using UnityEngine;

public abstract class Answer : MonoBehaviour
{
    public string Name;
    public int Index;

    public abstract string OnGetAnswers();
}