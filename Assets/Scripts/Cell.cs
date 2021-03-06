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
    //BFS vars
    [Header("BFS Variables")]
    public List<Cell> adjacencyList;
    public bool visited;
    public Cell parent;
    public int distance;

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

    public void ResetBFSVariables()
    {

        isCurrent = false;
        isTarget = false;
        isSelectable = false;
        adjacencyList = new List<Cell>();

        adjacencyList.Clear();
        visited = false;
        parent = null;
        distance = 0;
        isInAttackRange = false;
    }

    public void FindNeighbors(float jumpHeight)
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
                if (xCoordinate== 1 && zCoordinate == 0 )
                {
                }
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
        else if(isInAttackRange)
        {
            if (attachedUnit || true)
            {
                GetComponent<Renderer>().material.color = Color.yellow;
            } else
            {
                isInAttackRange = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public override string ToString() {
        return "X: " + xCoordinate.ToString() + " Y: " + zCoordinate.ToString();
    }

}
