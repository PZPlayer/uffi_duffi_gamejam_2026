using UnityEngine;
using UnityEngine.UI;

public class SliderAnswer : Answer
{
    [SerializeField] private Slider slider; 

    public override string OnGetAnswers()
    {
        return slider != null ? slider.value.ToString() : "0";
    }
}