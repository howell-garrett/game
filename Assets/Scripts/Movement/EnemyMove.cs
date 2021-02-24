using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : TacticsMove
{
    private bool hasInitialized = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
        if (Grid.gameBoard != null && !hasInitialized)
        {
            Init();
            hasInitialized = true;
        }
        if (TurnManager.isPlayerTurn)
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
            if (Physics.Raycast(ray, out hit))
            {
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
