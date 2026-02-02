using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Setup")]
    public WeaponBase[] weapons;
    public int currentWeaponIndex = 0;

    [Header("HUD Reference")]
    public HUDWeaponDisplay hudDisplay;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction switchAction;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        // Input System actions (you must set them in your InputActions asset)
        attackAction = playerInput.actions["Shoot"];
        switchAction = playerInput.actions["SwitchWeapon"];

        attackAction.performed += OnAttack;
        switchAction.performed += OnSwitch;

        // Equip first weapon
        EquipWeapon(currentWeaponIndex);

        // Update HUD
        if (hudDisplay)
            hudDisplay.UpdateHUD(weapons, currentWeaponIndex);
    }

    void OnEnable()
    {
        if (attackAction != null)
            attackAction.performed += OnAttack;

        if (switchAction != null)
            switchAction.performed += OnSwitch;
    }

    void OnDisable()
    {
        if (attackAction != null)
            attackAction.performed -= OnAttack;

        if (switchAction != null)
            switchAction.performed -= OnSwitch;
    }
    
    void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attacking");
        if (weapons[currentWeaponIndex] != null)
            weapons[currentWeaponIndex].OnAttack();
    }

    void OnSwitch(InputAction.CallbackContext context)
    {
        int nextIndex = (currentWeaponIndex + 1) % weapons.Length;
        EquipWeapon(nextIndex);
    }

    void EquipWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (i == index)
                weapons[i].OnEquip();
            else
                weapons[i].OnUnEquip();
        }

        currentWeaponIndex = index;

        if (hudDisplay)
            hudDisplay.UpdateHUD(weapons, currentWeaponIndex);
    }
}