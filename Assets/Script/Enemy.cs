using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance { get; private set; }

    [SerializeField] private GameObject HealthBarObject;
    private TextMeshProUGUI healthBarText;
    private Slider healthBarSlider;

    [SerializeField] private GameObject AttackBarObject;
    private TextMeshProUGUI attackBarText;
    private Image attackBarBg, attackBarFill;
    private Slider attackBarSlider;

    private Color startAttackColor, endAttackColor;

    [SerializeField] private GameObject BossNameObject;
    [SerializeField] private GameObject KickAudio;
    [SerializeField] private GameObject SlashAudio;

    public int health;
    private int damage;
    private int reward;
    private float attackTime;

    private Animator animator;
    public bool isReady = false, isDead = false, isAttacking = false, isBoss = false;
    Coroutine attackPlayerCoroutine;

    void Awake()
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
        animator = gameObject.GetComponent<Animator>();
    }

    void Start()
    {
        InitializeEnemy();
    }

    private void Update()
    {
        UpdateAttackBar();
    }

    private void InitializeEnemy()
    {
        int roomNumber = DungeonController.Instance.roomNumber;
        float healthMult = 1 + roomNumber / 20f;
        float damageMult = 1 + roomNumber / 50f;
        float rewardMult = 1 + roomNumber / 20f;
        float attackTimeMult = Mathf.Lerp(1f, 0.5f, roomNumber/50f);
        float bossMult = 1f;
        if (roomNumber != 0 && roomNumber % 20 == 0)
        {
            isBoss = true;
            float bossRandomMult = 1 + roomNumber / 200f;
            bossMult = UnityEngine.Random.Range(1 * bossRandomMult, 3 * bossRandomMult);
        }
        health = Mathf.RoundToInt(bossMult * UnityEngine.Random.Range(1 * healthMult, 2 * healthMult));
        damage = Mathf.RoundToInt(bossMult * UnityEngine.Random.Range(-2 * damageMult, -1 * damageMult));
        reward = Mathf.RoundToInt(bossMult * UnityEngine.Random.Range(1 * rewardMult, 2 * rewardMult));
        attackTime = bossMult * UnityEngine.Random.Range(6 * attackTimeMult, 8 * attackTimeMult);
        attackTime = (attackTime < 3f) ? 3f : attackTime;

        /*
        health = 5;
        damage = -2;
        reward = 1;
        attackTime = 3f;
        */

        if (isBoss)
        {
            HealthBarObject.GetComponent<RectTransform>().anchorMax = new Vector3(1, 0.94f);
            HealthBarObject.GetComponent<RectTransform>().anchorMin = new Vector3(0, 0.88f);

            AttackBarObject.GetComponent<RectTransform>().anchorMax = new Vector3(1, 0.88f);
            AttackBarObject.GetComponent<RectTransform>().anchorMin = new Vector3(0, 0.82f);

            BossNameObject.SetActive(true);
        }

        attackBarSlider = AttackBarObject.GetComponent<Slider>();
        attackBarSlider.maxValue = attackTime;
        attackBarSlider.minValue = 0;
        attackBarSlider.value = 0;

        attackBarText = AttackBarObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        attackBarText.text = "0";

        startAttackColor = startAttackColor != Color.clear ? startAttackColor : Color.white;
        endAttackColor = endAttackColor != Color.clear ? endAttackColor : new Color(1, 0.5f, 0, 1);
        attackBarFill = AttackBarObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        attackBarFill.color = startAttackColor;
        attackBarBg = AttackBarObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        attackBarBg.color = new Color(startAttackColor.r / 2, startAttackColor.g / 2, startAttackColor.b / 2);

        healthBarSlider = HealthBarObject.GetComponent<Slider>();
        healthBarSlider.maxValue = health;
        healthBarSlider.value = health;

        healthBarText = HealthBarObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        healthBarText.text = health.ToString();
    }

    private void UpdateAttackBar()
    {
        if (isReady && !isDead)
        {
            attackBarText.text = (Math.Truncate(10 * attackBarSlider.value) / 10).ToString();
            attackBarFill.color = Color.Lerp(startAttackColor, endAttackColor, attackBarSlider.value / attackBarSlider.maxValue);
            attackBarBg.color = new Color(attackBarFill.color.r / 2, attackBarFill.color.g / 2, attackBarFill.color.b / 2, 1f);
            if (!isAttacking && attackBarSlider.value >= attackBarSlider.maxValue)
            {
                attackBarSlider.value = attackTime;
                isAttacking = true;
                attackPlayerCoroutine = StartCoroutine(AttackPlayer());
            }
            else if (!isAttacking)
            {
                attackBarSlider.value += Time.deltaTime;
            }
        }
    }
    
    private void UpdateHealthBar()
    {
        if (health < 0)
        {
            health = 0;
        }
        healthBarText.text = health.ToString();
        healthBarSlider.value = health;
    }

    public void Ready()
    {
        animator.SetBool("isReady",true);
        isReady = true;
    }

    public void Calm(float calmDuration)
    {
        animator.SetBool("isReady", false);
        isReady = false;
        attackBarSlider.value = !isAttacking && attackBarSlider.value >= 2 ? attackBarSlider.value - 1.5f : 0;
        isAttacking = false;
        StartCoroutine(CoCalm(calmDuration));
    }

    private IEnumerator CoCalm(float calmDuration)
    {
        while (calmDuration>0)
        {
            calmDuration -= Time.deltaTime;
            yield return null;
        }
        Ready();
    }

    private IEnumerator AttackPlayer()
    {
        int randomAtkIndex = UnityEngine.Random.Range(0, 3);
        animator.SetInteger("AttackIndex", randomAtkIndex);
        animator.SetTrigger("Attack");
        AudioSource audioData;
        if (randomAtkIndex == 0)
        {
            audioData = KickAudio.GetComponent<AudioSource>();
        }
        else
        {
            audioData = SlashAudio.GetComponent<AudioSource>();
        }

        audioData.loop = false;
        audioData.volume = PlayerPrefs.GetFloat("soundVolume");
        audioData.Play();
        yield return new WaitForSeconds(randomAtkIndex != 0 ? 1.5f / 3f : 1f / 3f);

        DungeonController.Instance.UpdatePlayerHealth(damage);

        yield return new WaitForSeconds(randomAtkIndex != 0 ? 1.5f * 2f / 3f: 1f * 2f / 3f);
        while (attackBarSlider.value > 0)
        {
            attackBarSlider.value -= Time.deltaTime * attackTime * 2;
            yield return null;
        }
        attackBarSlider.value = 0f;

        isAttacking=false;
    }

    public void PlayerDamage(int damageDealt)
    {
        health -= damageDealt;
        UpdateHealthBar();
        if (health <= 0)
        {
            StartCoroutine(Death());
        }
    }

    public IEnumerator Death(bool skip = false)
    {
        isDead = true;
        if (attackPlayerCoroutine != null)
        {
            StopCoroutine(attackPlayerCoroutine);
        }
        Player.Instance.OnFightEnd();
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(1 / DungeonController.Instance.roomSpeed);
        Instance = null;
        DungeonController.Instance.EndEnemyRoom(reward, skip);
        //yield return new WaitForSeconds(1 / DungeonController.Instance.roomSpeed);
        
        
    }
}
