using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base Class for all/future Weapons

public abstract class WeaponBase : MonoBehaviour
{
    public string weaponName;
    public Sprite weaponIcon; // For HUD
    public bool isEquipped;

    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    public abstract void OnAttack();

    public virtual void OnEquip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
    }

    public virtual void OnUnEquip()
    {
        isEquipped = false;
        gameObject.SetActive(false);
    }

}
