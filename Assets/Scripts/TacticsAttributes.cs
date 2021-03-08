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

    [HideInInspector]
    public int actionPointsReset;
    public float healthReset;
    public List<Cell> selectableCells = new List<Cell>();

    [Header("Positional Information")]
    public int xPositionCurrent;
    public int zPositionCurrent;
    public int yPositionCurrent;

    public bool movementSelected = false;
    public bool attackingSelected = false;
    public bool isSelected = false;
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

    public Cell ReturnCurrentCell()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject.GetComponent<Cell>();
        }
        return null;
    }

    public void RemoveSelectableCells()
    {
        if (currentCell != null)
        {
            currentCell.isCurrent = false;
            currentCell = null;
        }
        foreach (Cell cell in selectableCells)
        {
            cell.ResetBFSVariables();
        }

        selectableCells.Clear();
    }

    public override string ToString()
    {
        return "Do I exist";
    }
}
