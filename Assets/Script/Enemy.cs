using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance { get; private set; }

    [SerializeField] private GameObject healthBarObject;
    private TextMeshProUGUI healthBarText;
    private Slider healthBarSlider;
    [SerializeField] private GameObject attackBarObject;
    private TextMeshProUGUI attackBarText;
    private Image attackBarBg, attackBarFill;
    private Slider attackBarSlider;
    [SerializeField] private Color startAttackColor, endAttackColor;

    [Header("EnemyStats")]
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private float attackTime;

    private Animator animator;
    public bool isReady = false, isDead = false, isAttacking = false;

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
        animator = GetComponent<Animator>();

        damage = damage != 0 ? damage : 1;
        attackTime = attackTime != 0f ? attackTime : 5f;
        health = health != 0 ? health : 10;
        
        attackBarSlider = attackBarObject.GetComponent<Slider>();
        attackBarSlider.maxValue = attackTime;
        attackBarSlider.minValue = 0;
        attackBarSlider.value = 0;

        attackBarText = attackBarObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        attackBarText.text = "0";

        startAttackColor = startAttackColor != Color.clear ? startAttackColor : new Color(0.5f, 1, 0, 1);
        endAttackColor = endAttackColor != Color.clear ? endAttackColor : new Color(1, 0.5f, 0, 1);
        attackBarFill = attackBarObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        attackBarFill.color = new Color(startAttackColor.r, startAttackColor.g, startAttackColor.b);
        attackBarBg = attackBarObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        attackBarBg.color = new Color(startAttackColor.r / 2, startAttackColor.g / 2, startAttackColor.b / 2);

        healthBarSlider = healthBarObject.GetComponent<Slider>();
        healthBarSlider.maxValue = health;
        healthBarSlider.value = health;

        healthBarText = healthBarObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        healthBarText.text = health.ToString();
    }

    private void UpdateAttackBar()
    {
        attackBarText.text = (Math.Truncate(10 * attackBarSlider.value)/10).ToString();
        attackBarFill.color = Color.Lerp(startAttackColor, endAttackColor, attackBarSlider.value / attackBarSlider.maxValue);
        attackBarBg.color = new Color(attackBarFill.color.r / 2, attackBarFill.color.g / 2, attackBarFill.color.b / 2, 1f);
        if (isReady)
        {
            if (!isAttacking && attackBarSlider.value >= attackBarSlider.maxValue)
            {
                attackBarSlider.value = attackTime;
                isAttacking = true;
                StartCoroutine(AttackPlayer());
            }
            else if (!isAttacking)
            {
                attackBarSlider.value += Time.deltaTime;
            }
        }
    }
    
    private void UpdateHealthBar()
    {
        healthBarText.text = health.ToString();
        healthBarSlider.value = health;
    }

    private void Ready()
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
            yield return null;
            calmDuration -= Time.deltaTime;
        }
        Ready();
    }

    private IEnumerator AttackPlayer()
    {

        int randomAtkIndex = UnityEngine.Random.Range(0, 3);
        animator.SetInteger("AttackIndex", randomAtkIndex);
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(randomAtkIndex != 0 ? 1.5f / 2 : 1f / 2);

        // TODO: deal damage to player

        yield return new WaitForSeconds(randomAtkIndex != 0 ? 1.5f/2 : 1f/2);
        while (attackBarSlider.value > 0)
        {
            yield return null;
            attackBarSlider.value -= Time.deltaTime * attackTime * 2;
        }
        attackBarSlider.value = 0f;

        isAttacking=false;
    }

    private void PlayerDamage(int damageDealt)
    {
        health -= damageDealt;
        UpdateHealthBar();
        if (health <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        isDead = true;
        animator.SetBool("isDead", true);
    }
}
