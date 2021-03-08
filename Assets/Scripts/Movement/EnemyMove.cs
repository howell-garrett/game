using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class EnemyMove : TacticsMove
{
    private bool hasInitialized = false;

    public bool checkedSelectableCells = false;
    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attributes.health <= 0)
        {
            gameObject.SetActive(false);
        }
        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }
        if (TurnManager.isPlayerTurn)
        {
            return;
        }
    }

    public void Deselect()
    {
        attributes.isSelected = false;
        attributes.movementSelected = false;
        attributes.attackingSelected = false;
        checkedSelectableCells = false;
        teamBounceCell = null;
        finalDestination = null;
        GetComponent<Highlighter>().constant = false;

    }

    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Cell")
                {
                    Cell c = hit.collider.GetComponent<Cell>();
                    if (c.isSelectable && !c.isCurrent)
                    {
                        finalDestination = c;
                        MoveToCell(c);
                    }
                }
            }
        }
    }
}
