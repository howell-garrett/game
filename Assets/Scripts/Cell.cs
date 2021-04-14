using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public GameObject cover;
    public GameObject coverPrefab;

    [Header("Cell Colors")]
    public Material red;
    public Material green;
    public Material cyan;
    public Material yellow;
    public Material grey;
    public Material white;
    public Material magenta;

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
        SpawnCover();
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
            if (tm.teamBounceCells.Count <= 0) {
                tm.MoveToCell(this, false);
            } else if (isSelectable) {
                tm.DrawBounceLine(transform.position, false);
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

    void SpawnCover()
    {
        if (isCovered)
        {
            GameObject coverFab = Instantiate(coverPrefab, transform.position, Quaternion.identity);
            coverFab.transform.position += new Vector3(0, coverFab.GetComponent<Collider>().bounds.extents.y, 0);
            cover = coverFab;
        }
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

    public List<Cell> GetAllNeighbors()
    {
        List<Cell> list = new List<Cell>();
        if (GetNeighbor(Directions.Right))
        {
            list.Add(GetNeighbor(Directions.Right));
        }
        if (GetNeighbor(Directions.Left))
        {
            list.Add(GetNeighbor(Directions.Left));
        }
        if (GetNeighbor(Directions.Up))
        {
            list.Add(GetNeighbor(Directions.Up));
        }
        if (GetNeighbor(Directions.Up))
        {
            list.Add(GetNeighbor(Directions.Up));
        }
        return list;
    }

    public void FindNeighbors(float jumpHeight)
    {
        
        if (xCoordinate < Grid.gameBoardWidth && Grid.gameBoard[xCoordinate+1][zCoordinate])
        {
            adjacencyList.Add(Grid.gameBoard[xCoordinate + 1][zCoordinate]);
        }
        if (xCoordinate > 0 && Grid.gameBoard[xCoordinate - 1][zCoordinate])
        {
            adjacencyList.Add(Grid.gameBoard[xCoordinate - 1][zCoordinate]);
        }
        if (zCoordinate < Grid.gameBoardHeight && Grid.gameBoard[xCoordinate][zCoordinate + 1])
        {
            adjacencyList.Add(Grid.gameBoard[xCoordinate][zCoordinate + 1]);
        }
        if (zCoordinate > 0 && Grid.gameBoard[xCoordinate][zCoordinate - 1])
        {
            adjacencyList.Add(Grid.gameBoard[xCoordinate][zCoordinate - 1]);
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
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material = magenta;
        }
        else if (isTarget || isFinalDestination)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material = green;
        }
        else if (isSelectable)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material = red;
        }
        else if (isBlocked)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material = grey;
        }
        else if(isInAttackRange)
        {
            if (attachedUnit)
            {
                GetComponent<Renderer>().enabled = true;
                GetComponent<Renderer>().material = yellow;
            } else
            {
                isInAttackRange = false;
                GetComponent<Renderer>().enabled = false;
            }
        }
        else if (isInShootRange)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material = cyan;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    public override string ToString() {
        return "X: " + xCoordinate.ToString() + " Y: " + zCoordinate.ToString();
    }

    public Cell GetNeighbor(Directions direction)
    {
        RaycastHit hit;
        if (direction == Directions.Right)
        {
            if (xCoordinate < Grid.gameBoardWidth && Grid.gameBoard[xCoordinate + 1][zCoordinate])
            {
                return Grid.gameBoard[xCoordinate + 1][zCoordinate];
            }
            
            return null;
        }
        if (direction == Directions.Left)
        {
            if (xCoordinate > 0 && Grid.gameBoard[xCoordinate - 1][zCoordinate])
            {
                return Grid.gameBoard[xCoordinate - 1][zCoordinate];
            }
            return null;
        }
        if (direction == Directions.Up)
        {
            if (zCoordinate < Grid.gameBoardHeight && Grid.gameBoard[xCoordinate][zCoordinate + 1])
            {
                return Grid.gameBoard[xCoordinate][zCoordinate + 1];
            }
            return null;
        }
        if (direction == Directions.Down)
        {
            if (zCoordinate > 0 && Grid.gameBoard[xCoordinate][zCoordinate - 1])
            {
                return Grid.gameBoard[xCoordinate][zCoordinate - 1];
            }
            return null;
        }
        return null;
    }

    public Cell GetCoverCell(Cell shotFrom)
    {
        if (shotFrom.xCoordinate > xCoordinate + 1)
        {
            if (GetNeighbor(Directions.Right) && GetNeighbor(Directions.Right).isCovered)
            {
                return GetNeighbor(Directions.Right);
            }
        }
        if (shotFrom.xCoordinate < xCoordinate - 1)
        {
            if (GetNeighbor(Directions.Left) && GetNeighbor(Directions.Left).isCovered)
            {
                return GetNeighbor(Directions.Left);
            }
        }
        if (shotFrom.zCoordinate > zCoordinate + 1)
        {
            if (GetNeighbor(Directions.Up) && GetNeighbor(Directions.Up).isCovered)
            {
                return GetNeighbor(Directions.Up);
            }
        }
        if (shotFrom.zCoordinate < zCoordinate - 1)
        {
            if (GetNeighbor(Directions.Down) && GetNeighbor(Directions.Down).isCovered)
            {
                return GetNeighbor(Directions.Down);
            }
        }
        return null;
    }

    public bool isSafeWhenShot(Cell shotFrom)
    {
        if (shotFrom.xCoordinate > xCoordinate + 1) {
            if (GetNeighbor(Directions.Right) && GetNeighbor(Directions.Right).isCovered)
            {
                return true;
            }
        }
        if (shotFrom.xCoordinate < xCoordinate - 1)
        {
            if (GetNeighbor(Directions.Left) && GetNeighbor(Directions.Left).isCovered)
            {
                return true;
            }
        }
        if (shotFrom.zCoordinate > zCoordinate + 1)
        {
            if (GetNeighbor(Directions.Up) && GetNeighbor(Directions.Up).isCovered)
            {
                return true;
            }
        }
        if (shotFrom.zCoordinate < zCoordinate - 1)
        {
            if (GetNeighbor(Directions.Down) && GetNeighbor(Directions.Down).isCovered)
            {
                return true;
            }
        }
        return false;
    }
}

public enum Directions
{
    Left, Right, Up, Down
}
