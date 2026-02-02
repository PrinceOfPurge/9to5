using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCleanerWeapon : WeaponBase
{
    public float fireRate = 0.25f;
    public int damage = 5;
    public float range = 25f;
    public LayerMask hitMask;
    public ParticleSystem sprayEffect;

    private bool canFire = true;

    public override void OnAttack()
    {
        Debug.Log("All-Purpose Cleaner Used");
        if (!canFire) return;
        canFire = false;

        // Play spray animation or particle
        if (sprayEffect) sprayEffect.Play();
        if (animator) animator.SetTrigger("Spray");

        // Raycast forward to simulate shooting
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, hitMask))
        {
            Debug.Log($"Sprayed {hit.collider.name} for {damage} damage!");
        }

        Invoke(nameof(ResetFire), fireRate);
    }

    void ResetFire()
    {
        canFire = true;
    }
}
