using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HighlightingSystem;

public class PlayerUI : MonoBehaviour
{
    TacticsMove pm;
    TacticsAttack pa;
    TacticsShoot ps;
    TacticsAttributes attributes;

    public Text actionPointCount;
    // Start is called before the first frame update
    void Start()
    {
        if (tag == "Player")
        {
            pm = GetComponent<PlayerMove>();
            pa = GetComponent<PlayerAttack>();
            ps = GetComponent<PlayerShoot>();
        } else if (tag == "Enemy")
        {
            pm = GetComponent<EnemyMove>();
            pa = GetComponent<EnemyAttack>();
            ps = GetComponent<EnemyShoot>();
        }
        
        attributes = GetComponent<TacticsAttributes>();
        UpdateActionPointsDisplay(attributes.actionPoints);
    }

    // Update is called once per frame
    void Update()
    {
        if (!TurnManager.isPlayerTurn)
        {
            //HideUI();
            //return;
        }

        if (attributes.isSelected)
        {
            ShowUI();
            if (attributes.movementSelected)
            {
                showMovementInfo();
            }
            else if (attributes.attackingSelected)
            {
                showAttackInfo();
            }
            else if (attributes.shootSelected)
            {
                showShootInfo();
            }

        }
        else
        {
            HideUI();
        }
    }

    void ShowUI()
    {
        UpdateActionPointsDisplay(attributes.actionPoints);
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

    void showShootInfo()
    {
        if (Grid.gameBoard != null && !ps.isShooting)
        {
            CheckMouse();
            if (!ps.checkedSelectableCells)
            {
                attributes.GetCurrentCell();
                ps.FindSelectableCells(attributes.currentCell);
                ps.checkedSelectableCells = true;
            }
        }
        else
        {
            ps.Shoot();
        }
    }

    void ResetCheckedSelectable (bool movement, bool attack, bool shoot)
    {
        attributes.movementSelected = movement;
        attributes.attackingSelected = attack;
        attributes.shootSelected = shoot;
        pm.checkedSelectableCells = false;
        ps.checkedSelectableCells = false;
        GameStateManager.DeselectAllCells();
    }

    public void selectShoot()
    {
        ResetCheckedSelectable(false, false, true);
    }

    public void selectMovement()
    {
        ResetCheckedSelectable(true, false, false);
        Debug.Log("SelectMovement");
    }

    public void selectAttack()
    {
        ResetCheckedSelectable(false, true, false);
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

    void UpdateActionPointsDisplay(int ap)
    {
        actionPointCount.text = "AP: " + ap;
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
                        c.isFinalDestination = true;
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
                    
                    else if (c.isInAttackRange && c.attachedUnit)
                    {
                        pa.Attack(c.attachedUnit.GetComponent<TacticsAttributes>());
                    }
                    else if (c.isInShootRange && c.attachedUnit)
                    {
                        ps.SpawnBullet(c.attachedUnit);
                        ps.isShooting = true;
                    }
                }
                else if (hit.collider.tag == tag)
                {
                    Cell c = hit.collider.gameObject.GetComponent<TacticsAttributes>().ReturnCurrentCell();
                    if (c.isSelectable)
                    {
                        pm.teamBounceCell = c;
                        ShowSelectableTeamBounceCells(c);
                    } 
                }
                else if (hit.collider.tag != tag && hit.collider.gameObject.GetComponent<TacticsAttributes>())
                {
                    Cell c = hit.collider.gameObject.GetComponent<TacticsAttributes>().ReturnCurrentCell();
                    if (c.isInAttackRange)
                    {
                        pa.Attack(c.attachedUnit.GetComponent<TacticsAttributes>());
                    } else if (c.isInShootRange)
                    {
                        ps.SetUpShot(c.attachedUnit);
                    }
                }
            }
        }
    }

}
