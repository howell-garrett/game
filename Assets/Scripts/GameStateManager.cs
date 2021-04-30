using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HighlightingSystem;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;
    public static float gravity = 9.8f;
    public static bool isAnyoneMoving = false;
    public static bool isAnyoneAttacking = false;
    public static bool isAnyoneSelected = false;
    public static GameObject activeUnit;
    public static GameObject activeLaunchUnit;
    public GameObject[] players;
    public GameObject[] enemies;
    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
    }

    // Update is called once per frame
    void Update()
    {
        ListenForDeselectTeamJump();
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DeselectAllUnits();
        }
        if (!isAnyoneMoving && !isAnyoneAttacking)
        {
            SelectUnit();
        }
    }

    public static void DeselectAllCells()
    {
        foreach (Cell c in GameStateManager.FindAllCells())
        {
            c.ResetBFSVariables();
        }
    }

    public static void ComputeAdjList()
    {
        Cell[] cells = GameStateManager.FindAllCells();
        foreach (Cell c in cells)
        {
            c.FindNeighbors(0);
        }
    }

    public static void ResetCellInfoWithoutParent()
    {
        foreach (Cell c in GameStateManager.FindAllCells())
        {
            c.isSelectable = false;
            c.visited = false;
            c.distance = 0;
        }
    }
    public static void ResetCellBools()
    {
        foreach (Cell c in GameStateManager.FindAllCells())
        {
            c.isSelectable = false;
            c.isInShootRange = false;
            c.isInAttackRange = false;
            c.isInAbilityRange = false;
            c.isCurrent = false;
            c.isTarget = false;
        }
    }

    public static void DeselectAllUnits()
    {
        if (activeUnit)
        {
            activeUnit.GetComponent<BoxCollider>().enabled = true;
        }
        isAnyoneSelected = false;
        activeUnit = null;
        activeLaunchUnit = null;
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().Deselect();
            playerUnits[i].GetComponent<TacticsAttributes>().Deselect();
            if (playerUnits[i].GetComponent<AbilityAttributes>() != null)
            {
                playerUnits[i].GetComponent<AbilityAttributes>().Deselect();
            }
            
        }

        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<TacticsAttributes>().Deselect();
            enemyUnits[i].GetComponent<EnemyMove>().Deselect();
            enemyUnits[i].GetComponent<Highlighter>().constant = false;
        }
        DeselectAllCells();
    }

    public static void ChangeNeighboringCoverLayer(Cell c, bool raycastWillHit)
    {
        int layer = 0;
        if (!raycastWillHit)
        {
            layer = 2;
        }
        List<Cell> neighbors = c.GetAllNeighbors();
        foreach (Cell item in neighbors)
        {
            if (item.attachedCover)
            {
                item.attachedCover.layer = layer;
            }
        }
    }

    public static Directions GetOppositeDirection(Directions d)
    {
        if (d == Directions.Up)
        {
            return Directions.Down;
        }
        else if (d == Directions.Down)
        {
            return Directions.Up;
        }
        else if (d == Directions.Left)
        {
            return Directions.Right;
        }
        else
        {
            return Directions.Left;
        }
    }

    public static void ChangeUnitsRaycastLayer(bool raycastWillHit)
    {
        int layer = 0;
        if (!raycastWillHit)
        {
            layer = 2;
        }
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerUnits)
        {
            player.layer = layer;
        }
        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyUnits)
        {
            enemy.layer = layer;
        }
    }

    public static Cell FindCell(int x, int z)
    {
        foreach (Cell cell in FindAllCells())
        {
            if (cell.xCoordinate == x && cell.zCoordinate == z)
            {
                return cell;
            }
        }
        return null;
    }

    public static Cell[] FindAllCells()
    {
        GameObject[] cellObjects = GameObject.FindGameObjectsWithTag("Cell");
        Cell[] cells = new Cell[cellObjects.Length];
        for (int i = 0; i < cellObjects.Length; i++)
        {
            cells[i] = cellObjects[i].GetComponent<Cell>();
        }
        return cells;
    }

    public static void SwapUnitTriggerColliders(bool isTrigger)
    {
        GameObject[] playerArr = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemyArr = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyArr.Length; i++) {
            enemyArr[i].GetComponent<BoxCollider>().isTrigger = isTrigger;
        }
        for (int i = 0; i < playerArr.Length; i++)
        {
            playerArr[i].GetComponent<BoxCollider>().isTrigger = isTrigger;
        }
    }

    public static void SwapUnitLayer(int layer, List<GameObject> units)
    {
        for (int i = 0; i< units.Count; i++)
		{
            units[i].layer = layer;
        }
    }

    public static void Select(TacticsAttributes unit)
    {
        DeselectAllUnits();
        isAnyoneSelected = true;
        activeUnit = unit.gameObject;
        unit.isSelected = true;
        unit.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    void ListenForDeselectTeamJump()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!isAnyoneMoving && activeUnit && activeUnit.GetComponent<TacticsAttributes>().movementSelected)
            {
                TacticsMove tm = activeUnit.GetComponent<TacticsMove>();
                if (tm.teamBounceCells.Count > 0)
                {
                    tm.teamBounceCells.RemoveAt(tm.teamBounceCells.Count - 1);
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.tag == "Cell")
                        {
                            Cell c = hit.collider.GetComponent<Cell>();
                            LineRenderer lr = activeUnit.GetComponent<LineRenderer>();
                            lr.positionCount = lr.positionCount - 13;
                            if (tm.teamBounceCells.Count > 0)
                            {
                                lr.positionCount = lr.positionCount - 13;
                                ResetCellInfoWithoutParent();
                                tm.FindSelectableCells(tm.teamBounceCells[tm.teamBounceCells.Count - 1]);
                                lr.positionCount += 13;
                                if (c.isSelectable)
                                {
                                    tm.DrawBounceLine(c.transform.position, false);
                                }
                            } else
                            {
                                ResetCellInfoWithoutParent();
                                tm.FindSelectableCells(activeUnit.GetComponent<TacticsAttributes>().cell);
                                if (c.isSelectable)
                                {
                                    tm.MoveToCell(c, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void SelectUnit()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Player" && TurnManager.isPlayerTurn && !isAnyoneSelected)
                {
                    TacticsAttributes player = hit.collider.GetComponent<TacticsAttributes>();
                    if (player.actionPoints > 0 && !player.ReturnCurrentCell().isSelectable)
                    {
                        Select(player);
                    }
                }
                else if (hit.collider.tag == "Enemy" && !TurnManager.isPlayerTurn && !isAnyoneSelected)
                {
                    TacticsAttributes player = hit.collider.GetComponent<TacticsAttributes>();
                    if (player.actionPoints > 0 && !player.ReturnCurrentCell().isSelectable)
                    {
                        Select(player);
                    }
                }
                else if (hit.collider.tag == "Cell")
                {
                    if (!hit.collider.gameObject.GetComponent<Cell>().isSelectable)
                    {
                        if (!hit.collider.gameObject.GetComponent<Cell>().isInAttackRange)
                        {
                            if (!hit.collider.gameObject.GetComponent<Cell>().isInShootRange)
                            {
                                if (!isAnyoneAttacking && !isAnyoneMoving)
                                {
                                    DeselectAllUnits();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void ResetBoard()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

}
