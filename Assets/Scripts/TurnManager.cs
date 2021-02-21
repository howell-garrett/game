using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;

    public static GameObject[] enemyUnits; 
    public static GameObject[] playerUnits; 
    // Start is called before the first frame update
    void Start()
    {
        enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        playerUnits = GameObject.FindGameObjectsWithTag("Player");
}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            NewPlayerTurn();
        }

        if (isPlayerTurn)
        {
            monitorTurnStatus("Player");
        } else
        {
            monitorTurnStatus("Enemy");
        }
    }

    void monitorTurnStatus(string whoToMonitor)
    {
        bool allGone = true;
        if (whoToMonitor == "Enemy")
        {
            for (int i = 0; i < enemyUnits.Length; i++)
            {
                if (!enemyUnits[i].GetComponent<EnemyMove>().hasMoved)
                {
                    allGone = false;
                }
            }
            if (allGone)
            {
                NewPlayerTurn();
            }
        } else
        {
            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (!playerUnits[i].GetComponent<PlayerMove>().hasMoved)
                {
                    allGone = false;
                }
            }
            if (allGone)
            {
                NewEnemyTurn();
            }
        }
    }


    public static void NewPlayerTurn()
    {
        isPlayerTurn = true;
        GameObject[] playerUnitss = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnitss.Length; i++)
        {
            playerUnitss[i].GetComponent<PlayerMove>().hasMoved = false;
            playerUnitss[i].GetComponent<PlayerMove>().isSelected = false;
        }
    }
    public static void NewEnemyTurn()
    {
        isPlayerTurn = false;
        GameObject[] enemyUnitss = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyUnitss.Length; i++)
        {
            enemyUnitss[i].GetComponent<EnemyMove>().hasMoved = false;
            enemyUnitss[i].GetComponent<EnemyMove>().isSelected = false;
        }
    }


}
