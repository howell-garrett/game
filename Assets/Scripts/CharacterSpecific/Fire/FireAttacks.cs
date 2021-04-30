using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireAttacks : MonoBehaviour, AbilityAttributes
{
    [Header("Flame wheel variables")]
    public int flameWheelCooldown;
    int flameWheelCooldownCurrent;
    public GameObject flameWheel;
    public Button flameWheelButton;
    [Header("Burn attack variables")]
    public int burnRange;
    public GameObject burnPrefab;
    public int burnCooldown;
    int burnCooldownCurrent;
    public Button burnButton;

    private void Start()
    {
        burnCooldownCurrent = 0;
        flameWheelCooldownCurrent = 0;
    }

    public void DecrementAbilityCooldowns() {
        if (flameWheelCooldownCurrent > 0)
        {
            flameWheelCooldownCurrent--;
        } else
        {
            flameWheelButton.interactable = true;
        }

        if (burnCooldownCurrent > 0)
        {
            burnCooldownCurrent--;
        }
        else
        {
            burnButton.interactable = true;
        }
    }

    public int GetShootAbilityRange()
    {
        return burnRange;
    }

    public void PerformTeammateAbility(GameObject teammate)
    {
    }

    public void PerformShootAbility(GameObject target)
    {
        GameStateManager.isAnyoneAttacking = true;
        GameStateManager.DeselectAllUnits();
        burnButton.interactable = false;
        GameObject burn = Instantiate(burnPrefab, transform.position + Vector3.up * .5f, Quaternion.identity);
        burn.GetComponent<ProjectileAttributes>().target = target.transform;
        burnCooldownCurrent = burnCooldown;
    }

    public void PerformFlameWheelAttack()
    {
        StartCoroutine(FlameWheelRoutine());
    }

    public IEnumerator FlameWheelRoutine()
    {
        Instantiate(flameWheel, transform.position + (Vector3.up * .5f), Quaternion.identity);
        flameWheelCooldownCurrent = flameWheelCooldown;
        yield return new WaitForSeconds(0.1f);
        GameStateManager.DeselectAllUnits();
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

    public void Deselect()
    {

    }
}
