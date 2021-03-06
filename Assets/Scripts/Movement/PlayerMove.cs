using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    public GameObject selectedArrow;
    private bool hasInitialized = false;

    public bool movementSelected = false;
    public bool attackingSelected = false;

    public bool checkedSelectableCells = false;
    // Start is called before the first frame update
    void Start()
    {
        HideUI();
        selectedArrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, transform.forward);

        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }

        if (!TurnManager.isPlayerTurn)
        {
            HideUI();
            return;
        }

        if (isSelected)
        {
            ShowUI();
            if (movementSelected)
            {
                showMovementInfo();
            }
            if (attackingSelected)
            {
                showAttackInfo();
            }
            
        } else
        {
            HideUI();
        }
    }

    void ShowUI()
    {
        selectedArrow.SetActive(true);
        transform.GetChild(0).GetComponent<Canvas>().enabled = true;
    }

    void HideUI()
    {
        selectedArrow.SetActive(false);
        transform.GetChild(0).GetComponent<Canvas>().enabled = false;
    }

    void showMovementInfo()
    {
        
        if (Grid.gameBoard != null && !isMoving)
        {
            CheckMouse();
            if (!checkedSelectableCells)
            {
                FindSelectableCells(currentCell);
                checkedSelectableCells = true;
            }
        }
        else
        {
            Move();
        }
    }

    public void selectMovement() {
        attackingSelected = false;
        movementSelected = true;
    }

    public void selectAttack()
    {
        attackingSelected = true;
        movementSelected = false;
        GameStateManager.DeselectAllCells();
    }

    void showAttackInfo()
    {
        GameStateManager.DeselectAllCells();
        if (Grid.gameBoard != null && !isMoving)
        {
            FindCellsInAttackRange();
            CheckMouse();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        movementSelected = false;
        attackingSelected = false;
        checkedSelectableCells = false;
        teamBounceCell = null;
        finalDestination = null;
    }

    void ShowSelectableTeamBounceCells(Cell c)
    {
        teamBounceCell = c;
        Debug.Log(c);
        GameStateManager.asdf();
        FindSelectableCells(c);
    }
    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.tag == "Cell")
                {
                    Cell c = hit.collider.GetComponent<Cell>();
                    if (c.isSelectable && !c.isCurrent)
                    {
                        finalDestination = c;
                        if (c.attachedUnit != null)
                        {
                            if (c.attachedUnit.tag != tag)
                            {

                                MoveToCell(c);
                            }
                            else
                            {
                                teamBounceCell = c;
                                ShowSelectableTeamBounceCells(c);
                            }
                        }
                        else if (teamBounceCell != null)
                        {
                            MoveToCell(teamBounceCell);
                        }
                        else {
                            MoveToCell(c);
                        }
                    }
                    else if (c.isInAttackRange)
                    {
                        Attack(c.attachedUnit.GetComponent<EnemyMove>());
                    }
                }
            }
        }
    }
}
