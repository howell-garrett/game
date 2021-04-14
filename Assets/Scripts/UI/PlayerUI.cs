using System.Collections;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HighlightingSystem;

public class PlayerUI : MonoBehaviour
{
    TacticsMove pm;
    TacticsAttack pa;
    TacticsShoot ps;
    TacticsAttributes attributes;
    public GameObject shotCount;
    InputField shotCountInputField;
    public Text actionPointCount;
    public Text movementCostUI;

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
        shotCount.SetActive(false);
        shotCountInputField = shotCount.GetComponent<InputField>();
        shotCountInputField.text = "0";
        attributes = GetComponent<TacticsAttributes>();
        UpdateActionPointsDisplay(attributes.actionPoints);
    }

    int count = 0;
    float timer = 0;
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
        if (!pm.isMoving)
        {
            CheckMouse();
            if (!pm.checkedSelectableCells)
            {
                pm.FindSelectableCells(attributes.cell);
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
        if ( !ps.isShooting)
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
        movementCostUI.gameObject.SetActive(false);
        ResetCheckedSelectable(false, false, true);
        shotCount.SetActive(true);
    }

    public void selectMovement()
    {
        movementCostUI.gameObject.SetActive(true);
        ResetCheckedSelectable(true, false, false);
        shotCount.SetActive(false);
    }

    public void selectAttack()
    {
        movementCostUI.gameObject.SetActive(false);
        ResetCheckedSelectable(false, true, false);
        shotCount.SetActive(false);
    }

    void showAttackInfo()
    {
        GameStateManager.DeselectAllCells();
        if (!pm.isMoving)
        {
            pa.FindCellsInAttackRange();
            CheckMouse();
        }
    }

    public void EndTurn()
    {
        attributes.actionPoints = 0;
        GameStateManager.DeselectAllUnits();
    }

    public void Deselect()
    {
        pm.Deselect();
    }

    void ShowSelectableTeamBounceCells(Cell c)
    {
        GameStateManager.ResetCellInfoWithoutParent();
        pm.FindSelectableCells(c);
    }

    void UpdateActionPointsDisplay(int ap)
    {
        actionPointCount.text = "AP: " + ap;
    }

    public void ClickCell(Cell c) {
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
                else if (pm.teamBounceCells.Count < attributes.maximumTeamBounces)
                {
                    pm.teamBounceCells.Add(c);
                    pm.DrawBounceLine(c.transform.position, true);
                    ShowSelectableTeamBounceCells(c);
                }
            }
            else if (pm.teamBounceCells.Count > 0)
            {
                pm.MoveToCell(pm.teamBounceCells[0], true);
                foreach (Cell item in pm.teamBounceCells)
                {
                    item.isTarget = true;
                }
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

    void CheckMouse()
    {
        //Debug.Log(EventSystem.current.IsPointerOverGameObject());
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (hit.collider.tag == "Cell")
                {
                    ClickCell(hit.collider.GetComponent<Cell>());
                }
                else if (hit.collider.tag == tag && hit.collider.gameObject.name != name)
                {
                    Cell c = hit.collider.gameObject.GetComponent<TacticsAttributes>().ReturnCurrentCell();
                    if (c.isSelectable)
                    {
                        if (pm.teamBounceCells.Count < attributes.maximumTeamBounces)
                        {
                            pm.teamBounceCells.Add(c);
                            //Debug.Log(c);
                            pm.DrawBounceLine(c.transform.position, true);
                            ShowSelectableTeamBounceCells(c);
                        }
                            
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
                        int howManyShots = Int32.Parse(shotCountInputField.text);
                        if (howManyShots * ps.shotCost > attributes.actionPoints)
                        {
                            Debug.Log("not enough AP");
                            return;
                        }
                        if (howManyShots == 0)
                        {
                            return;
                        }
                        if (!ps.HasLineOfSight(c))
                        {
                            return;
                        }
                        count = 0;
                        StartCoroutine(ShootCoroutine(ps, c, howManyShots));
                        shotCountInputField.text = "0";
                        shotCount.SetActive(false);
                    }
                }
            }
        }
    }

    IEnumerator ShootCoroutine(TacticsShoot ps, Cell c, int howManyShots)
    {
        
        while (count < howManyShots)
        {
            
            yield return new WaitForSeconds(.1f);
            ps.SetUpShot(c.attachedUnit);
            timer = 0;
            count++;
        }
    }

}
