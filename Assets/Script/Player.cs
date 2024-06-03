using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public GameObject parent;
    public Camera mainCamera;
    public Light playerLight;

    private List<Vector2> touchPoints = new List<Vector2>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        mainCamera = parent.transform.GetChild(0).gameObject.GetComponent<Camera>();
        playerLight = parent.transform.GetChild(1).gameObject.GetComponent<Light>();
    }

    void Update()
    {
        
    }

}