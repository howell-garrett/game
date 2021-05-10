using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static bool isPlayerTurn = true;

    public static GameObject[] enemyUnits; 
    public static GameObject[] playerUnits;
    // Start is called before the first frame update
    void Start()
    {
        UpdateUnitCount();
}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            NewPlayerTurn();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            NewEnemyTurn();
        }
    }

    public static void MonitorTurnStatus()
    {
        bool allGone = true;
        if (!isPlayerTurn)
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

    public static void UpdateUnitCount()
    {
        enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        playerUnits = GameObject.FindGameObjectsWithTag("Player");
    }

    public static void NewPlayerTurn()
    {
        GameStateManager.CreatePopupAlert("Player");
        GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Player Turn";
        //Debug.Log(GameStateManager.activeUnit.name);
        // GameStateManager.DeselectAllUnits();
        isPlayerTurn = true;
        //GameObject[] playerUnitss = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerUnits.Length; i++)
        {
            playerUnits[i].GetComponent<PlayerMove>().ResetAttributes();
            playerUnits[i].GetComponent<PlayerMove>().Deselect();
            playerUnits[i].GetComponent<TacticsAttributes>().CheckStatus();
            if (playerUnits[i].GetComponent<AbilityAttributes>() != null)
            {
                playerUnits[i].GetComponent<AbilityAttributes>().DecrementAbilityCooldowns();
            }
        }
    }
    public static void NewEnemyTurn()
    {
        GameStateManager.CreatePopupAlert("Enemy");
        GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Enemy Turn";
        isPlayerTurn = false;
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].GetComponent<EnemyMove>().ResetAttributes();
            enemyUnits[i].GetComponent<TacticsAttributes>().CheckStatus();
        }
    }


}
