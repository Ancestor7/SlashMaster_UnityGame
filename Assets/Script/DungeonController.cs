using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DungeonController;
using static UnityEditor.Progress;

public class DungeonController : MonoBehaviour
{
    public static DungeonController Instance { get; private set; }

    #region Enums
    /*
    enum Item
    {
        None,
        Potion,
        AtkBoost,
        DefBoost,
        Protection,
        Run,
        TimeSlow,
        Calm,
        RawDmg,
        InstaKill,
        Revive,
    }
    */
    public enum RoomType
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
    [SerializeField] public GameObject PlayerHud;
    [SerializeField] private GameObject PauseDialog;
    [SerializeField] private GameObject UseRoomDialog;
    [SerializeField] private GameObject GameOverDialog;

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
    [SerializeField] private int playerMaxHealth;
    [SerializeField] private int playerMinHealth;
    [SerializeField] public int playerMoney;
    [SerializeField] public float roomSpeed;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color emptyHealthColor = Color.red;

    private Color midColor = new(1,1,0,1);
    private int playerCurrentHealth;
    private Slider playerHealthSlider;
    private TextMeshProUGUI playerHealthText;
    private Image playerHealthFill;
    private Image playerHealthBg;
    private int playerShieldHealth;
    private Slider playerShieldSlider;
    private TextMeshProUGUI playerShieldText;
    public Item[] playerItems = new Item[3];
    public int playerDmg;
    public int defence = 0;
    public int dmgIncrease = 2, defIncrease = 2, shieldAmount = 5;
    private bool immune = false, shieldActive = false, screenIsDark = true;

