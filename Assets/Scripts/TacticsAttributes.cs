using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsAttributes : MonoBehaviour
{
    public int actionPoints = 2;
    public int attackRange = 2;
    public Cell currentCell;

    [Header("Attack and Defense Attributes")]
    public float health = 10;
    public float attackPower = 4;

    [HideInInspector]
    public int actionPointsReset;
    public float healthReset;

    [Header("Positional Information")]
    public int xPositionCurrent;
    public int zPositionCurrent;
    public int yPositionCurrent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetCurrentCell()
    {
        currentCell = Grid.gameBoard[xPositionCurrent][zPositionCurrent];
        //currentCell =  GetTargetCell(gameObject);
        currentCell.setIsCurrent(true);
    }
}
