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
    int bigShotRange;
    int standardShotRange;
    int range;
    TacticsAttributes attributes;
    public GameObject shotCount;
    InputField shotCountInputField;
    public Text actionPointCount;
    public Text movementCostUI;
    public bool usingShootAbility;

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
        usingShootAbility = false;
        attributes = GetComponent<TacticsAttributes>();
        UpdateActionPointsDisplay(attributes.actionPoints);
        if (GetComponent<AbilityAttributes>() != null)
        {
            standardShotRange = GetComponent<AbilityAttributes>().GetStandardShotRange();
            bigShotRange = GetComponent<AbilityAttributes>().GetBigShotRange();
        }  else
        {
            standardShotRange = 10;
            bigShotRange = 10;
        }
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
                ShowMovementInfo();
            }
            else if (attributes.attackingSelected)
            {
                ShowAttackInfo();
            }
            else if (attributes.shootSelected)
            {
                ShowShootInfo();
            } else if (attributes.shootAbilitySelected)
            {
                ShowShootAbilityInfo();
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

    void ShowMovementInfo()
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
            HideUI();
            pm.Move();
            
        }
    }

    void ShowShootInfo()
    {
        if (!ps.isShooting)
        {
            CheckMouse();
            if (!ps.checkedSelectableCells && !usingShootAbility)
            {
                attributes.GetCurrentCell();
                ps.FindSelectableCells(attributes.currentCell, false, range);
                ps.checkedSelectableCells = true;
            }
        }
        else
        {
            ps.Shoot();
        }
    }

    public void ShowShootAbilityInfo()
    {
        if (!ps.isShooting)
        {
            CheckMouse();
        }
        else
        {
            ps.Shoot();
        }
    }

    void ResetCheckedSelectable (bool movement, bool attack, bool shoot)
    {
        HideAddtionals();
        attributes.movementSelected = movement;
        attributes.attackingSelected = attack;
        attributes.shootSelected = shoot;
        pm.checkedSelectableCells = false;
        ps.checkedSelectableCells = false;
        GameStateManager.DeselectAllCells();
    }

    public void SelectShoot()
    {
        range = standardShotRange;
        ResetCheckedSelectable(false, false, true);
        shotCount.SetActive(true);
    }
    public void SelectBigShoot()
    {
        range = bigShotRange;
        ResetCheckedSelectable(false, false, true);
    }

    public void SelectMovement()
    {
        movementCostUI.gameObject.SetActive(true);
        ResetCheckedSelectable(true, false, false);
    }

    public void SelectAttack()
    {
        HideAddtionals();
        ResetCheckedSelectable(false, true, false);
    }

    void ShowAttackInfo()
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
    }

    public void HideAddtionals()
    {
        GetComponent<LineRenderer>().enabled = false;
        shotCount.SetActive(false);
        movementCostUI.gameObject.SetActive(false);
        usingShootAbility = false;
        GameStateManager.activeUnit.GetComponent<TacticsAttributes>().movementSelected = false;
        GameStateManager.activeUnit.GetComponent<TacticsAttributes>().attackingSelected = false;
        GameStateManager.DeselectAllCells();
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
                        if (usingShootAbility)
                        {
                            GetComponent<AbilityAttributes>().PerformShootAbility(c.attachedUnit);
                            usingShootAbility = false;
                        }
                        else if (shotCountInputField.gameObject.activeSelf)
                        {
                            int howManyShots = Int32.Parse(shotCountInputField.text);
                            if (howManyShots == 0)
                            {
                                return;
                            }
                            if (!ps.HasLineOfSight(c))
                            {
                                return;
                            }
                            count = 0;
                            ps.PerformShoot(c, howManyShots, false);
                            ps.isShooting = true;
                            shotCountInputField.text = "0";
                            shotCount.SetActive(false);
                        } else
                        {
                            if (!ps.HasLineOfSight(c))
                            {
                                return;
                            }
                            count = 0;
                            ps.isShooting = true;
                            ps.PerformShoot(c, 1, true);
                            shotCountInputField.text = "0";
                            shotCount.SetActive(false);
                        }
                    }
                }
            }
        }
    }

}
