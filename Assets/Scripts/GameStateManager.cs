using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;
    public static float gravity = 9.8f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
        SelectUnit();
    }

    public static void DeselectAllUnits()
    {
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().isSelected = false;
        }

        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<EnemyMove>().isSelected = false;
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
                    PlayerMove player = hit.collider.GetComponent<PlayerMove>();
                    if (!player.hasMoved)
                    {
                        DeselectAllUnits();
                        player.isSelected = true;
                    }
                }
                if (hit.collider.tag == "Enemy" && !TurnManager.isPlayerTurn)
                {
                    EnemyMove player = hit.collider.GetComponent<EnemyMove>();
                    if (!player.hasMoved)
                    {
                        DeselectAllUnits();
                        player.isSelected = true;
                    }
                }
            }
        }
    }

    void ResetBoard()
    {
        TurnManager.isPlayerTurn = true;
        DeselectAllUnits();
        for (int i = 0; i < Grid.gameBoard.Count; i++)
        {
            for (int j = 0; j < Grid.gameBoard[i].Count; j++)
            {
                Grid.gameBoard[i][j].ResetVariables();
            }
        }
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<EnemyMove>().hasMoved = false;
            enemyUnits[i].GetComponent<EnemyMove>().isSelected = false;
            enemyUnits[i].GetComponent<EnemyMove>().isMoving = false;
            enemyUnits[i].GetComponent<EnemyMove>().ResetPosition();

        }
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().hasMoved = false;
            playerUnits[i].GetComponent<PlayerMove>().isSelected = false;
            playerUnits[i].GetComponent<PlayerMove>().isMoving = false;
            playerUnits[i].GetComponent<PlayerMove>().ResetPosition();
        }

}

}
