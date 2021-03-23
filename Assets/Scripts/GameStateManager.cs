using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;
    public static float gravity = 9.8f;
    public static bool isAnyoneMoving = false;
    public static bool isAnyoneAttacking = false;
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
        foreach (List<Cell> list in Grid.gameBoard)
        {
            foreach (Cell c in list)
            {
                c.ResetBFSVariables();
            }
        }
    }
    public static void AllCellsNotSelectable()
    {
        foreach (List<Cell> list in Grid.gameBoard)
        {
            foreach (Cell c in list)
            {
               // c.inWalkRange = false;
            }
        }
    }
    public static void ResetCellInfoWithoutParent()
    {
        foreach (List<Cell> list in Grid.gameBoard)
        {
            foreach (Cell c in list)
            {
                c.isSelectable = false;
                c.visited = false;
                c.distance = 0;
            }
        }
    }

    public static void DeselectAllUnits()
    {
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

    public static void SwapUnitLayer(int layer, List<GameObject> units)
    {
        for (int i = 0; i< units.Count; i++)
		{
            units[i].layer = layer;
        }
    }

    void SelectUnit()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Player" && TurnManager.isPlayerTurn)
                {
                    TacticsAttributes player = hit.collider.GetComponent<TacticsAttributes>();
                    if (player.actionPoints > 0 && !player.ReturnCurrentCell().isSelectable)
                    {
                        DeselectAllUnits();
                        activeUnit = player.gameObject;
                        player.isSelected = true;
                    }
                }
                else if (hit.collider.tag == "Enemy" && !TurnManager.isPlayerTurn)
                {
                    TacticsAttributes player = hit.collider.GetComponent<TacticsAttributes>();
                    if (player.actionPoints > 0 && !player.ReturnCurrentCell().isSelectable)
                    {
                        DeselectAllUnits();
                        activeUnit = player.gameObject;
                        player.isSelected = true;
                    }
                }
                else if (hit.collider.tag == "Cell" )
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
