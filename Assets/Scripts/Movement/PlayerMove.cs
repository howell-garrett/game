using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{

    private bool hasInitialized = false;
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

        if (!TurnManager.isPlayerTurn)
        {
            return;
        }

        if (isSelected)
        {
            if (Grid.gameBoard != null && !isMoving)
            {
                FindSelectableCells();
                CheckMouse();
            }
            else
            {
                Move();
            }
        }
    }

    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.tag == "Cell")
                {
                    Cell c = hit.collider.GetComponent<Cell>();
                    if (c.isSelectable && !c.isCurrent)
                    {
                        MoveToCell(c);
                    }
                }
            }
        }
    }
}
