using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsAttack : MonoBehaviour
{
    public GameObject attack;
    public TacticsAttributes attributes;
    public int attackPower;
    // Start is called before the first frame update


    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FindCellsInAttackRange()
    {
        attributes.GetCurrentCell();
        attributes.currentCell.FindNeighbors(0);
        List<Cell> l = attributes.currentCell.adjacencyList;
        foreach (Cell c in l)
        {
            c.isInAttackRange = true;
        }
    }

    public void Attack(TacticsAttributes targetAttributes)
    {
        targetAttributes.TakeDamage(attackPower, true);
        attributes.actionPoints--;
        GameStateManager.DeselectAllUnits();
        Instantiate(attack, targetAttributes.transform.position, targetAttributes.transform.rotation);
        attributes.anim.SetTrigger("Attack");
    }
}
