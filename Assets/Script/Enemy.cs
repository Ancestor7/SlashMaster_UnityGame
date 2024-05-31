using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        /*yield return new WaitForSeconds(Random.Range(3, 10));
        animator.SetTrigger("Ready");

        while (true)
        {
            yield return new WaitForSeconds(3);
            animator.SetInteger("AttackIndex", Random.Range(0, 4));
            animator.SetTrigger("Attack");
        }*/
    }

    private void Update()
    {
        
    }

}
