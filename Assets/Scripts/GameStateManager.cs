﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class GameStateManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;
    public static float gravity = 9.8f;
    public static bool isAnyoneMoving = false;
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
        if (!isAnyoneMoving)
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
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().Deselect();
            
        }

        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<TacticsAttributes>().isSelected = false;
            enemyUnits[i].GetComponent<Highlighter>().constant = false;
        }
        DeselectAllCells();
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
                        player.isSelected = true;
                    }
                }
                else if (hit.collider.tag == "Enemy" && !TurnManager.isPlayerTurn)
                {
                    TacticsAttributes player = hit.collider.GetComponent<TacticsAttributes>();
                    if (player.actionPoints > 0 && !player.ReturnCurrentCell().isSelectable)
                    {
                        DeselectAllUnits();
                        player.isSelected = true;
                    }
                }
                else if (hit.collider.tag == "Cell" )
                {
                    
                    if (!hit.collider.gameObject.GetComponent<Cell>().isSelectable)
                    {
                        DeselectAllUnits();
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
                Grid.gameBoard[i][j].ResetBFSVariables();
            }
        }
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<TacticsAttributes>().health = enemies[i].GetComponent<TacticsAttributes>().healthReset;
            enemies[i].GetComponent<EnemyMove>().ResetAttributes();
            enemies[i].GetComponent<EnemyMove>().ResetPosition();

        }
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<PlayerMove>().ResetAttributes();
            players[i].GetComponent<PlayerMove>().ResetPosition();
        }

}

}
