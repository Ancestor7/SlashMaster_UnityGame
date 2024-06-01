using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public GameObject parent;

    [SerializeField] float speed;

    private Camera mainCamera;
    private Light playerLight;

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
        speed = 2.5f;
    }

    void Update()
    {

    }

    public void EnterDungeon()
    {
        transform.position = new Vector3(0, 1.5f, -2f);
        mainCamera.fieldOfView += 40f;
    }

    public void MoveCharacterAtStart()
    {
        StartCoroutine(CoMoveCharacterAtStart());
    }

    private IEnumerator CoMoveCharacterAtStart()
    {
        while (transform.position.z < 10f)
        {
            transform.position += speed * Time.deltaTime * transform.forward;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, 10f);
        //yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }

    public void EnterDungeonRoom()
    {
        StartCoroutine(CoEnterDungeonRoom());
    }

    private IEnumerator CoEnterDungeonRoom()
    {
        yield return null;
    }

}