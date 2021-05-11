using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class PlayerMove : TacticsMove
{
    private bool hasInitialized = false;
    

    // Start is called before the first frame update
    void Start()
    {
        Vector3 p = new Vector3(0, 0.5f, 0);
        transform.position += p;
    }

    // Update is called once per frame
    void Update()
    {

        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }
    }
}
