using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireAttacks : MonoBehaviour, AbilityAttributes
{
    public int flameWheelCooldown;
    int flameWheelCooldownCurrent;
    public GameObject flameWheel;
    public Button flameWheelButton;

    public int burnRange;

    public void DecrementAbilityCooldowns() {
        if (flameWheelCooldownCurrent > 0)
        {
            flameWheelCooldownCurrent--;
        } else
        {
            flameWheelButton.interactable = true;
        }
    }

    public int GetShootAbilityRange()
    {
        return burnRange;
    }

    public void PerformFlameWheelAttack()
    {
        Instantiate(flameWheel, transform.position + (Vector3.up * .5f), Quaternion.identity);
        flameWheelCooldownCurrent = flameWheelCooldown;
    }

    public void ShowBurnRange()
    {
        GameStateManager.DeselectAllCells();
        PlayerUI ui = GetComponent<PlayerUI>();
        ui.HideAddtionals();
        ui.usingShootAbility = true;
        GetComponent<TacticsShoot>().FindSelectableCells(GetComponent<TacticsAttributes>().cell, true);
        GetComponent<TacticsAttributes>().shootAbilitySelected = true;

    }
}
