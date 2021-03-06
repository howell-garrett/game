﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : TacticsAttributes
{

    List<Cell> selectableCells = new List<Cell>();

    [Header("Movement Attributes")]
    public float jumpHeight = 2f;
    public float moveSpeed = 2f;
    public int moveDistance = 5;
    public float jumpVelocity = 4.5f;

    [Header("Other")]
    public bool hasMoved = false;
    public bool isMoving = false;
    public bool isSelected = false;

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

    protected void Init()
    {
        Grid.gameBoard[xPositionCurrent][zPositionCurrent].attachedUnit = transform.gameObject;
        actionPointsReset = actionPoints;
        healthReset = health;
        halfHeight = GetComponent<Collider>().bounds.extents.y;
        Cell startingPlace = Grid.gameBoard[xPositionCurrent][zPositionCurrent];
        yPositionCurrent = Grid.gameBoard[xPositionCurrent][zPositionCurrent].yCoordinate;
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
            Debug.Log("COMPUTE");
            ComputeAdjList();
            GetCurrentCell();
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
                        selectableCells.Add(c);
                    }
                } else
                {
                    c.isSelectable = true;
                    selectableCells.Add(c);
                }
            }
            //Debug.Log(c);
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

    public void FindCellsInAttackRange() {
        GetCurrentCell();
        currentCell.FindNeighbors(jumpHeight);
        List<Cell> l = currentCell.adjacencyList;
        foreach (Cell c in l)
        {
            c.isInAttackRange = true;
        }
    }

    public void MoveToCell(Cell c)
    {

        path.Clear();
        GetCurrentCell();
        currentCell.attachedUnit = null;
        Cell next = c;
        while (next != null)
        {
            path.Push(next);
            Debug.Log(next);
            next = next.parent;
        }
        c.isTarget = true;
        isMoving = true;
    }

    public void Move()
    {
        Cell lastInPath = null;
        if (path.Count > 0)
        {
            Cell c = path.Peek();
            //c.attachedUnit = null;
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
                xPositionCurrent = path.Peek().xCoordinate;
                zPositionCurrent = path.Peek().zCoordinate;
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
                /*
                CalculateHeading(target);
                SetHorizontalVelocity();

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime; */
            } else
            {
                RemoveSelectableCells();
                if (teamBounceCell != null)
                {
                    Grid.gameBoard[finalDestination.xCoordinate][finalDestination.zCoordinate]
                        .attachedUnit = transform.gameObject;
                    xPositionCurrent = finalDestination.xCoordinate;
                    zPositionCurrent = finalDestination.zCoordinate;
                    //teamBounceCell.attachedUnit = transform.gameObject;
                } else
                {
                    Grid.gameBoard[xPositionCurrent][zPositionCurrent].attachedUnit = transform.gameObject;
                }
                bounceTimeCounter = 0;
                teamBounceCell = null;
                finalDestination = null;
                isMoving = false;
                hasMoved = true;
                actionPoints--;
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

    public void Attack(TacticsMove tm)
    {
        tm.health -= attackPower;
        actionPoints--;
        GameStateManager.DeselectAllUnits();
    }

    protected void RemoveSelectableCells()
    {
        if (currentCell != null)
        {
            currentCell.isCurrent = false;
            currentCell = null;
        }
        foreach(Cell cell in selectableCells)
        {
            cell.ResetBFSVariables();
        }

        selectableCells.Clear();
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

        xPositionCurrent = originalCell.xCoordinate;
        zPositionCurrent = originalCell.zCoordinate;
    }
    public void ResetAttributes()
    {
        hasMoved = false;
        isSelected = false;
        actionPoints = actionPointsReset;
    }
}
