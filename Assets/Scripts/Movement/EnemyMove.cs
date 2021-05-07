using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class EnemyMove : TacticsMove
{
    private bool hasInitialized = false;
    
    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }
        if (TurnManager.isPlayerTurn)
        {
            return;
        }
    }
}
