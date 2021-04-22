using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public List<Cell> teamBounceCells;
    public Cell finalDestination; //nice
    public TacticsAttributes attributes;
    public bool checkedSelectableCells = false;
    public Text movementCostUI;


    Stack<Cell> path = new Stack<Cell>(); //for the path
    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();
    Cell originalCell;
    float bounceTimeCounter = 0;
    Animator animator;
    LineRenderer pathRenderer;

    protected void Init()
    {
        teamBounceCells = new List<Cell>();
        pathRenderer = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        attributes = GetComponent<TacticsAttributes>();
        attributes.actionPointsReset = attributes.actionPoints;
        attributes.healthReset = attributes.health;
        attributes.cell.attachedUnit = transform.gameObject;
        Cell startingPlace = attributes.cell;
        attributes.yPositionCurrent = attributes.cell.yCoordinate;
        originalCell = startingPlace;
        transform.position = startingPlace.transform.position;
        transform.position += new Vector3(0, startingPlace.GetComponent<Collider>().bounds.extents.y,0);
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
        Cell[] cells = GameStateManager.FindAllCells();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].FindNeighbors(jumpHeight);
        }
    }

    public void FindSelectableCells(Cell cellParam)
    {
        Queue<Cell> process = new Queue<Cell>();
        if (teamBounceCells.Count <= 0)
        {
            ComputeAdjList();
            attributes.GetCurrentCell();
        }

        //Debug.Log(cellParam);
            process.Enqueue(cellParam);
            if (cellParam)
            {
                cellParam.visited = true;
            }
        


        int availableMoveSpots = moveDistance;
        if (attributes.actionPoints < moveDistance)
        {
            availableMoveSpots = attributes.actionPoints;
        }
        int lowestY = 100;
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
            if (teamBounceCells.Count <= 0)
            {
                if (c && c.distance < availableMoveSpots)
                {
                    foreach (Cell cell in c.adjacencyList)
                    {
                        if (!cell.visited)
                        {
                            if (c.yCoordinate < lowestY) { lowestY = c.yCoordinate; }
                            if ((cell.yCoordinate <= lowestY || attributes.canClimb))
                            {
                                if (cell.parent)
                                {
                                    print(cell.parent);
                                }
                                if (cell.attachedUnit != null)
                                {
                                    if (cell.attachedUnit.tag == tag)
                                    {
                                        cell.parent = c;
                                        cell.visited = true;
                                        cell.distance = 1 + c.distance;
                                        process.Enqueue(cell);
                                    }
                                    else
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
                }
            } else
            {
               // Debug.Log(11111);
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

    Vector3 finalDesinationTarget;
    public void MoveToCell(Cell c, bool startMoving)
    {
       
        path.Clear();
        attributes.GetCurrentCell();
        Cell next = c;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
        Cell[] pathCopy = new Cell[path.Count];

        if (teamBounceCells.Count > 0 && finalDestination)
        {
            //DrawBounceLine(finalDestination.transform.position, true);
        }
        pathRenderer.enabled = true;
        if (startMoving)
        {
            if (teamBounceCells.Count > 0)
            {
                teamBounceCells.RemoveAt(0);
            }
            attributes.currentCell.attachedUnit = null;
            attributes.actionPoints -= path.Count - 1;
            animator.SetBool("isWalking", true);
            GameStateManager.isAnyoneMoving = true;
            isMoving = true;
            c.isTarget = true;
            finalDesinationTarget = finalDestination.transform.position;
            finalDesinationTarget.y += finalDestination.GetComponent<Collider>().bounds.extents.y;
            return;
        }
        pathRenderer.positionCount = path.Count;
        pathRenderer.SetPositions(CellsToPositions(path));
        movementCostUI.text = "Cost: " + (pathRenderer.positionCount - 1);
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
    Cell lastInPath = null;
    public void Move()
    {
        
        if (path.Count > 0)
        {
            Cell c = path.Peek();
            Vector3 target = c.transform.position;
            //unit's position on top of target tile
            target.y += c.GetComponent<Collider>().bounds.extents.y;
            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                if (lastInPath && lastInPath.yCoordinate != c.yCoordinate)
                {
                    Bounce(target);
                    return;
                }
                bounceTimeCounter = 0;

                CalculateHeading(target);
                SetHorizontalVelocity();

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            } else
            {
                //Debug.Log();
                //reached goal
                attributes.xPositionCurrent = path.Peek().xCoordinate;
                attributes.zPositionCurrent = path.Peek().zCoordinate;
                lastInPath = path.Pop();
            }
        }
        else 
        {
            if (teamBounceCells.Count > 0)
            {
                Vector3 target;
                target = teamBounceCells[0].transform.position;
                target.y += teamBounceCells[0].GetComponent<Collider>().bounds.extents.y;
                
                if (Vector3.Distance(transform.position, target) >= 0.05f)
                {
                    if (!bounceHasTriggered)
                    {
                        attributes.anim.SetTrigger("bounce");
                        bounceHasTriggered = true;
                    }
                    Bounce(target);
                } else
                {
                    bounceTimeCounter = 0;
                    bounceHasTriggered = false;
                    teamBounceCells.RemoveAt(0);
                }
                
            }
            else if (Vector3.Distance(transform.position, finalDesinationTarget) >= 0.05f)
            {
                if (!bounceHasTriggered)
                {
                    attributes.anim.SetTrigger("bounce");
                    bounceHasTriggered = true;
                }
                Bounce(finalDesinationTarget);
            }
            else
            {
                attributes.RemoveSelectableCells();
                finalDestination.attachedUnit = transform.gameObject;
                attributes.xPositionCurrent = finalDestination.xCoordinate;
                attributes.zPositionCurrent = finalDestination.zCoordinate;
                attributes.yPositionCurrent = finalDestination.yCoordinate;
                attributes.cell = finalDestination;
                bounceTimeCounter = 0;
                pathRenderer.enabled = false;
                bounceHasTriggered = false;
                teamBounceCells.Clear();
                finalDestination = null;
                isMoving = false;
                GameStateManager.isAnyoneMoving = false;
                hasMoved = true;
                animator.SetBool("isWalking", false);
                GameStateManager.DeselectAllUnits();
            }
            
        }
    }
    void Bounce(Vector3 target)
    {
        Vector3 c = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(c);
        bounceTimeCounter += Time.deltaTime;

        bounceTimeCounter = bounceTimeCounter % bounceTime;
        transform.position = MathParabola.Parabola(
            transform.position, target, bounceHeight, bounceTimeCounter / bounceTime);
    }

    public void DrawBounceLine(Vector3 posn, bool addPoint)
    {
       
        if (addPoint)
        {
            pathRenderer.positionCount = pathRenderer.positionCount + 1;
        }
        pathRenderer.SetPosition(pathRenderer.positionCount - 1, posn); 

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
        teamBounceCells.Clear();
        finalDestination = null;
        GetComponent<Highlighter>().constant = false;
        GetComponent<LineRenderer>().enabled = false;
        movementCostUI.gameObject.SetActive(false);

    }
}