    private int playerScore;
    public int roomNumber;
    public RoomType currentRoomType;
    private bool playerIsAtEnterance, playerIsMoving = false, inCombat = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DebugCamera.SetActive(false);
        DarkScreen.SetActive(true);
        PauseDialog.SetActive(false);
        AttachButtons();
    }

    private void AttachButtons()
    {
        PlayerHud.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(PauseGame);
        
        PauseDialog.transform.GetChild(0).transform.GetChild(5).GetComponent<Button>().onClick.AddListener(BackToMenu);
        PauseDialog.transform.GetChild(0).transform.GetChild(6).GetComponent<Button>().onClick.AddListener(ResumeGame);

        GameOverDialog.transform.GetChild(0).transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene(2); });
        GameOverDialog.transform.GetChild(0).transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene(1); });

        GameObject itemBar = PlayerHud.transform.GetChild(4).gameObject;
        itemBar.transform.GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(delegate { UseItem(0); });
        itemBar.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(delegate { UseItem(1); });
        itemBar.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(delegate { UseItem(2); });

        PauseDialog.transform.GetChild(0).transform.GetChild(2).transform.GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMusicVolume(); });
    }

    void Start()
    {
        UpdateMusicVolume();

        roomSpeed = roomSpeed != 0 ? roomSpeed : 5f;   
        
        EnterPlayerToDungeon();
        InitializePlayerData();

        darkScreenImage = DarkScreen.GetComponent<Image>();
        darkScreenImage.color = new Color(darkScreenImage.color.r, darkScreenImage.color.g, darkScreenImage.color.b, 1f);

        LightenScreen();
        CreateRoom();
        RoomBegin();
    }

    #region PlayerMovement

    private void EnterPlayerToDungeon()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = new Vector3(0, 1.5f, -2f);
            Player.Instance.mainCamera.fieldOfView = 100f;
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
        while (playerIsMoving || screenIsDark)
        {
            yield return null;
        }
        playerIsMoving = true;
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
                moveDistance = -3f;
                playerIsAtEnterance = false;
            }
        }
        else
        {
            moveDistance = -5f;
            playerIsAtEnterance = true;
        }

        float destinationPos = transform.position.z + moveDistance;
        while (transform.position.z > destinationPos)
        {
            transform.position -= roomSpeed * Time.deltaTime * transform.forward;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, destinationPos);
        playerIsMoving = false;

        if (isMovingToNextRoom)
        {
            DeleteOldRoom();
        }
    }

    #endregion PlayerMovement

    #region PlayerFunctions

    private void InitializePlayerData()
    {
        playerMaxHealth = playerMaxHealth != 0 ? playerMaxHealth : 10;
        playerMinHealth = 0;
        playerCurrentHealth = playerMaxHealth;
        playerMoney = 0;
        roomNumber = 0;
        playerScore = 0;
        playerDmg = playerDmg != 0 ? playerDmg : 1;
        playerItems = new Item[] { new Item(0), new Item(0), new Item(0) };
        UpdateItemDisplay();

        playerHealthSlider = PlayerHud.transform.GetChild(3).gameObject.GetComponent<Slider>();
        playerHealthSlider.maxValue = playerMaxHealth; 
        playerHealthSlider.minValue = 0;
        playerHealthSlider.value = playerMaxHealth;

        playerHealthText = playerHealthSlider.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        playerHealthFill = playerHealthSlider.transform.GetChild(1).gameObject.transform.GetChild(0).GetComponent<Image>();
        playerHealthBg = playerHealthSlider.transform.GetChild(0).gameObject.GetComponent<Image>();

        playerHealthText.text = playerCurrentHealth.ToString();
        playerHealthFill.color = Color.green;
        playerHealthBg.color = new Color(Color.green.r/2, Color.green.g/2, Color.green.b/2, 1f);

        PlayerHud.transform.GetChild(6).gameObject.SetActive(false);
        playerShieldSlider = PlayerHud.transform.GetChild(6).gameObject.GetComponent<Slider>();
        playerShieldText = playerShieldSlider.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        playerShieldSlider.maxValue = 5;
        playerShieldHealth = (int)playerShieldSlider.maxValue;
    }

    private void UpdateScoreAndSpeed(bool skip, int roomIncrement = 1)
    {
        if (!skip)
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
        }

        PlayerHud.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Room " + roomNumber.ToString();
        PlayerHud.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "Score: " + playerScore.ToString();

        // TODO: Increase room speed by room number
        roomSpeed *= 1.01f;
    }

    public void UpdatePlayerHealth(int healthChange)
    {
        if (immune)
        {
            healthChange = 0;
        }

        if (healthChange != 0)
        {   
            if (healthChange > 0)
            {
                playerCurrentHealth = 
                    (playerCurrentHealth + healthChange) > playerMaxHealth 
                    ? playerMaxHealth 
                    : playerCurrentHealth + healthChange;

            }
            else
            {
                if (shieldActive)
                {
                    playerShieldHealth += healthChange;
                    playerShieldSlider.value = playerShieldHealth;
                    playerShieldText.text = playerShieldSlider.value.ToString();
                    if (playerShieldHealth <= 0)
                    {
                        LoseShield();
                    }
                }
                else
                {
                    healthChange += defence;
                    if (healthChange > 0)
                    {
                        healthChange = 0;
                    }
                    playerCurrentHealth =
                        (playerCurrentHealth + healthChange) < playerMinHealth
                        ? playerMinHealth
                        : playerCurrentHealth + healthChange;
                }
            }

            playerHealthText.text = playerCurrentHealth.ToString();
            if (playerCurrentHealth >= playerMaxHealth / 2f)
            {
                playerHealthFill.color = Color.Lerp(midColor, fullHealthColor, (playerCurrentHealth - playerMaxHealth / 2f) / (playerMaxHealth / 2f));
            }
            else
            {
                playerHealthFill.color = Color.Lerp(emptyHealthColor, midColor, playerCurrentHealth / (playerMaxHealth / 2f));
            }
            playerHealthBg.color = new Color(playerHealthFill.color.r / 2, playerHealthFill.color.g / 2, playerHealthFill.color.b / 2, 1f);

            playerHealthSlider.value = playerCurrentHealth;
        }

        StartCoroutine(PulsePlayerHealth());

        // death check
        if (playerCurrentHealth == 0)
        {
            bool revived = false;
            for (int i = 0; i < 3; i++)
            {
                if (playerItems[i].id == 10)
                {
                    UseItem(i);
                    revived = true;
                    break;
                }
            }
            if (revived)
            {
                UpdatePlayerHealth(playerMaxHealth);
            }
            else
            {
                Player.Instance.OnFightEnd();
                GameOver();
            }
        }
    }

    private IEnumerator PulsePlayerHealth()
    {
        Vector3 minScale = new Vector3(1f, 1f, 1);
        Vector3 maxScale = new Vector3(1.05f, 1.2f, 1);
        bool expanding = true;
        float duration = 0.1f, elapsed = 0f;

        while (true)
        {
            if (expanding)
            {
                playerHealthFill.transform.localScale = Vector3.Lerp(minScale, maxScale, elapsed / duration);
                if (elapsed >= duration)
                {
                    playerHealthFill.transform.localScale = maxScale;
                    elapsed = 0f;
                    expanding = false;
                }
            }
            else
            {
                playerHealthFill.transform.localScale = Vector3.Lerp(maxScale, minScale, elapsed / duration);
                if (elapsed >= duration)
                {
                    playerHealthFill.transform.localScale = minScale;
                    break;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void CoinChange(int coinChange)
    {
        playerMoney += coinChange;
        PlayerHud.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"Coins: {playerMoney}";
    }

    public void UpdateItemDisplay()
    {
        GameObject itemBar = PlayerHud.transform.GetChild(4).gameObject;
        
        for (int i = 0; i < playerItems.Length; i++)
        {
            Image itemImage = itemBar.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();

            if (playerItems[i].id == 0)
            {
                itemImage.enabled = false;
            }
            else
            {
                itemImage.enabled = true;
                itemImage.sprite = playerItems[i].sprite;
            }
        }
    }

    private void UseItem(int index)
    {
        if (!playerIsMoving && inCombat)
        {
            playerItems[index].active = true;
            StartCoroutine(CoItemCooldown(index));
        }
    }

    private IEnumerator CoItemCooldown(int index)
    {
        Image itemCooldown = PlayerHud.transform.GetChild(4).transform.GetChild(index).transform.GetChild(1).transform.GetComponent<Image>();
        float itemTimer = 0.2f;
        switch (playerItems[index].id)
        {
            case 1:
                UpdatePlayerHealth(5);
                break;
            case 2:
                StartCoroutine(CoItemEffect(playerItems[index].id));
                itemTimer = 10f;
                break;
            case 3:
                StartCoroutine(CoItemEffect(playerItems[index].id));
                itemTimer = 10f;
                break;
            case 4:
                GainShield();
                break;
            case 5:
                RunAway();
                break;
            case 6:
                StartCoroutine(CoItemEffect(playerItems[index].id));
                itemTimer = 10f;
                break;
            case 7:
                Enemy.Instance.Calm(10f);
                itemTimer = 10f;
                break;
            case 8:
                Enemy.Instance.PlayerDamage(5);
                break;
            case 9:
                Enemy.Instance.PlayerDamage(Enemy.Instance.health);
                break;
            case 10:
                UpdatePlayerHealth(playerMaxHealth);
                itemTimer = 0.2f;
                break;
            case 11:
                StartCoroutine(CoItemEffect(playerItems[index].id));
                itemTimer = 10f;
                break;
            default:
                itemTimer = 0f;
                break;
        }
        float elapsed = 0;
        while (elapsed < itemTimer)
        {
            itemCooldown.transform.localScale = new Vector3(
                Mathf.Lerp(1f, 0f, elapsed / itemTimer),
                itemCooldown.transform.localScale.y,
                itemCooldown.transform.localScale.z);

            elapsed += Time.deltaTime;
            yield return new WaitUntil(() => inCombat);
        }
        itemCooldown.transform.localScale = new Vector3(0f, itemCooldown.transform.localScale.y, itemCooldown.transform.localScale.z);
        playerItems[index] = new Item(0);
        UpdateItemDisplay();
        yield return null;
    }

    private IEnumerator CoItemEffect(int index)
    {
        if (index == 2)
        {
            playerDmg += dmgIncrease;
        } 
        else if (index == 3)
        {
            defence += defIncrease;
        }
        else if (index == 6)
        {
            Time.timeScale = 0.5f;
        }
        else if (index == 11)
        {
            immune = true;
        }

        float elapsed = 0;
        while (elapsed < 10f)
        {
            if (index == 6)
            {
                elapsed += Time.fixedDeltaTime;
            }
            else
            {
                elapsed += Time.deltaTime;
            }
            
            yield return new WaitUntil(() => inCombat);
        }

        if (index == 2)
        {
            playerDmg -= dmgIncrease;
        }
        else if (index == 3)
        {
            defence -= defIncrease;
        }
        else if (index == 6)
        {
            Time.timeScale = 1f;
        }
        else if (index == 11)
        {
            immune = false;
        }
    }
    
    public int EmptyPlayerSlotIndex()
    {
        for (int i = 0; i < playerItems.Length; i++)
        {
            if (playerItems[i].id == 0)
            {
                return i;
            }
        }

        return -1;
    }
    private void GainShield()
    {
        shieldActive = true;
        GameObject moneyDisplay = PlayerHud.transform.GetChild(2).gameObject;
        GameObject shieldSlider = PlayerHud.transform.GetChild(6).gameObject;

        if (shieldSlider.activeSelf)
        {
            shieldSlider.GetComponent<Slider>().value = shieldSlider.GetComponent<Slider>().maxValue;
        }
        else
        {
            shieldSlider.SetActive(true);
            moneyDisplay.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0.33f);
            moneyDisplay.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.38f);
            shieldSlider.GetComponent<Slider>().maxValue = shieldAmount;
            shieldSlider.GetComponent<Slider>().value = shieldSlider.GetComponent<Slider>().maxValue;
        }
    }

    private void LoseShield()
    {
        shieldActive = false;
        GameObject moneyDisplay = PlayerHud.transform.GetChild(2).gameObject;
        GameObject shieldSlider = PlayerHud.transform.GetChild(6).gameObject;

        shieldSlider.SetActive(false);
        moneyDisplay.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0.265f);
        moneyDisplay.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.315f);
    }

    private void RunAway()
    {
        StartCoroutine(Enemy.Instance.Death(true));
    }

    public void DamageEnemy()
    {
        Enemy.Instance.PlayerDamage(playerDmg);
    }
    
    #endregion

    #region ScreenEffects

    private void DarkenScreen()
    {
        StartCoroutine(CoScreenAlphaChange(Color.black));
    }

    private void LightenScreen()
    {
        StartCoroutine(CoScreenAlphaChange(Color.clear));
    }

    private IEnumerator CoScreenAlphaChange(Color targetColor)
    {
        float duration = 1f, elapsed = 0f;
        Color initialColor = darkScreenImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreenImage.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreenImage.color = targetColor;
        screenIsDark = false;
        if (targetColor == Color.clear)
        {
            screenIsDark = false;
        }
    }

    private IEnumerator CoDarkenAndLighten()
    {
        float duration = 1f, elapsed = 0f;
        Color initialColor = Color.clear;
        Color targetColor = Color.black;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreenImage.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreenImage.color = targetColor;
        duration = 1f;
        elapsed = 0f;
        initialColor = Color.black;
        targetColor = Color.clear;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreenImage.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreenImage.color = targetColor;
    }

    #endregion

    #region RoomFunctions

    private void CreateRoom(){
        /* Place the start room or next room */
        if (currentRoomObject != null)
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


        int randomRoom = UnityEngine.Random.Range(1, 4);
        RoomType roomType = RoomType.Monster;
        if (roomNumber == 0)
        {
            roomType = RoomType.Monster;
        }
        else if (roomNumber % 20 == 0)
        {
            roomType = RoomType.Boss;
        }
        else if ((roomNumber + 1) % 20 == 0 || roomNumber % 4 == 0)
        {
            switch (randomRoom)
            {
                case 1:
                    roomType = RoomType.Shop;
                    break;
                case 2:
                    roomType = RoomType.Bonfire;
                    break;
                case 3:
                    roomType = RoomType.Chest;
                    break;
            }
        }
        else
        {
            roomType = RoomType.Monster;
        }

        GameObject dungRoomType;
        switch (roomType)
        {
            case RoomType.Boss: // TODO: Make seperate boss
                dungRoomType = Instantiate(EnemyPrefab, EnemyPrefab.transform.position + vector3, EnemyPrefab.transform.rotation);
                break;
            case RoomType.Shop:
                dungRoomType = Instantiate(ShopStallPrefab, ShopStallPrefab.transform.position + vector3, ShopStallPrefab.transform.rotation);
                break;
            case RoomType.Bonfire:
                dungRoomType = Instantiate(BonfirePrefab, BonfirePrefab.transform.position + vector3, BonfirePrefab.transform.rotation);
                break;
            case RoomType.Chest:
                dungRoomType = Instantiate(ChestPrefab, ChestPrefab.transform.position + vector3, ChestPrefab.transform.rotation);
                break;
            default:
                dungRoomType = Instantiate(EnemyPrefab, EnemyPrefab.transform.position + vector3, EnemyPrefab.transform.rotation);
                break;
        }
        

        dungRoomType.transform.parent = dungRoom.transform;
        currentRoomType = roomType;
        return dungRoom;
    }

    private void RoomBegin()
    {
        switch (currentRoomType)
        {
            case RoomType.Shop:
            case RoomType.Chest:
            case RoomType.Bonfire:
                StartCoroutine(OpenEnterRoomDialog());
                break;
            default:
                MovePlayerFurtherIn();
                inCombat = true;
                StartCoroutine(StartCombat());
                break;
        }
    }

    private IEnumerator OpenEnterRoomDialog()
    {
        while (playerIsMoving)
        {
            yield return null;
        }
        GameObject roomDialog = Instantiate(UseRoomDialog);
        roomDialog.transform.SetParent(transform.GetChild(0).gameObject.transform, false);
        yield return null;

    }

    public void RoomOver(bool skip=false)
    {
        inCombat= false;
        UpdateScoreAndSpeed(skip);
        CreateRoom();
        MovePlayerToNextRoom();
        RoomBegin();
    }

    private void DeleteOldRoom(){
        Destroy(currentRoomObject);
        currentRoomObject = nextRoomObject;
        nextRoomObject = null;
    }

    private IEnumerator StartCombat()
    {
        while (playerIsMoving || screenIsDark)
        {
            yield return null;
        }
        //yield return new WaitForSeconds(3f / roomSpeed);
        if (Enemy.Instance == null)
        {
            Debug.LogError("Enemy Instance null");
        }
        else
        {
            Enemy.Instance.Ready();
        }
        Player.Instance.OnFightStart();
        yield return null;
    }

    public void EndEnemyRoom(int coinReward, bool skip = false)
    {
        if (!skip) { CoinChange(coinReward); }
        RoomOver(skip);
    }

    public void Rest()
    {
        StartCoroutine(CoRest());
    }

    public IEnumerator CoRest()
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.fixedDeltaTime;
            darkScreenImage.color = Color.Lerp(Color.clear, Color.black, elapsed / 1f);
            yield return null;
        }
        darkScreenImage.color = Color.black;

        yield return new WaitForSeconds(0.5f);
        UpdatePlayerHealth(playerMaxHealth / 2);

        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.fixedDeltaTime;
            darkScreenImage.color = Color.Lerp(Color.black, Color.clear, elapsed / 1f);
            yield return null;
        }
        darkScreenImage.color = Color.clear;

        Destroy(DialogManager.Instance);

        RoomOver();
    }

    public void OpenChest()
    {
        StartCoroutine(OpenChestCoroutine());
    }

    private IEnumerator OpenChestCoroutine()
    {
        GameObject chest = transform.GetChild(1).transform.GetChild(2).transform.GetChild(0).gameObject;
        Animator chestAnimator = chest.GetComponent<Animator>();
        chestAnimator.SetTrigger("opened");
        yield return new WaitForSeconds(1);

        int random, index = EmptyPlayerSlotIndex();
        if (index != -1)
        {
            random = UnityEngine.Random.Range(1, 4);
            chest.transform.GetChild(random).transform.gameObject.SetActive(true);
            if (random == 1)
            {
                int randomCoin = UnityEngine.Random.Range(5, (int)(roomNumber * 1.25f) + 1);
                CoinChange(randomCoin);
            }
            if (random == 2)
            {
                int randomItem = UnityEngine.Random.Range(1, 8);
                playerItems[index] = new Item(randomItem);
                UpdateItemDisplay();
            }
            if (random == 3)
            {
                // sad
            }
        }
        else
        {
            random = UnityEngine.Random.Range(1, 3);
            if (random == 1)
            {
                chest.transform.GetChild(random).transform.gameObject.SetActive(true);
                int randomCoin = UnityEngine.Random.Range(5, (int)(roomNumber * 1.25f) + 1);
                CoinChange(randomCoin);
            }
            if (random == 2)
            {
                chest.transform.GetChild(random + 1).transform.gameObject.SetActive(true);
                // sad
            }
        }
        yield return new WaitForSeconds(1);
        RoomOver();
    }

    #endregion

    #region ButtonFunctinos

    private void PauseGame()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void BackToMenu()
    {
        PauseDialog.SetActive(false);
        PlayerHud.SetActive(false);
        ResumeGame();
        DarkenScreen();
        Player.Instance.OnFightEnd();
        StartCoroutine(CoBackToMenu());
    }

    private IEnumerator CoBackToMenu()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
        Player.Instance.ResetPlayer();
    }

    #endregion

    private void GameOver()
    {
        PlayerHud.SetActive(false);
        GameOverDialog.SetActive(true);
    }

    public void UpdateMusicVolume()
    {
        transform.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("musicVolume");
    }
}
