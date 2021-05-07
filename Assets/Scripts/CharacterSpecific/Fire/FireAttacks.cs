using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireAttacks : MonoBehaviour, AbilityAttributes
{
    TacticsAttributes attributes;
    [Header("Flame wheel variables")]
    public int flameWheelCooldown;
    int flameWheelCooldownCurrent;
    public int flameWheelCost;
    public GameObject flameWheel;
    public Button flameWheelButton;
    [Header("Burn attack variables")]
    public int burnRange;
    public int burnCost;
    public GameObject burnPrefab;
    public int burnCooldown;
    int burnCooldownCurrent;
    public Button burnButton;
    [Header("Standard Shot")]
    public GameObject fireShotPrefab;
    public int standardShotRange;
    public int standardShotCost;
    public GameObject bigShotPrefab;
    public int bigShotRange;
    public int bigShotCost;
    public Transform castPoint;

    private void Start()
    {
        burnCooldownCurrent = 0;
        flameWheelCooldownCurrent = 0;
        attributes = GetComponent<TacticsAttributes>();
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

    public int GetStandardShotRange()
    {
        return standardShotRange;
    }

    public int GetStandardShotCost()
    {
        return standardShotCost;
    }

    public int GetBigShotRange()
    {
        return bigShotRange;
    }

    public int GetBigShotCost()
    {
        return bigShotCost;
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

    public void PerformShoot(Cell c, int howManyShots, bool isBigShot)
    {
        int shotCost = standardShotCost;
        if (isBigShot)
        {
            shotCost = bigShotCost;
        }
        if (howManyShots*shotCost > attributes.actionPoints)
        {
            print("Not enough AP");
            return;
        }
        StartCoroutine(ShootCorountine(c, howManyShots, isBigShot, shotCost));
    }

    IEnumerator ShootCorountine(Cell target, int howManyShots, bool isBigShot, int shotCost)
    {
        yield return attributes.TurnTowardsTarget(target.transform.position);
        attributes.anim.SetTrigger("Attack");
        GameObject shotToFire = fireShotPrefab;
        if (isBigShot)
        {
            shotToFire = bigShotPrefab;
        }
        attributes.DecrementActionPoints(howManyShots * shotCost);
        yield return new WaitForSeconds(.7f);
        for (int i = 0; i < howManyShots; i++)
        {
            GameObject projectile = Instantiate(shotToFire, castPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileAttributes>().SetProjectileTarget(target.attachedUnit, attributes.cell);
            yield return new WaitForSeconds(.1f);
        }
    }

    public void ShowBurnRange()
    {
        GameStateManager.DeselectAllCells();
        PlayerUI ui = GetComponent<PlayerUI>();
        ui.HideAddtionals();
        ui.usingShootAbility = true;
        GetComponent<TacticsShoot>().FindSelectableCells(GetComponent<TacticsAttributes>().cell, true, burnRange);
        GetComponent<TacticsAttributes>().shootAbilitySelected = true;

    }

    public void Deselect()
    {

    }
}
