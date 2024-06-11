using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public UnityEvent<string, int> submitScoreEvent;

    public void SubmitScore(string userName, int userScore)
    {
        submitScoreEvent.Invoke(userName, userScore);
    }
}
