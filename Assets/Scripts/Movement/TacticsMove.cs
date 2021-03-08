using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour
{

    

    [Header("Movement Attributes")]
    public float jumpHeight = 2f;
    public float moveSpeed = 2f;
    public int moveDistance = 5;
    public float jumpVelocity = 4.5f;

    [Header("Other")]
    public bool hasMoved = false;
    public bool isMoving = false;
    public GameObject attack;

    Stack<Cell> path = new Stack<Cell>(); //for the path
    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();
    public float halfHeight = 0;
    private Cell originalCell;

    public Cell teamBounceCell;
    public Cell finalDestination; //nice

    float bounceTimeCounter = 0;
    public float bounceTime = 2;

    public float bounceHeight = 1;

    public TacticsAttributes attributes;

    protected void Init()
    {
        attributes = GetComponent<TacticsAttributes>();
        Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent].attachedUnit = transform.gameObject;
        attributes.actionPointsReset = attributes.actionPoints;
        attributes.healthReset = attributes.health;
        halfHeight = GetComponent<Collider>().bounds.extents.y;
        Cell startingPlace = Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent];
        attributes.yPositionCurrent = Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent].yCoordinate;
        originalCell = startingPlace;
        transform.position = startingPlace.transform.position;
        transform.position = new Vector3(
            transform.position.x, 
            transform.position.y + halfHeight + startingPlace.GetComponent<Collider>().bounds.extents.y, //adding halfHeights of unit and cell
            transform.position.z);
    }

    public Cell GetTargetCell(GameObject target)
    {
        RaycastHit hit;
        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1)) {
            return hit.collider.GetComponent<Cell>();
        }
        return null;
    }

    public void ComputeAdjList()
    {
        List<List<Cell>> board = Grid.gameBoard;
        for (int i = 0; i < board.Count; i++) {
            for (int j = 0; j < board[i].Count; j++)
            {
                Cell c = board[i][j].GetComponent<Cell>();
                c.FindNeighbors(jumpHeight);
            }
        }
    }

    public void FindSelectableCells(Cell cellParam)
    {
        Queue<Cell> process = new Queue<Cell>();
        if (teamBounceCell == null)
        {
            ComputeAdjList();
            attributes.GetCurrentCell();
        }

        if (teamBounceCell != null)
        {
            process.Enqueue(teamBounceCell);
            teamBounceCell.visited = true;
        }
        else
        {
            process.Enqueue(cellParam);
            if (cellParam)
            {
                cellParam.visited = true;
            }
        }
        

        while (process.Count > 0)
        {
            Cell c = process.Dequeue();

            if (c && !c.isBlocked)
            {
                if (c.attachedUnit)
                {
                    if (c.attachedUnit.tag == tag)
                    {
                        c.isSelectable = true;
                        attributes.selectableCells.Add(c);
                    }
                } else
                {
                    c.isSelectable = true;
                    attributes.selectableCells.Add(c);
                }
            }
            if (c.distance < moveDistance)
            {
                foreach (Cell cell in c.adjacencyList)
                {
                    
                    if (!cell.visited)
                    {
                        if (cell.attachedUnit != null)
                        {
                            if (cell.attachedUnit.tag == tag)
                            {
                                if (teamBounceCell == null)
                                {
                                    cell.parent = c;
                                }
                                cell.visited = true;
                                cell.distance = 1 + c.distance;
                                process.Enqueue(cell);
                            }
                            cell.visited = true;
                            cell.distance = 1 + c.distance;
                        }
                        else if (!cell.isBlocked)
                        {
                            if (teamBounceCell == null)
                            {
                                cell.parent = c;
                            }
                            cell.visited = true;
                            cell.distance = 1 + c.distance;
                            process.Enqueue(cell);
                        }
                        else if (cell.isBlocked && teamBounceCell != null)
                        {
                            cell.visited = true;
                            cell.distance = 1 + c.distance;
                            process.Enqueue(cell);
                        }
                    }
                }
            }
        }
    }

    public void MoveToCell(Cell c)
    {

        path.Clear();
        attributes.GetCurrentCell();
        attributes.currentCell.attachedUnit = null;
        Cell next = c;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
        c.isTarget = true;
        isMoving = true;
        GameStateManager.isAnyoneMoving = true;
    }

    public void Move()
    {
        Cell lastInPath = null;
        if (path.Count > 0)
        {
            Cell c = path.Peek();
            Vector3 target = c.transform.position;
            //unit's position on top of target tile
            target.y += halfHeight + c.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {

                CalculateHeading(target);
                SetHorizontalVelocity();

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            } else
            {
                //reached goal
                attributes.xPositionCurrent = path.Peek().xCoordinate;
                attributes.zPositionCurrent = path.Peek().zCoordinate;
                lastInPath = path.Pop();
            }
        }
        else 
        {
            Vector3 target = finalDestination.transform.position;
            target.y += halfHeight + finalDestination.GetComponent<Collider>().bounds.extents.y;
            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                Bounce();
            } else
            {
                attributes.RemoveSelectableCells();
                if (teamBounceCell != null)
                {
                    Grid.gameBoard[finalDestination.xCoordinate][finalDestination.zCoordinate]
                        .attachedUnit = transform.gameObject;
                    attributes.xPositionCurrent = finalDestination.xCoordinate;
                    attributes.zPositionCurrent = finalDestination.zCoordinate;
                } else
                {
                    Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent].attachedUnit = transform.gameObject;
                }
                bounceTimeCounter = 0;
                teamBounceCell = null;
                finalDestination = null;
                isMoving = false;
                GameStateManager.isAnyoneMoving = false;
                hasMoved = true;
                attributes.actionPoints--;
                GameStateManager.DeselectAllUnits();
            }
            
        }
    }
    void Bounce()
    {
        bounceTimeCounter += Time.deltaTime;

        bounceTimeCounter = bounceTimeCounter % bounceTime;
        transform.position = MathParabola.Parabola(
            transform.position, finalDestination.transform.position, bounceHeight, bounceTimeCounter / bounceTime);
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    public void ResetPosition()
    {
        transform.position = originalCell.transform.position;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + halfHeight + originalCell.GetComponent<Collider>().bounds.extents.y, //adding halfHeights of unit and cell
            transform.position.z);

        attributes.xPositionCurrent = originalCell.xCoordinate;
        attributes.zPositionCurrent = originalCell.zCoordinate;
    }
    public void ResetAttributes()
    {
        hasMoved = false;
        attributes.isSelected = false;
        attributes.actionPoints = attributes.actionPointsReset;
    }
}
