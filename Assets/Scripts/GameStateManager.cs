using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
    void SelectEnemy()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Enemy")
                {
                    DeselectAllUnits();
                    EnemyMove player = hit.collider.GetComponent<EnemyMove>();
                    if (!player.hasMoved)
                    {
                        player.isSelected = true;
                        player.hasMoved = true;
                    }
                }
            }
        }
    }

}
