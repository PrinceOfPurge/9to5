using UnityEngine;
using UnityEngine.UI;

public class HUDWeaponDisplay : MonoBehaviour
{
    [Header("HUD Elements")]
    public Image[] weaponIcons;

    // Called by WeaponManager when weapon changes
    public void UpdateHUD(WeaponBase[] weapons, int activeIndex)
    {
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (i < weapons.Length)
            {
                weaponIcons[i].sprite = weapons[i].weaponIcon;
                weaponIcons[i].color = (i == activeIndex) ? Color.white : new Color(1, 1, 1, 0.4f);
            }
            else
            {
                weaponIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
