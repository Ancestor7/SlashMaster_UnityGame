using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonController : MonoBehaviour
{
    #region Enums
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

    enum RoomType
    {
        Monster,  // 60
        Boss,     // 10
        Shop,     // 20
        Bonfire, // 10
        Chest     // 10
    }

    #endregion

    [Header("Objects")]
    [SerializeField] private GameObject DebugCamera;
    [SerializeField] private GameObject DarkScreen;
    [SerializeField] private GameObject PlayerHud;

    [Header("Prefabs")]
    [SerializeField] private GameObject DungeonRoomPrefab;
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private GameObject ShopStallPrefab;
    [SerializeField] private GameObject BonfirePrefab;
    [SerializeField] private GameObject ChestPrefab;
    [SerializeField] private GameObject BossPrefab;

    private Image darkScreenImage;
    private GameObject currentRoomObject = null, nextRoomObject = null;

    [Header("Player Settings")]
    [SerializeField] private float playerHealth;
    [SerializeField] private float playerMoney;
    [SerializeField] private float roomSpeed;

    private int playerScore;
    private int roomNumber;
    private RoomType? currentRoomType = null, nextRoomType = null;
    private List<Item> playerItems = new();
    private bool playerIsAtEnterance;

    void Start()
    {
        roomSpeed = roomSpeed != 0 ? roomSpeed : 2.5f;
        DarkScreen.SetActive(true);
        DebugCamera.SetActive(false);
        EnterPlayerToDungeon();
        InitializePlayerData();
        darkScreenImage = DarkScreen.GetComponent<Image>();
        darkScreenImage.color = new Color(darkScreenImage.color.r, darkScreenImage.color.g, darkScreenImage.color.b, 1f);
        LightenScreen();
        CreateRoom();
        RoomBegin();
    }

    void Update()
    {
        
    }

    #region PlayerFunctions

    private void InitializePlayerData()
    {
        playerHealth = 10f;
        playerMoney = 0f;
        roomNumber = 0;
        playerScore = 0;
        playerItems.Clear();
    }

    private void EnterPlayerToDungeon()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = new Vector3(0, 1.5f, -2f);
            Player.Instance.mainCamera.fieldOfView += 40f;
            playerIsAtEnterance = true;
        }
        else
        {
            Debug.LogError("PlayerNotFound");
        }
    }

    private void MovePlayerFurtherIn()
    {
        StartCoroutine(CoMovePlayer(false));
    }

    private void MovePlayerToNextRoom()
    {
        StartCoroutine(CoMovePlayer(true));
    }

    private IEnumerator CoMovePlayer(bool isMovingToNextRoom)
    {
        float moveDistance;
        if (playerIsAtEnterance)
        {
            if (isMovingToNextRoom)
            {
                moveDistance = -8f;
                playerIsAtEnterance = true;

            }
            else
            {
                moveDistance = -2f;
                playerIsAtEnterance = false;
            }
        }
        else
        {
            moveDistance = -6f;
            playerIsAtEnterance = true;
        }

        float destinationPos = transform.position.z + moveDistance;
        yield return new WaitForSeconds(1f);
        while (transform.position.z > destinationPos)
        {   
            yield return null;
            transform.position -= roomSpeed * Time.deltaTime * transform.forward;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, destinationPos);

        if ((currentRoomType == RoomType.Boss || currentRoomType == RoomType.Monster && !isMovingToNextRoom))
        {
            ReadyEnemy();
        }

    }

    private void UpdateScoreAndSpeed(int roomIncrement = 1)
    {
        roomNumber += roomIncrement;
        switch (currentRoomType)
        {
            case RoomType.Boss:
                playerScore += 5;
                break;
            case RoomType.Monster:
                playerScore += 2;
                break;
            default:
                break;
        }
        playerScore += 1;

        PlayerHud.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Room " + roomNumber.ToString();
        PlayerHud.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "Score: " + playerScore.ToString();

        // TODO: Increase room speed by room number
    }

    #endregion

    #region ScreenEffects
    public void DarkenScreen()
    {
        StartCoroutine(CoDarkenScreen());
    }

    private IEnumerator CoDarkenScreen()
    {
        yield return new WaitForSeconds(0.5f);
        float duration = 2f, elapsed = 0f;
        Color initialColor = darkScreenImage.color, targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreenImage.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreenImage.color = targetColor;
    }

    public void LightenScreen()
    {
        StartCoroutine(CoLightenScreen());
    }

    private IEnumerator CoLightenScreen()
    {
        while (darkScreenImage.color.a != 0)
        {
            darkScreenImage.color = Color.Lerp(darkScreenImage.color, Color.clear, Time.deltaTime * 2f);
            yield return null;
        }
        darkScreenImage.color = Color.clear;
    }
    #endregion

    #region RoomFunctions

    private void CreateRoom(){
        /* Place the start room or next room */
        if (currentRoomObject != null && nextRoomObject != null)
        {
            GameObject newRoom = InstantiateRoom(DungeonRoomPrefab, currentRoomObject.transform.position + new Vector3(0, 0, 8), Quaternion.identity);
            nextRoomObject = newRoom;
        }
        else
        {
            GameObject newRoom =  InstantiateRoom(DungeonRoomPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            currentRoomObject = newRoom;
            playerIsAtEnterance = true;
        }
    }

    private GameObject InstantiateRoom(GameObject room, Vector3 vector3, Quaternion quaternion)
    {
        GameObject dungRoom = Instantiate(room, vector3, quaternion);
        dungRoom.transform.parent = gameObject.transform;

        int randomRoom = Random.Range(0, 10);
        RoomType roomType = RoomType.Monster;
        if (roomNumber > 4)
        {
            switch (randomRoom)
            {
                case int n when n <= 4:
                    roomType = RoomType.Monster;
                    break;
                case int n when n <= 6:
                    roomType = RoomType.Shop;
                    break;
                case int n when n == 7:
                    roomType = RoomType.Bonfire;
                    break;
                case int n when n == 8:
                    roomType = RoomType.Chest;
                    break;
                case int n when n == 9:
                    roomType = RoomType.Boss;
                    break;
            }
        }
        else
        {
            roomType = RoomType.Monster;
        }

        GameObject roomTypePrefab = EnemyPrefab;
        switch (roomType)
        {
            case RoomType.Monster:
            case RoomType.Boss: // TODO: Make seperate boss
                roomTypePrefab = EnemyPrefab;
                break;
            case RoomType.Shop:
                roomTypePrefab = ShopStallPrefab;
                break;
            case RoomType.Bonfire:
                roomTypePrefab = BonfirePrefab;
                break;
            case RoomType.Chest:
                roomTypePrefab = ChestPrefab; 
                break;
        }
        GameObject dungRoomType = Instantiate(roomTypePrefab, roomTypePrefab.transform.position, roomTypePrefab.transform.rotation);

        dungRoomType.transform.parent = dungRoom.transform;
        currentRoomType = roomType;
        return dungRoom;
    }

    private void RoomBegin()
    {
        bool roomFinished = false;

        if (currentRoomType == RoomType.Monster || currentRoomType == RoomType.Boss)
        {
            MovePlayerFurtherIn();
        }

        // TODO: Implement room gameplay

        if (roomFinished)
        {
            RoomOver();
        }
    }

    private void RoomOver()
    {
        UpdateScoreAndSpeed();
        CreateRoom();
        MovePlayerToNextRoom();
        DeleteOldRoom();
        RoomBegin();
    }

    private void DeleteOldRoom(){
        Destroy(currentRoomObject);
        currentRoomObject = nextRoomObject;
        nextRoomObject = null;
    }

    #endregion

    private void ReadyEnemy()
    {
        gameObject.transform.GetChild((roomNumber == 0) ? 1 : 2).gameObject.transform.GetChild(2).GetComponent<Animator>().SetBool("isReady", true);
        Enemy.Instance.isReady = true;
    }
}
