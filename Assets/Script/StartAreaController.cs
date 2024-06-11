using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartAreaController : MonoBehaviour
{
    public static StartAreaController Instance { get; private set; }

    [Header("Menu Pages")]
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject Info;
    [SerializeField] GameObject Ranks;
    [SerializeField] GameObject Store;
    [SerializeField] GameObject Settings;
    [SerializeField] GameObject DarkScreen;

    [Header("PlayButtons")]
    [SerializeField] Button PlayButton;

    private Image darkScreen;

    private void OnEnable()
    {
        PlacePlayer();
    }

    void Start()
    {
        UpdateMusicVolume();
        SetMainMenu();
        PlacePlayer();
        darkScreen = DarkScreen.GetComponent<Image>();
        darkScreen.color = new Color(darkScreen.color.r, darkScreen.color.g, darkScreen.color.b, 1f);
        PlayButton.onClick.AddListener(OnPlayButtonPress);
        PlayButton.onClick.AddListener(DarkenScreen);
        Settings.transform.GetChild(1).transform.GetChild(2).transform.GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMusicVolume(); });
        LightenScreen();
    }

    public void SetMainMenu()
    {
        MainMenu.SetActive(true);
        Info.SetActive(false);
        Ranks.SetActive(false);
        Store.SetActive(false);
        Settings.SetActive(false);
    }

    public void PlacePlayer()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = new Vector3(0, 1, 0);
            Player.Instance.mainCamera.fieldOfView = 60f;
        }
    }
    private void OnPlayButtonPress()
    {
        StartCoroutine(CoMoveCharacterAtStart());
    }

    private IEnumerator CoMoveCharacterAtStart()
    {
        while (transform.localPosition.z > -10f)
        {
            transform.localPosition -= 3f * Time.deltaTime * transform.forward;
            yield return null;
        }
        transform.localPosition = new Vector3(transform.position.x, transform.position.y, -10f);
        SceneManager.LoadScene(2);
    }

    public void DarkenScreen()
    {
        StartCoroutine(CoDarkenScreen());
    }

    private IEnumerator CoDarkenScreen()
    {
        DarkScreen.SetActive(true);
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

    private void LightenScreen()
    {
        StartCoroutine(CoLightenScreen());
    }

    private IEnumerator CoLightenScreen()
    {
        yield return new WaitForSeconds(0.5f);
        float duration = 0.5f, elapsed = 0f;
        Color initialColor = darkScreen.color, targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkScreen.color = Color.Lerp(initialColor, targetColor, elapsed / duration);
            yield return null;
        }
        darkScreen.color = targetColor;
        DarkScreen.SetActive(false);
    }

    public void UpdateMusicVolume()
    {
        transform.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("musicVolume");
    }
}
