using UnityEngine;

public interface AbilityAttributes
{
    void DecrementAbilityCooldowns();
    int GetShootAbilityRange();
    void PerformShootAbility(GameObject g);
    void PerformTeammateAbility(GameObject g);
    void Deselect();
}
