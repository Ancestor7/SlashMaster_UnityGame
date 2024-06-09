using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private GameObject StoreDialog;

    [Header("Objects")]
    [SerializeField] private GameObject Title;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private GameObject EnterButton;

    [Header("Font Sprite Assets")]
    [SerializeField] private TMP_SpriteAsset storeFont;
    [SerializeField] private TMP_SpriteAsset restFont;
    [SerializeField] private TMP_SpriteAsset chestFont;

    private GameObject roomDialog;

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

        
        Button enterButton = EnterButton.GetComponent<Button>();
        TextMeshProUGUI buttonText = EnterButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        if (DungeonController.Instance.currentRoomType == DungeonController.RoomType.Shop)
        {
            roomDialog = Instantiate(StoreDialog);
            roomDialog.SetActive(false);
            roomDialog.transform.SetParent(DungeonController.Instance.gameObject.transform.GetChild(0), false);
            Title.GetComponent<TextMeshProUGUI>().text = "Enter Store?";
            buttonText.text = "<sprite=0>\nEnter";
            buttonText.spriteAsset = storeFont;
            enterButton.onClick.AddListener(delegate { roomDialog.SetActive(true); } );
        }
        if (DungeonController.Instance.currentRoomType == DungeonController.RoomType.Bonfire)
        {
            Title.GetComponent<TextMeshProUGUI>().text = "Rest?";
            buttonText.text = "<sprite=0>\nRest";
            buttonText.spriteAsset = restFont;
            enterButton.onClick.AddListener(delegate { DungeonController.Instance.Rest(); } );
        }
        if (DungeonController.Instance.currentRoomType == DungeonController.RoomType.Chest)
        {
            Title.GetComponent<TextMeshProUGUI>().text = "Open Chest?";
            buttonText.text = "<sprite=0>\nOpen";
            buttonText.spriteAsset = chestFont;
            enterButton.onClick.AddListener(delegate { DungeonController.Instance.OpenChest(); DungeonController.Instance.RoomOver(); Destroy(gameObject); } );
        }

        enterButton.onClick.AddListener(delegate { UseRoomDialogDisabled(); });
        ContinueButton.GetComponent<Button>().onClick.AddListener(delegate { if (roomDialog != null) { Destroy(roomDialog); } DungeonController.Instance.RoomOver(); Destroy(gameObject); });
    }

    public void UseRoomDialogEnabled()
    {
        gameObject.SetActive(true);
    }

    public void UseRoomDialogDisabled()
    {
        gameObject.SetActive(false);
    }

}
