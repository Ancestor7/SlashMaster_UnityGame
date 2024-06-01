using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonController : MonoBehaviour
{
    [SerializeField] private Button moveButton;

    void Start()
    {
        if (moveButton != null)
        {
            moveButton.onClick.AddListener(OnMoveButtonClick);
        }
    }

    private void OnMoveButtonClick()
    {
        if (Player.Instance != null)
        {
            //Player.Instance.MoveCharacter();
        }
    }
}
