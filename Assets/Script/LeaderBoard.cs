using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dan.Main;

public class LeaderBoard : MonoBehaviour
{
    public static LeaderBoard Instance { get; private set; }

    [SerializeField] private List<TextMeshProUGUI> ranks;
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private List<TextMeshProUGUI> scores;

    [SerializeField] private TextMeshProUGUI playerRank;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerScore;

    private string publicKey = "d9043619c48a1c25d24f3c9b5f6f61579a097370d86939e64666029de0857ced";

    public void GetLeaderBoard()
    {
        
        LeaderboardCreator.GetLeaderboard(publicKey, ((msg) =>
        {
            for (int i = 0; i < names.Count; i++)
            {
                names[i].text = msg[i].Username;
                scores[i].text = msg[i].Score.ToString();
            }
        }));

        if (PlayerPrefs.GetString("username") == null)
        {
            playerRank.text = "0";
            playerName.text = "";
            playerScore.text = "0";
        }
        else
        {
            
        }
    }

    public void SetLeaderboardEntry(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(publicKey, username, score, ((msg) =>
        {
            GetLeaderBoard();
        }));
    }

    /*public void QueryLeaderBoard()
    {
        LeaderboardCreator.GetPersonalEntry(publicKey,)
    }*/
}
