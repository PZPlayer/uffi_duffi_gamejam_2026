using TMPro;
using UnityEngine;

public class InputFieldAnswer : Answer
{
    [SerializeField] private TMP_InputField inputField; 

    public override string OnGetAnswers()
    {
        return inputField != null ? inputField.text : string.Empty;
    }
}