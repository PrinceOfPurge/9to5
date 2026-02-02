using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomWeapon : WeaponBase
{
    public float attackCooldown = 0.8f;
    public float attackRange = 2f;
    public int damage = 10;
    public LayerMask hitMask;

    private bool canAttack = true;

    public override void OnAttack()
    {
        Debug.Log("Broom Used");
        if (!canAttack) return;
        canAttack = false;

        // Trigger broom swing animation
        if (animator) animator.SetTrigger("Swing");

        // Example: Raycast forward for melee hit
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange, hitMask))
        {
            Debug.Log($"Hit {hit.collider.name} for {damage} damage!");
            // You can add score logic here if you want (or leave to ScoreManager)
        }

        // Reset cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

}