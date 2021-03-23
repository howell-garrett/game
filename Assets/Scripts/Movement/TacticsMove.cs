using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

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
    public float bounceTime = 2;
    public float halfHeight = 0;
    public float bounceHeight = 1;
    public Cell teamBounceCell;
    public Cell finalDestination; //nice
    public TacticsAttributes attributes;
    public bool checkedSelectableCells = false;


    Stack<Cell> path = new Stack<Cell>(); //for the path
    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();
    Cell originalCell;
    float bounceTimeCounter = 0;
    Animator animator;
    LineRenderer pathRenderer;

    protected void Init()
    {
        pathRenderer = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        attributes = GetComponent<TacticsAttributes>();
        Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent].attachedUnit = transform.gameObject;
        attributes.actionPointsReset = attributes.actionPoints;
        attributes.healthReset = attributes.health;
        Cell startingPlace = Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent];
        attributes.yPositionCurrent = Grid.gameBoard[attributes.xPositionCurrent][attributes.zPositionCurrent].yCoordinate;
        originalCell = startingPlace;
        transform.position = startingPlace.transform.position;
        transform.position = new Vector3(
            transform.position.x, 
            transform.position.y  + startingPlace.GetComponent<Collider>().bounds.extents.y, //adding halfHeights of unit and cell
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


        int availableMoveSpots = moveDistance;
        if (attributes.actionPoints < moveDistance)
        {
            availableMoveSpots = attributes.actionPoints;
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
                        c.inWalkRange = true;
                    }
                } else
                {
                    c.isSelectable = true;
                    c.inWalkRange = true;
                    attributes.selectableCells.Add(c);
                }
            }
            if (!teamBounceCell)
            {

                if (c.distance < availableMoveSpots)
                {
                    foreach (Cell cell in c.adjacencyList)
                    {
                        if (!cell.visited)
                        {
                            if (cell.attachedUnit != null)
                            {
                                if (cell.attachedUnit.tag == tag)
                                {
                                    cell.parent = c;
                                    cell.visited = true;
                                    cell.distance = 1 + c.distance;
                                    process.Enqueue(cell);
                                } else
                                {
                                    cell.visited = true;
                                    cell.distance = 1 + c.distance;
                                }
                            }
                            else if (!cell.isBlocked)
                            {
                                cell.parent = c;
                                cell.visited = true;
                                cell.distance = 1 + c.distance;
                                process.Enqueue(cell);
                            }
                            else if (cell.isBlocked)
                            {
                                cell.visited = true;
                                cell.distance = 1 + c.distance;
                            }
                        }
                    }
                }
            } else
            {
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
                                    cell.visited = true;
                                    cell.distance = 1 + c.distance;
                                    process.Enqueue(cell);
                                }
                                cell.visited = true;
                                cell.distance = 1 + c.distance;
                            }
                            else
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
    }

    public void MoveToCell(Cell c, bool startMoving)
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
        
        Cell[] pathCopy = new Cell[path.Count];
        pathRenderer.positionCount = path.Count;
        pathRenderer.SetPositions(CellsToPositions(path));
        if (teamBounceCell && finalDestination)
        {
            DrawBounceLine(finalDestination.transform.position);
        }
        pathRenderer.enabled = true;
        if (startMoving)
        {

            attributes.actionPoints -= path.Count - 1;
            animator.SetBool("isWalking", true);
            GameStateManager.isAnyoneMoving = true;
            isMoving = true;
            c.isTarget = true;
            //GameStateManager.AllCellsNotSelectable();
        }
    }

    Vector3[] CellsToPositions(Stack<Cell> stack)
    {
        Cell[] cells = stack.ToArray();
        Vector3[] temp = new Vector3[stack.Count];

        int count = 0;
        Vector3 yOffset = new Vector3(0, .1f, 0);
        foreach (Cell item in cells)
        {
            temp[count] = item.transform.position + yOffset;
            count++;
        }
        return temp;
    }

    bool bounceHasTriggered = false;
    public void Move()
    {
        Cell lastInPath = null;
        //Debug.Log(path.Count);
        if (path.Count > 0)
        {
            Cell c = path.Peek();
            Vector3 target = c.transform.position;
            //unit's position on top of target tile
            target.y += c.GetComponent<Collider>().bounds.extents.y;
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
            target.y += finalDestination.GetComponent<Collider>().bounds.extents.y;
            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                if (!bounceHasTriggered)
                {
                    attributes.anim.SetTrigger("bounce");
                    bounceHasTriggered = true;
                }
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
                pathRenderer.enabled = false;
                bounceHasTriggered = false;
                teamBounceCell = null;
                finalDestination = null;
                isMoving = false;
                GameStateManager.isAnyoneMoving = false;
                hasMoved = true;
                animator.SetBool("isWalking", false);
                GameStateManager.DeselectAllUnits();
            }
            
        }
    }
    void Bounce()
    {
        Vector3 c = new Vector3(finalDestination.transform.position.x, transform.position.y, finalDestination.transform.position.z);
        transform.LookAt(c);
        bounceTimeCounter += Time.deltaTime;

        bounceTimeCounter = bounceTimeCounter % bounceTime;
        transform.position = MathParabola.Parabola(
            transform.position, finalDestination.transform.position, bounceHeight, bounceTimeCounter / bounceTime);
    }

    public void DrawBounceLine(Vector3 posn)
    {
        pathRenderer.positionCount = path.Count + 1;
        pathRenderer.SetPosition(path.Count, posn);
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
            transform.position.y + originalCell.GetComponent<Collider>().bounds.extents.y, //adding halfHeights of unit and cell
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

    public void Deselect()
    {
        attributes.isSelected = false;
        attributes.movementSelected = false;
        attributes.attackingSelected = false;
        checkedSelectableCells = false;
        teamBounceCell = null;
        finalDestination = null;
        GetComponent<Highlighter>().constant = false;
        GetComponent<LineRenderer>().enabled = false;

    }
}
