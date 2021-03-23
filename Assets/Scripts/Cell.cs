using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    [Header("Coordinates")]
    public int xCoordinate = 0;
    public int zCoordinate = 0;
    public int yCoordinate;
    public GameObject attachedUnit;

    [Header("Cell Bools")]
    public bool isWalkable = true; //movement is impossible, lava, deepwater etc NOT OCCUPIED
    public bool isCurrent;
    public bool isTarget;
    public bool isSelectable;
    public bool isOccupied;
    public bool isBlocked;
    public bool isInAttackRange;
    public bool isInShootRange;
    public bool isCovered;
    //BFS vars
    [Header("BFS Variables")]
    public List<Cell> adjacencyList;
    public bool visited;
    public Cell parent;
    public int distance;
    public bool isFinalDestination;
    public bool inWalkRange;

    public Cell(int x, int z)
    {
        xCoordinate = x;
        zCoordinate = z;
        adjacencyList = new List<Cell>();
        yCoordinate = 1; //default
        isBlocked = false;
    }
    public Cell(int x, int z, int y)
    {
        xCoordinate = x;
        zCoordinate = z;
        adjacencyList = new List<Cell>();
        yCoordinate = y;
        isBlocked = false;
    }

    public void setIsCurrent(bool b)
    {
        isCurrent = b;
    }

    // Start is called before the first frame update
    private void Start()
    {
        ResetBFSVariables();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateCellColor();
    }

    private void OnMouseEnter()
    {
        if (!GameStateManager.isAnyoneMoving && 
            GameStateManager.activeUnit && 
            GameStateManager.activeUnit.GetComponent<TacticsAttributes>().movementSelected)
            
        {
            TacticsMove tm = GameStateManager.activeUnit.GetComponent<TacticsMove>();
            if (!tm.teamBounceCell) {

                tm.MoveToCell(this, false);
            } else {
                tm.DrawBounceLine(transform.position);
            }
            
        }
    }

    public void ResetBFSVariables()
    {

        isCurrent = false;
        isTarget = false;
        isFinalDestination = false;
        isSelectable = false;
        adjacencyList = new List<Cell>();
        adjacencyList.Clear();
        visited = false;
        parent = null;
        distance = 0;
        isInAttackRange = false;
        isInShootRange = false;
    }

    public void FindNeighborsHex(float jumpHeight)
    {
        ResetBFSVariables();

        List<GameObject> temp = new List<GameObject>();
        temp.Add(GameObject.Find(xCoordinate - 1 + "," + zCoordinate)); //left
        temp.Add(GameObject.Find(xCoordinate + 1 + "," + zCoordinate)); //right

        if ((xCoordinate % 2 == 0 && zCoordinate % 2 == 0) ||
            (xCoordinate % 2 == 1 && zCoordinate % 2 == 0)) //double even or odd,even
        {
            temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate + 1))); //top right
            temp.Add(GameObject.Find(xCoordinate - 1 + "," + (zCoordinate + 1))); //top left
            temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate - 1))); //bottom right
            temp.Add(GameObject.Find(xCoordinate - 1 + "," + (zCoordinate - 1))); //bottom left

        } else if ((xCoordinate % 2 == 1 && zCoordinate % 2 == 1) || 
            (xCoordinate % 2 == 0 && zCoordinate % 2 == 1)) //double odd or even,odd
        {
            temp.Add(GameObject.Find(xCoordinate + 1 + "," + (zCoordinate + 1))); //top right
            temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate + 1))); //top left
            temp.Add(GameObject.Find(xCoordinate + 1 + "," + (zCoordinate - 1))); //bottom right
            temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate - 1))); //bottom left
        }
        for (int i = 0; i < temp.Count; i++)
        {

            if (temp[i] != null)
            {
                adjacencyList.Add(temp[i].GetComponent<Cell>());
            }
        }
    }

    public void FindNeighbors(float jumpHeight)
    {
        ResetBFSVariables();

        List<GameObject> temp = new List<GameObject>();
        temp.Add(GameObject.Find(xCoordinate - 1 + "," + zCoordinate)); //left
        temp.Add(GameObject.Find(xCoordinate + 1 + "," + zCoordinate)); //right
        temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate + 1))); //up
        temp.Add(GameObject.Find(xCoordinate + "," + (zCoordinate - 1))); //down

        for (int i = 0; i < temp.Count; i++)
        {

            if (temp[i] != null)
            {
                adjacencyList.Add(temp[i].GetComponent<Cell>());
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
        else if (isTarget || isFinalDestination)
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
        else if(isInAttackRange)
        {
            if (attachedUnit)
            {
                GetComponent<Renderer>().material.color = Color.yellow;
            } else
            {
                isInAttackRange = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
        }
        else if (isInShootRange)
        {
            GetComponent<Renderer>().material.color = Color.cyan;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public override string ToString() {
        return "X: " + xCoordinate.ToString() + " Y: " + zCoordinate.ToString();
    }

    public Cell getNeighbor(string direction)
    {
        if (direction == "right")
        {
            if (xCoordinate < Grid.gameBoard.Count-1)
            {
                return Grid.gameBoard[xCoordinate + 1][zCoordinate];
            }
        }
        if (direction == "left")
        {
            if (xCoordinate > 0)
            {
                return Grid.gameBoard[xCoordinate - 1][zCoordinate];
            }
        }
        if (direction == "up")
        {
            if (zCoordinate < Grid.gameBoard[0].Count - 1)
            {
                return Grid.gameBoard[xCoordinate][zCoordinate + 1];
            }
        }
        if (direction == "down")
        {
            if (zCoordinate > 0)
            {
                return Grid.gameBoard[xCoordinate][zCoordinate - 1];
            }
        }
        return null;
    }

    /*
    public string CellDirection(Cell neighbor)
    {
        if (neighbor.xCoordinate > xCoordinate)
        {
            return "right";
        }
        if (neighbor.xCoordinate < xCoordinate)
        {
            return "left";
        }
        if (neighbor.zCoordinate > zCoordinate)
        {
            return "up";
        } if (neighbor.zCoordinate < zCoordinate)
        {
            return "down";
        }
        return "none";
    } */

    public Cell GetCoverCell(Cell shotFrom)
    {
        if (shotFrom.xCoordinate > xCoordinate + 1)
        {
            if (getNeighbor("right") && getNeighbor("right").isCovered)
            {
                return getNeighbor("right");
            }
        }
        if (shotFrom.xCoordinate < xCoordinate - 1)
        {
            if (getNeighbor("left") && getNeighbor("left").isCovered)
            {
                return getNeighbor("left");
            }
        }
        if (shotFrom.zCoordinate > zCoordinate + 1)
        {
            if (getNeighbor("up") && getNeighbor("up").isCovered)
            {
                return getNeighbor("up");
            }
        }
        if (shotFrom.zCoordinate < zCoordinate - 1)
        {
            if (getNeighbor("down") && getNeighbor("down").isCovered)
            {
                return getNeighbor("down");
            }
        }
        return null;
    }

    public bool isSafeWhenShot(Cell shotFrom)
    {
        if (shotFrom.xCoordinate > xCoordinate + 1) {
            if (getNeighbor("right") && getNeighbor("right").isCovered)
            {
                return true;
            }
        }
        if (shotFrom.xCoordinate < xCoordinate - 1)
        {
            if (getNeighbor("left") && getNeighbor("left").isCovered)
            {
                return true;
            }
        }
        if (shotFrom.zCoordinate > zCoordinate + 1)
        {
            if (getNeighbor("up") && getNeighbor("up").isCovered)
            {
                return true;
            }
        }
        if (shotFrom.zCoordinate < zCoordinate - 1)
        {
            if (getNeighbor("down") && getNeighbor("down").isCovered)
            {
                return true;
            }
        }
        return false;
    }

}
