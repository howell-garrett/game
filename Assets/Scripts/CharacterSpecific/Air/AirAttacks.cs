using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AirAttacks : MonoBehaviour, AbilityAttributes
{
    public int airPushDistance;
    public bool isListeningForAirPush;
    public Directions pushDirection;
    public GameObject windPrefab;
    //for every push
    float pushSpeed;
    bool willGoOutOfBounds;
    bool willFall;
    Cell destinationCell;
    HashSet<TacticsAttributes> hasTurned;
    TacticsAttributes attributes;
    Vector3 vecDir;

    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
        pushSpeed = 1;
        willFall = false;
        hasTurned = new HashSet<TacticsAttributes>();
    }

    // Update is called once per frame
    void Update()
    {
        //print(transform.rotation.y);
        if (isListeningForAirPush)
        {
            ListenForAirPush();
        }
    }

    void Reset()
    {
        pushSpeed = 1;
        willFall = false;
        willGoOutOfBounds = false;
        destinationCell = null;
    }

    public void DecrementAbilityCooldowns()
    {
        //throw new System.NotImplementedException();
    }

    public void Deselect()
    {
        //throw new System.NotImplementedException();
        isListeningForAirPush = false;
    }

    public int GetShootAbilityRange()
    {
        throw new System.NotImplementedException();
    }

    public void PerformShootAbility(GameObject g)
    {
        
    }

    public void ShowDirection(string dir)
    {
        hasTurned.Clear();
        GameStateManager.ResetCellBools();
        if (dir == "up") {
            pushDirection = Directions.Up;
        } else if (dir == "down")
        {
            pushDirection = Directions.Down;
        } else if (dir == "right")
        {
            pushDirection = Directions.Right;
        } else
        {
            pushDirection = Directions.Left;
        }
        attributes.FaceDirection(pushDirection);

        Queue<Cell> q = new Queue<Cell>();
        if (attributes.cell.GetNeighbor(pushDirection))
        {
            q.Enqueue(attributes.cell.GetNeighbor(pushDirection));
        }
        if (pushDirection == Directions.Up || pushDirection == Directions.Down)
        {
            if (q.Peek().GetNeighbor(Directions.Left) && q.Peek().GetNeighbor(Directions.Left).yCoordinate <= attributes.yPositionCurrent)
            {
                q.Enqueue(q.Peek().GetNeighbor(Directions.Left));
            }
            if (q.Peek().GetNeighbor(Directions.Right) && q.Peek().GetNeighbor(Directions.Right).yCoordinate <= attributes.yPositionCurrent)
            {
                q.Enqueue(q.Peek().GetNeighbor(Directions.Right));
            }
        }
        if (pushDirection == Directions.Left || pushDirection == Directions.Right)
        {
            if (q.Peek().GetNeighbor(Directions.Up) && q.Peek().GetNeighbor(Directions.Up).yCoordinate <= attributes.yPositionCurrent)
            {
                q.Enqueue(q.Peek().GetNeighbor(Directions.Up));
            }
            if (q.Peek().GetNeighbor(Directions.Down) && q.Peek().GetNeighbor(Directions.Down).yCoordinate <= attributes.yPositionCurrent)
            {
                q.Enqueue(q.Peek().GetNeighbor(Directions.Down));
            }
        }
        foreach (Cell c in q)
        {
            if (c.yCoordinate == attributes.yPositionCurrent)
            {
                c.isInAbilityRange = true;
            }
        }
        while (q.Count > 0)
        {
            Cell neighbor = q.Peek().GetNeighbor(pushDirection);
            if (neighbor)
            {
                if (neighbor.yCoordinate <= attributes.cell.yCoordinate)
                {
                    if (!neighbor.isCovered)
                    {
                        q.Enqueue(q.Peek().GetNeighbor(pushDirection));
                    }
                }
                if (neighbor.yCoordinate == attributes.cell.yCoordinate)
                {
                    q.Peek().GetNeighbor(pushDirection).isInAbilityRange = true;
                }
            }
            q.Dequeue();
        }

        List<GameObject> unitsBlown = new List<GameObject>();
        foreach (Cell cell in GameStateManager.FindAllCells())
        {
            if (cell.isInAbilityRange && cell.attachedUnit && cell.yCoordinate == attributes.cell.yCoordinate)
            {
                unitsBlown.Add(cell.attachedUnit);
            }
        }
        uwu = unitsBlown;
        isListeningForAirPush = true;
    }
    List<GameObject> uwu;

    void PerformAirAttack(GameObject unit)
    {
        GameStateManager.isAnyoneAttacking = true;
        unit.AddComponent<CollisionListenerAir>();
        unit.GetComponent<CollisionListenerAir>().direction = pushDirection;
        Queue<Vector3> vecs = GetPathPoints(unit.GetComponent<TacticsAttributes>().cell);
        GameStateManager.ResetCellBools();
        GameStateManager.SwapUnitTriggerColliders(true);
        GetComponent<BoxCollider>().isTrigger = false;
        StartCoroutine(BlowUnit(unit, vecs));
    }

    IEnumerator BlowUnit(GameObject unit, Queue<Vector3> points)
    {
        attributes.anim.SetTrigger("AirPush");
        yield return new WaitForSeconds(0.75f);
        GameObject wind = Instantiate(windPrefab, transform);
        wind.transform.position = transform.position - transform.forward + Vector3.up;
        Destroy(wind, 4);
        yield return new WaitForSeconds(0.5f);
        int positionCount = points.Count;
        CollisionListenerAir col = unit.GetComponent<CollisionListenerAir>();
        TacticsAttributes unitTA = unit.GetComponent<TacticsAttributes>();
        Vector3 prev = points.Peek();
        bool animHasTriggered = false;
        unit.GetComponent<Animator>().SetTrigger("blown");
        yield return new WaitForSeconds(.7f);
        while (points.Count > 0)
        {
            if (points.Count <= points.Count / 2 && !willFall)
            {
                pushSpeed -= Time.deltaTime * 3;
            }
            else
            {
                pushSpeed += Time.deltaTime * 3;
            }
            if (Vector3.Distance(unit.transform.position, points.Peek()) >= 0.02f)
            {
                unit.transform.position =
                    Vector3.MoveTowards(unit.transform.position, points.Peek(), Time.deltaTime * pushSpeed);
                if (col.stopMoving)
                {
                    unit.GetComponent<Animator>().SetTrigger("hitSomething");
                    destinationCell = col.destination;
                    break;
                }
                yield return null;
            } else
            {
                points.Dequeue();
                if (prev.y > points.Peek().y & !animHasTriggered)
                {
                    animHasTriggered = true;
                    unit.GetComponent<Animator>().SetTrigger("fall");
                }
            }
        }
        Destroy(unit.GetComponent<CollisionListenerAir>());
        unitTA.cell.attachedUnit = null;
        unitTA.cell = destinationCell;
        destinationCell.attachedUnit = unit;
        unitTA.xPositionCurrent = destinationCell.xCoordinate;
        unitTA.zPositionCurrent = destinationCell.zCoordinate;
        unitTA.yPositionCurrent = destinationCell.yCoordinate;
        if (!willGoOutOfBounds)
        {
            while (unit.transform.position != destinationCell.transform.position)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, destinationCell.transform.position, Time.deltaTime / 2);
                yield return null;
            }
        } else
        {
            unit.transform.position = destinationCell.transform.position;
        }
        
        Reset();
        GameStateManager.isAnyoneAttacking = false;
        GameStateManager.SwapUnitTriggerColliders(false);
        print("arrived");
    }

    Queue<Vector3> GetPathPoints(Cell start)
    {
        Cell previous = start;
        Queue<Vector3> vecs = new Queue<Vector3>();
        vecs.Enqueue(start.transform.position);
        destinationCell = start;
        for (int i = 0; i < airPushDistance; i++)
        {
            if (previous.GetNeighbor(pushDirection) && previous.GetNeighbor(pushDirection).yCoordinate >= previous.yCoordinate) {
                vecs.Enqueue(previous.GetNeighbor(pushDirection).transform.position);
                previous = previous.GetNeighbor(pushDirection);
                destinationCell = previous;
            } else if (previous.GetNeighbor(pushDirection) && previous.GetNeighbor(pushDirection).yCoordinate < previous.yCoordinate)
            {
                willFall = true;
                destinationCell = previous.GetNeighbor(pushDirection).GetNeighbor(pushDirection);
                List<Vector3> bezPoints = 
                    BezierCurveLineRenderer.GetCurvePoints(
                        previous.transform.position + GetDirectionVector(pushDirection)*.5f, // slight overhang on high cell
                        previous.GetNeighbor(pushDirection).GetNeighbor(pushDirection).transform.position, 1); // two cells past
                foreach (Vector3 point in bezPoints)
                {
                    vecs.Enqueue(point);
                }
                break;
            } else if (!previous.GetNeighbor(pushDirection))
            {
                willGoOutOfBounds = true;
                willFall = true;
                List<Vector3> bezPoints = 
                    BezierCurveLineRenderer.GetCurvePoints(
                        previous.transform.position + GetDirectionVector(pushDirection) * .5f, 
                        previous.transform.position + GetDirectionVector(pushDirection) * 2 + Vector3.down*2, 1);
                foreach (Vector3 point in bezPoints)
                {
                    vecs.Enqueue(point);
                }
                break;
            }
        }

        return vecs;
    }

    Vector3 GetDirectionVector(Directions d)
    {
        if (d == Directions.Up)
        {
            return Vector3.forward;
        } if (d == Directions.Down)
        {
            return Vector3.back;
        } if (d == Directions.Left)
        {
            return Vector3.left;
        } else
        {
            return Vector3.right;
        }
    }

    public void ListenForAirPush()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hit.collider.GetComponent<TacticsAttributes>() && hit.collider.tag != tag)
            {
                Cell c = hit.collider.GetComponent<TacticsAttributes>().cell;
                TacticsAttributes ta = hit.collider.GetComponent<TacticsAttributes>();
                if (c.isInAbilityRange)
                {
                    if (c.GetNeighbor(pushDirection) &&
                        (c.GetNeighbor(pushDirection).attachedCover || c.GetNeighbor(pushDirection).attachedUnit || c.GetNeighbor(pushDirection).yCoordinate > c.yCoordinate))
                    {
                    } else
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            PerformAirAttack(hit.collider.gameObject);
                            isListeningForAirPush = false;
                        }
                        if (!hasTurned.Contains(ta))
                        {
                            ta.FaceDirection(GameStateManager.GetOppositeDirection(pushDirection));
                            hasTurned.Add(ta);
                        }
                    }
                }
            }
        }
    }

    public void PerformTeammateAbility(GameObject g)
    {
        throw new System.NotImplementedException();
    }


}
