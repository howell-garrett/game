using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public bool isWalkable = true; //movement is impossible, lava, deepwater etc NOT OCCUPIED

    public bool isCurrent;
    public bool isTarget;
    public bool isSelectable;
    public bool isOccupied;
    public List<Cell> adjacencyList;
    //BFS vars
    public bool visited;
    public Cell parent;
    public int distance;

    public int xCoordinate = 0;
    public int yCoordinate = 0;
    public int height;
    public bool isBlocked;

    public Cell(int x, int y)
    {
        xCoordinate = x;
        yCoordinate = y;
        adjacencyList = new List<Cell>();
        height = 1; //default
        isBlocked = false;
    }
    public Cell(int x, int y, int givenHeight)
    {
        xCoordinate = x;
        yCoordinate = y;
        adjacencyList = new List<Cell>();
        height = givenHeight;
        isBlocked = false;
    }

    public void setIsCurrent(bool b)
    {
        isCurrent = b;
    }

    // Start is called before the first frame update
    private void Start()
    {
        ResetVariables();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCellColor();
    }

    public void ResetVariables()
    {

        isCurrent = false;
        isTarget = false;
        isSelectable = false;
        adjacencyList = new List<Cell>();

        adjacencyList.Clear();
        visited = false;
        parent = null;
        distance = 0;
    }

    public void FindNeighbors(float jumpHeight)
    {
        ResetVariables();
        if (xCoordinate > 0) // left
        {
            if (!Grid.gameBoard[xCoordinate - 1][yCoordinate].isBlocked) {
                adjacencyList.Add(Grid.gameBoard[xCoordinate - 1][yCoordinate]);
            }
        }
        if (xCoordinate < Grid.gameBoard.Count - 1) // Right
        {
            if (!Grid.gameBoard[xCoordinate + 1][yCoordinate].isBlocked)
            {
                adjacencyList.Add(Grid.gameBoard[xCoordinate + 1][yCoordinate]);
            }
        }
        if (yCoordinate > 0) // Back
        {
            if (!Grid.gameBoard[xCoordinate][yCoordinate - 1].isBlocked)
            {
                adjacencyList.Add(Grid.gameBoard[xCoordinate][yCoordinate - 1]);
            }
        }
        if (yCoordinate < Grid.gameBoard[xCoordinate].Count - 1) // Front
        {
            if (!Grid.gameBoard[xCoordinate][yCoordinate + 1].isBlocked)
            {
                adjacencyList.Add(Grid.gameBoard[xCoordinate][yCoordinate + 1]);
            }
        }
    }

    public void CheckCell(Vector3 direction, float jumpHeight)
    {
        Vector3 halfExtents = new Vector3(.25f, (1+jumpHeight)/2, .25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);
        foreach (Collider item in colliders)
        {
            Cell cell = item.GetComponent<Cell>();
            if (cell != null && cell.isWalkable)
            {
                RaycastHit hit; 

                if(!Physics.Raycast(cell.transform.position, Vector3.up, out hit, 1)) //if not occupied, stupid change this
                {
                    adjacencyList.Add(cell);
                }
            }
        }
    }

    void UpdateCellColor()
    {
        if (isCurrent)
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (isTarget)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else if (isSelectable)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else if (isBlocked)
        {
            GetComponent<Renderer>().material.color = Color.gray;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public override string ToString() {
        return "X: " + xCoordinate.ToString() + " Y: " + yCoordinate.ToString();
    }

}
