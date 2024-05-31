using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private bool gameStarting { get; set; } 
    private string currentLevel { get; set; }
    private Camera mainCamera;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameStarting = false;
        currentLevel = null;
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            DontDestroyOnLoad(mainCamera.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarting && transform.position.z < 0)
        {
            transform.position += transform.forward * 3 * Time.deltaTime;
        }
        else if (gameStarting && transform.position.z >= 0)
        {
            SceneManager.LoadScene(1);
            transform.position = new Vector3(0, 1.75f, 4.5f);
            transform.Rotate(0, 180f, 0, Space.Self);
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = 60f;
            }
            gameStarting = false;
        }



    }

    public void StartGame(string currentLevel)
    {
        gameStarting = true;
        this.currentLevel = currentLevel;
    }
}
