using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    private Item[] shopItems = new Item[3];
    private GameObject[] shopItemObjects = new GameObject[3], playerItemObjects = new GameObject[3];
    
    private int roomNumber;

    [SerializeField] private GameObject StoreItemPrefab;
    [SerializeField] private int highLootThreshold;
    [SerializeField] private int midLootThreshold;

    private void Awake()
    {
        //DialogManager.Instance.UseRoomDialogDisabled();
        //transform.SetParent(DungeonController.Instance.gameObject.transform.GetChild(0), false);
        Button btn1 = transform.GetChild(0).gameObject.transform.GetChild(2).gameObject.GetComponent<Button>();
        Button btn2 = transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.GetComponent<Button>();
        btn1.onClick.AddListener(delegate { DialogManager.Instance.UseRoomDialogEnabled(); });
        btn2.onClick.AddListener(delegate { DialogManager.Instance.UseRoomDialogEnabled(); });

        roomNumber = DungeonController.Instance.roomNumber;
        highLootThreshold = highLootThreshold != 0 ? highLootThreshold : 25;
        midLootThreshold = midLootThreshold != 0 ? midLootThreshold : 10;
        GetPlayerMoney();
        GenerateItems();
    }

    private void GetPlayerMoney()
    {
        int playerMoney = DungeonController.Instance.playerMoney;
        transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"Coins: {playerMoney}";
        transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"Coins: {playerMoney}";
    }

    private void GenerateItems()
    {
        var itemProbabilities = new List<(int itemId, int probability)>
        {
            (1, 50), (2, 35), (3, 35), (4, 25), (5, 25),
            (6, 20), (7, 20), (8, 20),
            (9, 10), (10, 10), (11, 10),
        };

        int randomUpperLimit = 170;

        if (roomNumber > highLootThreshold)
        {
            randomUpperLimit += 30;
        }
        if (roomNumber > midLootThreshold)
        {
            randomUpperLimit += 60;
        }

        shopItems[0] = GetRandomItem(itemProbabilities, randomUpperLimit);
        shopItems[1] = GetRandomItem(itemProbabilities, randomUpperLimit);
        shopItems[2] = GetRandomItem(itemProbabilities, randomUpperLimit);

        InstantiateItems();
        UpdateItemButtons();
    }

    private Item GetRandomItem(List<(int itemId, int probability)> itemProbabilities, int randomUpperLimit)
    {
        int randomValue = Random.Range(0, randomUpperLimit);
        int cumulativeProbability = 0;

        foreach (var (itemId, probability) in itemProbabilities)
        {
            cumulativeProbability += probability;
            if (randomValue < cumulativeProbability)
            {
                return new Item(itemId);
            }
        }

        return new Item(0); 
    }

    private void InstantiateItems()
    {
        GameObject shopPanel = transform.GetChild(0).gameObject;
        Vector2 startShopVector = new(0.05f, 0.675f); // height: 0.15f, gap: 0.025
        Vector2 endShopVector = new(0.95f, 0.825f);
        Vector2 startSellVector = new(0.05f, 0.675f); // height: 0.15f, gap: 0.025
        Vector2 endSellVector = new(0.95f, 0.825f);
        float vectorChange = 0.175f;
        for (int i = 0; i < shopItems.Length; i++)
        {
            GameObject shopItemObject = Instantiate(StoreItemPrefab);
            shopItemObject.transform.SetParent(shopPanel.transform, false);
            //shopItemObject.transform.parent = shopPanel.transform;
            shopItemObject.GetComponent<RectTransform>().anchorMin = startShopVector;
            shopItemObject.GetComponent<RectTransform>().anchorMax = endShopVector;
            shopItemObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = shopItems[i].sprite;
            shopItemObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
            shopItemObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = shopItems[i].itemName;
            GameObject bottomText = shopItemObject.transform.GetChild(2).gameObject;
            bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = shopItems[i].itemDesc;
            bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"{shopItems[i].itemCost} <sprite=0>";
            shopItemObjects[i] = shopItemObject;

            startShopVector = new Vector2(startShopVector.x, startShopVector.y - vectorChange);
            endShopVector = new Vector2(endShopVector.x, endShopVector.y - vectorChange);
            Button itemButton = shopItemObject.GetComponent<Button>();
            itemButton.onClick.AddListener(delegate { BuyItem(System.Array.IndexOf(shopItemObjects, shopItemObject)); });
        }
        GameObject sellPanel = transform.GetChild(1).gameObject;
        
        if (DungeonController.Instance.playerItems.Length > 0)
        {
            for (int i = 0; i < DungeonController.Instance.playerItems.Length; i++)
            {
                GameObject sellItemObject = Instantiate(StoreItemPrefab);
                sellItemObject.transform.SetParent(sellPanel.transform, false);
                sellItemObject.GetComponent<RectTransform>().anchorMin = startSellVector;
                sellItemObject.GetComponent<RectTransform>().anchorMax = endSellVector;

                if (DungeonController.Instance.playerItems[i].id != 0)
                {
                    sellItemObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = DungeonController.Instance.playerItems[i].sprite;
                    sellItemObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
                    sellItemObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = DungeonController.Instance.playerItems[i].itemName;
                    GameObject bottomText = sellItemObject.transform.GetChild(2).gameObject;
                    bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = DungeonController.Instance.playerItems[i].itemDesc;
                    bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"{DungeonController.Instance.playerItems[i].itemCost} <sprite=0>";
                }
                else
                {
                    sellItemObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = null;
                    sellItemObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "Empty";
                    GameObject bottomText = sellItemObject.transform.GetChild(2).gameObject;
                    bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                    bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                }
                playerItemObjects[i] = sellItemObject;
                startSellVector = new Vector2(startSellVector.x, startSellVector.y - vectorChange);
                endSellVector = new Vector2(endSellVector.x, endSellVector.y - vectorChange);

                Button itemButton = sellItemObject.GetComponent<Button>();
                itemButton.onClick.AddListener(delegate { SellItem(System.Array.IndexOf(playerItemObjects, sellItemObject)); });
            }
        }
    }

    private void UpdateItemButtons()
    {
        GameObject shopPanel = transform.GetChild(0).gameObject;
        Vector2 startShopVector = new(0.05f, 0.675f); // height: 0.15f, gap: 0.025
        Vector2 endShopVector = new(0.95f, 0.825f);
        Vector2 startSellVector = new(0.05f, 0.675f); // height: 0.15f, gap: 0.025
        Vector2 endSellVector = new(0.95f, 0.825f);
        float vectorChange = 0.175f;

        
        for (int i = 0;i < shopItemObjects.Length;i++) 
        {
            shopItemObjects[i].transform.SetParent(shopPanel.transform, false);
            shopItemObjects[i].GetComponent<RectTransform>().anchorMin = startShopVector;
            shopItemObjects[i].GetComponent<RectTransform>().anchorMax = endShopVector;
            shopItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = shopItems[i].sprite;
            shopItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
            shopItemObjects[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = shopItems[i].itemName;
            GameObject bottomText = shopItemObjects[i].transform.GetChild(2).gameObject;
            bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = shopItems[i].itemDesc;
            bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"{shopItems[i].itemCost} <sprite=0>";

            startShopVector = new Vector2(startShopVector.x, startShopVector.y - vectorChange);
            endShopVector = new Vector2(endShopVector.x, endShopVector.y - vectorChange);

            Button itemButton = shopItemObjects[i].GetComponent<Button>();
            itemButton.interactable = true;
            TextMeshProUGUI coinText = shopItemObjects[i].transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            int emptySlotIndex = DungeonController.Instance.EmptyPlayerSlotIndex();
            if (shopItems[i].id != 0)
            {
                if (emptySlotIndex >= 0)
                {
                    itemButton.interactable = true;
                }
                else
                {
                    itemButton.interactable = false;
                    continue;
                }

                if (DungeonController.Instance.playerMoney >= shopItems[i].itemCost)
                {
                    itemButton.interactable = true;
                    coinText.color = Color.black;
                }
                else
                {
                    itemButton.interactable = false;
                    coinText.color = new Color(0.8f,0,0,1f);
                    continue;
                }
            }
            else
            {
                itemButton.interactable = false;
                shopItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.clear;
            }
            

        }

        GameObject sellPanel = transform.GetChild(1).gameObject;
        
        for (int i = 0; i < DungeonController.Instance.playerItems.Length; i++)
        {
            playerItemObjects[i].transform.SetParent(sellPanel.transform, false);
            playerItemObjects[i].GetComponent<RectTransform>().anchorMin = startSellVector;
            playerItemObjects[i].GetComponent<RectTransform>().anchorMax = endSellVector;

            if (DungeonController.Instance.playerItems[i].id != 0)
            {
                playerItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = DungeonController.Instance.playerItems[i].sprite;
                playerItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;

                playerItemObjects[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = DungeonController.Instance.playerItems[i].itemName;
                GameObject bottomText = playerItemObjects[i].transform.GetChild(2).gameObject;
                bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = DungeonController.Instance.playerItems[i].itemDesc;
                bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"{DungeonController.Instance.playerItems[i].itemCost / 2} <sprite=0>";
            }
            else
            {
                playerItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = null;
                playerItemObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.clear;
                playerItemObjects[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject bottomText = playerItemObjects[i].transform.GetChild(2).gameObject;
                bottomText.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                bottomText.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "";
            }

            startSellVector = new Vector2(startSellVector.x, startSellVector.y - vectorChange);
            endSellVector = new Vector2(endSellVector.x, endSellVector.y - vectorChange);

            Button playerItemButton = playerItemObjects[i].GetComponent<Button>();
            playerItemButton.onClick.AddListener(delegate { SellItem(i); });
            playerItemButton.interactable = true;
            if (DungeonController.Instance.playerItems[i].id == 0 || DungeonController.Instance.playerItems[i].active == true)
            {
                playerItemButton.interactable = false;
            }
        }
    }

    public void BuyItem(int index)
    {
        DungeonController.Instance.CoinChange(-shopItems[index].itemCost);
        int playerIndex = DungeonController.Instance.EmptyPlayerSlotIndex();
        DungeonController.Instance.playerItems[playerIndex] = shopItems[index];
        shopItems[index] = new Item(0);
        UpdateItemButtons();
        GetPlayerMoney();
        DungeonController.Instance.UpdateItemDisplay();
    }

    public void SellItem(int index)
    {
        DungeonController.Instance.CoinChange(DungeonController.Instance.playerItems[index].itemCost / 2);
        DungeonController.Instance.playerItems[index] = new Item(0);
        UpdateItemButtons();
        GetPlayerMoney();
        DungeonController.Instance.UpdateItemDisplay();
    }
}


