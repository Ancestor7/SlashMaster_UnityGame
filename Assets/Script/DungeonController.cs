using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonController : MonoBehaviour
{
    [SerializeField] GameObject DarkScreen;
    private Image darkScreen;

    [SerializeField] GameObject dungeonRoomPrefab;

    [Header("Player Settings")]
    [SerializeField] float PlayerHealth;
    [SerializeField] float PlayerMoney;
    private float RoomScore;
    private List<Item> items;

    void Start()
    {
        Player.Instance.EnterDungeon();
        darkScreen = DarkScreen.GetComponent<Image>();
        darkScreen.color = new Color(darkScreen.color.r, darkScreen.color.g, darkScreen.color.b, 1f);
        LightenScreen();
        PlayerHealth = 10f;
        PlayerMoney = 0f;
        RoomScore = 0f;

    }

    void Update()
    {
        
    }

    public void DarkenScreen()
    {
        StartCoroutine(CoDarkenScreen());
    }

    private IEnumerator CoDarkenScreen()
    {
        yield return new WaitForSeconds(0.5f);
        float duration = 2f, elapsed = 0f;
        Color initialColor = darkScreen.color, targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreen.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreen.color = targetColor;
    }

    public void LightenScreen()
    {
        StartCoroutine(CoLightenScreen());
    }

    private IEnumerator CoLightenScreen()
    {
        yield return new WaitForSeconds(0.5f);
        float duration = 2f, elapsed = 0f;
        Color initialColor = darkScreen.color, targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreen.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreen.color = targetColor;
    }

    /*private void EnterPlayerToDungeon()
    {
        if (Player.Instance != null)
        {
            Player.Instance.EnterDungeon();
        }
    }*/

    enum RoomType
    {
        Monster,
        Boss,
        Trap,
        Shop,
        Fountain,
        Chest
    }

    private void SpawnRoom()
    {

    }

    enum Item
    {
        AtkBoost,
        DefBoost,
        TimeSlow,
        Potion,
        Revive,
        RawDmg,
        InstaKill,
        Run
    }
}
