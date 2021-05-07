using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AirAttacks : MonoBehaviour, AbilityAttributes
{
    [Header("Air Push")]
    public int airPushDistance;
    public bool isListeningForAirPush;
    public Directions pushDirection;
    public GameObject windPrefab;

    [Header("Air Tornado")]
    public int tornadoRange;
    public int tornadoDamage;
    public int tornadoEdgeDamage;
    public GameObject tornadoPrefab;
    bool isListeningForAirTornado;

    [Header("Standard Shot")]
    public GameObject airShotPrefab;
    public int standardShotRange;
    public int standardShotCost;
    public int standardShotDamage;
    public Transform castPoint;
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
        isListeningForAirPush = false;
        isListeningForAirTornado = false;
    }

    // Update is called once per frame
    void Update()
    {
        //print(transform.rotation.y);
        if (isListeningForAirPush)
        {
            ListenForAirPush();
        }
        else if (isListeningForAirTornado)
        {
            ListenForTornado();
        }
    }

    void Reset()
    {
        pushSpeed = 1;
        willFall = false;
        willGoOutOfBounds = false;
        destinationCell = null;
    }

    public int GetStandardShotRange()
    {
        return standardShotRange;
    }

    public int GetStandardShotCost()
    {
        return standardShotCost;
    }

    public int GetBigShotRange()
    {
        return 0;
    }

    public int GetBigShotCost()
    {
        return 0;
    }


    public void DecrementAbilityCooldowns()
    {
        //throw new System.NotImplementedException();
    }

    public void Deselect()
    {
        //throw new System.NotImplementedException();
        isListeningForAirPush = false;
        isListeningForAirTornado = false;
    }

    public int GetShootAbilityRange()
    {
        throw new System.NotImplementedException();
    }

    GameObject tornadoTarget;
    public void PerformShootAbility(GameObject g)
    {
        isListeningForAirTornado = false;
        tornadoTarget = g;
        attributes.anim.SetTrigger("tornado");
    }

    public void PerformTornado()
    {
        StartCoroutine(TornadoCoroutine());
    }

    public void PerformShoot(Cell c, int howManyShots, bool isBigShot)
    {
        StartCoroutine(ShootCorountine(c, howManyShots, isBigShot, standardShotCost));
    }

    IEnumerator ShootCorountine (Cell target, int howManyShots, bool isBigShot, int shotCost)
    {
        yield return attributes.TurnTowardsTarget(target.transform.position);
        attributes.anim.SetTrigger("Attack");
        yield return new WaitForSeconds(.7f);
        attributes.DecrementActionPoints(howManyShots * shotCost);
        for (int i = 0; i < howManyShots; i++)
        {
            GameObject projectile = Instantiate(airShotPrefab, castPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileAttributes>().SetProjectileTarget(target.attachedUnit, attributes.cell);
            projectile.GetComponent<ProjectileAttributes>().damage = standardShotDamage;
            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator TornadoCoroutine()
    {
        Destroy(Instantiate(tornadoPrefab, tornadoTarget.transform.position, Quaternion.identity), 5);
        TacticsAttributes targetAttributes = tornadoTarget.GetComponent<TacticsAttributes>();
        yield return new WaitForSeconds(0.1f);
        targetAttributes.TakeDamage(tornadoDamage, true);
        yield return new WaitForSeconds(0.25f);
        foreach (Cell c in targetAttributes.cell.GetAllNeighbors())
        {
            if (c.yCoordinate == targetAttributes.cell.yCoordinate && c.attachedUnit && c != attributes.cell)
            {
                c.attachedUnit.GetComponent<TacticsAttributes>().TakeDamage(tornadoEdgeDamage, true);
            }
        }
        yield return new WaitForSeconds(2);
        GameStateManager.DeselectAllUnits();
    }

    public void ShowAirTornadoRange()
    {
        GetComponent<PlayerUI>().HideAddtionals();
        Deselect();
        isListeningForAirTornado = true;
        GameStateManager.DeselectAllCells();
        GameStateManager.ComputeAdjList();
        Queue<Cell> process = new Queue<Cell>();
        process.Enqueue(attributes.cell);
        if (attributes.cell)
        {
            attributes.cell.visited = true;
        }
        while (process.Count > 0)
        {
            Cell c = process.Dequeue();
            c.isInAbilityRange = true;
            attributes.selectableCells.Add(c);
            if (c.distance < tornadoRange)
            {
                foreach (Cell cell in c.adjacencyList)
                {
                    if (!cell.visited)
                    {
                        cell.parent = c;
                        cell.visited = true;
                        cell.distance = 1 + c.distance;
                        process.Enqueue(cell);
                    }
                }
            }
        }
    }

    public void ShowDirection(string dir)
    {
        Deselect();
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

        Cell previous;
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
                    } else
                    {
                        q.Peek().isInAbilityRange = false;
                    }
                } else
                {
                    q.Peek().isInAbilityRange = false;
                }
                if (neighbor.yCoordinate == attributes.cell.yCoordinate)
                {
                    neighbor.isInAbilityRange = true;
                }
            }
            previous = q.Peek(); ;
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
        isListeningForAirPush = true;
    }

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
        yield return new WaitForSeconds(.6f);
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
                if (points.Count > 0 && prev.y > points.Peek().y && !animHasTriggered)
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
        yield return new WaitForSeconds(0.5f);
        unit.GetComponent<Animator>().SetTrigger("windStops");
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
                Vector3 final = previous.GetNeighbor(pushDirection).transform.position;
                if (previous.GetNeighbor(pushDirection).yCoordinate > previous.yCoordinate)
                {
                    final = new Vector3(final.x, transform.position.y, final.z);
                }
                vecs.Enqueue(final);
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
            if (hit.collider.GetComponent<TacticsAttributes>())
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

    HashSet<Cell> willBeDamaged = new HashSet<Cell>();
    public void ListenForTornado()
    {
        foreach (Cell c in willBeDamaged)
        {
            c.isInDamageRange = false;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hit.collider.GetComponent<TacticsAttributes>() && hit.collider.tag != tag)
            {
                Cell c = hit.collider.GetComponent<TacticsAttributes>().cell;
                if (Input.GetMouseButtonUp(0))
                {
                    TacticsAttributes ta = hit.collider.GetComponent<TacticsAttributes>();
                    if (c.isInAbilityRange)
                    {
                        PerformShootAbility(hit.collider.gameObject);
                    }
                } else
                {
                    c.isInDamageRange = true;
                    willBeDamaged.Add(c);
                    foreach(Cell cell in c.GetAllNeighbors())
                    {
                        if (cell.yCoordinate == c.yCoordinate && c != attributes.cell)
                        {
                            willBeDamaged.Add(cell);
                            cell.isInDamageRange = true;
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
