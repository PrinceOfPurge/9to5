using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "WeaponData_", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "New Weapon";
    public WeaponType type = WeaponType.Melee;

    [Header("Common")]
    public float damage = 10;
    public float attackRate = 1f;   // Per Second
    public float range = 2f;

    [Header("Ranged")]
    public int magazineSize = 6;
    public int reserveAmmo = 24;
    public float reloadTime = 1.2f;
    public float bulletSpeed = 25f;
    public GameObject projectilePrefab;
    public float spreadAngle = 0f;  // In Degrees

    [Header("Melee")]
    public float meleeRadius = 0.6f;
    public Vector3 meleeOffset = Vector3.forward * 1f;

    [Header("Other")]
    public Sprite hudIcon;
    public GameObject placeholderPrefab;
}
