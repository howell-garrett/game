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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DeselectAllUnits();
        }
        if (!isAnyoneMoving || !isAnyoneAttacking)
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
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().Deselect();
            playerUnits[i].GetComponent<TacticsAttributes>().Deselect();
            
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
            if (item.cover)
            {
                item.cover.layer = layer;
            }
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

    public static void SwapUnitLayer(int layer, List<GameObject> units)
    {
        for (int i = 0; i< units.Count; i++)
		{
            units[i].layer = layer;
        }
    }

    void Select(TacticsAttributes unit)
    {
        DeselectAllUnits();
        isAnyoneSelected = true;
        activeUnit = unit.gameObject;
        unit.isSelected = true;
        unit.gameObject.GetComponent<BoxCollider>().enabled = false;
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
