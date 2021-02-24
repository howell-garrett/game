using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    public GameObject selectedArrow;
    private bool hasInitialized = false;

    public bool movementSelected = false;
    public bool attackingSelected = false;
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
            FindSelectableCells();
            CheckMouse();
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
                        MoveToCell(c);
                    }
                    if (c.isInAttackRange)
                    {
                        Attack(c.attachedUnit.GetComponent<EnemyMove>());
                    }
                }
            }
        }
    }
}
