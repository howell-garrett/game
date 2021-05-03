using UnityEngine;

public interface AbilityAttributes
{
    void DecrementAbilityCooldowns();
    int GetShootAbilityRange();
    void PerformShootAbility(GameObject g);
    void PerformShoot(Cell c,int howManyShots, bool isBigShot);
    void PerformTeammateAbility(GameObject g);
    void Deselect();
}
