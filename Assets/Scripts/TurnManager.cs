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
                if (enemyUnits[i].GetComponent<TacticsAttributes>().actionPoints > 0)
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
                if (playerUnits[i].GetComponent<TacticsAttributes>().actionPoints > 0)
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
       // GameStateManager.DeselectAllUnits();
        isPlayerTurn = true;
        //GameObject[] playerUnitss = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().ResetAttributes();
            playerUnits[i].GetComponent<PlayerMove>().Deselect();
        }
    }
    public static void NewEnemyTurn()
    {
        //GameStateManager.DeselectAllUnits();
        isPlayerTurn = false;
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<EnemyMove>().ResetAttributes();
        }
    }


}
