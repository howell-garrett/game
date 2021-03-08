using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class PlayerMove : TacticsMove
{
    private bool hasInitialized = false;

    public bool checkedSelectableCells = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, transform.forward);

        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }
    }

    public void Deselect()
    {
        attributes.isSelected = false;
        attributes.movementSelected = false;
        attributes.attackingSelected = false;
        checkedSelectableCells = false;
        teamBounceCell = null;
        finalDestination = null;
        GetComponent<Highlighter>().constant = false;
        
    }
}
