using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class EnemyUI : MonoBehaviour
{
    EnemyMove pm;
    EnemyAttack pa;
    public TacticsAttributes attributes;
    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<EnemyMove>();
        pa = GetComponent<EnemyAttack>();
        attributes = GetComponent<TacticsAttributes>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnManager.isPlayerTurn)
        {
            HideUI();
            return;
        }
        if (attributes.isSelected)
        {
            ShowUI();
            if (attributes.movementSelected)
            {
                showMovementInfo();
            }
            if (attributes.attackingSelected)
            {
                showAttackInfo();
            }

        }
        else
        {
            HideUI();
        }
    }

    void ShowUI()
    {
        GetComponent<Highlighter>().constant = true;
        transform.GetChild(0).GetComponent<Canvas>().enabled = true;
    }

    void HideUI()
    {
        transform.GetChild(0).GetComponent<Canvas>().enabled = false;
    }

    void showMovementInfo()
    {
        if (Grid.gameBoard != null && !pm.isMoving)
        {
            CheckMouse();
            if (!pm.checkedSelectableCells)
            {
                pm.FindSelectableCells(attributes.currentCell);
                pm.checkedSelectableCells = true;
            }
        }
        else
        {
            pm.Move();
        }
    }

    public void selectMovement()
    {
        attributes.attackingSelected = false;
        attributes.movementSelected = true;
    }

    public void selectAttack()
    {
        attributes.attackingSelected = true;
        attributes.movementSelected = false;
        GameStateManager.DeselectAllCells();
    }

    void showAttackInfo()
    {
        GameStateManager.DeselectAllCells();
        if (Grid.gameBoard != null && !pm.isMoving)
        {
            pa.FindCellsInAttackRange();
            CheckMouse();
        }
    }

    public void Deselect()
    {
        pm.Deselect();
    }

    void ShowSelectableTeamBounceCells(Cell c)
    {
        pm.teamBounceCell = c;
        GameStateManager.ResetCellInfoWithoutParent();
        pm.FindSelectableCells(c);
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
                        pm.finalDestination = c;
                        if (c.attachedUnit != null)
                        {
                            if (c.attachedUnit.tag != tag)
                            {

                                pm.MoveToCell(c, true);
                            }
                            else
                            {
                                pm.teamBounceCell = c;
                                ShowSelectableTeamBounceCells(c);
                            }
                        }
                        else if (pm.teamBounceCell != null)
                        {
                            pm.MoveToCell(pm.teamBounceCell, true);
                        }
                        else
                        {
                            pm.MoveToCell(c, true);
                        }
                    }
                    else if (c.isInAttackRange)
                    {
                        pa.Attack(c.attachedUnit.GetComponent<TacticsAttributes>());
                    }
                } else if (hit.collider.tag == "Enemy")
                {
                    Cell c = hit.collider.gameObject.GetComponent<TacticsAttributes>().ReturnCurrentCell();
                    if (c.isSelectable)
                    {
                        pm.teamBounceCell = c;
                        ShowSelectableTeamBounceCells(c);
                    }
                }
                else if (hit.collider.tag == "Player")
                {
                    Cell c = hit.collider.gameObject.GetComponent<TacticsAttributes>().ReturnCurrentCell();
                    if (c.isInAttackRange)
                    {
                        pa.Attack(c.attachedUnit.GetComponent<TacticsAttributes>());
                    }
                }
            }
        }
    }
}
