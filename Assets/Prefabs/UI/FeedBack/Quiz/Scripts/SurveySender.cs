using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SurveySender : MonoBehaviour
{
    [SerializeField] private UnityEvent _onSend; 
    [SerializeField] private UnityEvent _onError; 

    [Header("Answers")]
    [SerializeField] private List<Answer> answers = new List<Answer>();

    [SerializeField] private string gameName;

    [Header("Telegram")]
    [SerializeField] private string botToken;
    [SerializeField] private string chatId;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SendSurvey()
    {
        string finalText = "";
        finalText += "#" + gameName + "\n";

        for (int i = 0; i < answers.Count; i++)
        {
            if (answers[i] == null) continue;

            string answerText = answers[i].OnGetAnswers();
               
            finalText += "[" + answers[i].Index.ToString() + "] ";
            finalText += answers[i].Name + ": ";
            finalText += answerText;
            finalText += "\n\n";
        }

        StartCoroutine(SendToTelegram(finalText));
    }

    IEnumerator SendToTelegram(string message)
    {
        string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

        WWWForm form = new WWWForm();
        form.AddField("chat_id", chatId);
        form.AddField("text", message);

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            _onError?.Invoke();
            Debug.LogError("Telegram send error: " + request.error);
        }
        else
        {
            _onSend?.Invoke();
            Debug.Log("Survey sent successfully");
        }
    }
}