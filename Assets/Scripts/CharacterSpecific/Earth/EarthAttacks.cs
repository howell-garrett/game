using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EarthAttacks : MonoBehaviour, AbilityAttributes
{
    TacticsAttributes attributes;
    [Header("Earthquake variables")]
    public int earthquakeCooldown;
    int earthquakeCooldownCurrent;
    public int earthquakeRange;
    public float earthquakeSpeed;
    public int earthquakeDamage;
    public int numberOfOscillations = 10;
    public float movementDepth = .4f;
    public GameObject dustPrefab;
    public Button earthquakeButton;
    [Header("Push Cover")]
    public int pushCoverCooldown;
    public int pushCoverCooldownCurrent;
    public Button pushCoverButton;
    [Header("Launch Teammate")]
    public int launchCooldown;
    public int launchCooldownCurrent;
    public int launchTeammateRange;
    public Button LaunchTeammateButton;
    public bool isListeningForTeammateLaunch;
    [Header("Shoot")]
    //public int damage;
    public GameObject rockPrefab;
    public GameObject shotPrefab;
    public int standardShotRange;
    public int standardShotCost;
    public int standardShotDamage;
    public int bigShotRange;
    public int bigShotCost;
    public int bigShotDamage;
    public GameObject dustPrefabLarge;
    [Range(0, 1)]
    public float scaleModifier;

    // Start is called before the first frame update
    void Start()
    {
        earthquakeCooldownCurrent = earthquakeCooldown;
        pushCoverCooldownCurrent = pushCoverCooldown;
        launchCooldownCurrent = launchCooldown;
        attributes = GetComponent<TacticsAttributes>();
        isListeningForTeammateLaunch = false;
        bouncePosns = new Queue<Vector3>();
    }

    private void Update()
    {
        if (isListeningForTeammateLaunch)
        {
            ListenForTeammateLaunch();
        }
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
        return bigShotRange;
    }

    public int GetBigShotCost()
    {
        return bigShotCost;
    }


    public void DecrementAbilityCooldowns()
    {
        if (earthquakeCooldownCurrent > 0)
        {
            earthquakeCooldown--;
        } else
        {
            earthquakeButton.interactable = true;
        }

        if (pushCoverCooldownCurrent > 0)
        {
            pushCoverCooldownCurrent--;
        }
        else
        {
            //pushCoverButton.interactable = true;
        }

        if (launchCooldownCurrent > 0)
        {
            launchCooldownCurrent--;
        }
        else
        {
            LaunchTeammateButton.interactable = true;
        }
    }
    public int GetShootAbilityRange()
    {
        return 1;
    }
    public void PerformShootAbility(GameObject g)
    {

    }

    public void TriggerEarthquakeAnim()
    {
        attributes.anim.SetTrigger("earthquake");
        print("triggered");
    }

    public void PerformEarthquake ()
    {
        GameStateManager.DeselectAllCells();
        GameStateManager.DeselectAllUnits();
        earthquakeButton.interactable = false;
        GameStateManager.isAnyoneAttacking = true;
        GameStateManager.DeselectAllCells();
        earthquakeCooldownCurrent = earthquakeCooldown;
        StartCoroutine(PerformEarthquakeCoroutine());
    }

    public IEnumerator PerformEarthquakeCoroutine()
    {
        Cell current = GetComponent<TacticsAttributes>().cell;
        HashSet<Cell> cells = new HashSet<Cell>();

        cells.Add(current);
        for (int i = 0; i < earthquakeRange; i++)
        {
            List<Cell> cellRing = new List<Cell>();
            foreach (Cell c in cells)
            {
                List<Cell> neighbors = c.GetAllNeighbors();
                foreach (Cell neighbor in neighbors)
                {
                    if (!cells.Contains(neighbor))
                    {
                        cellRing.Add(neighbor);
                    }
                }
            }
            HashSet<Cell> cellRingNoDups = new HashSet<Cell>(cellRing);
            StartCoroutine(PerformEarthquakeEffect(cellRingNoDups));
            yield return new WaitForSeconds(.15f);
            foreach (Cell c in cellRing)
            {
                cells.Add(c);
            }
        }
    }


    IEnumerator PerformEarthquakeEffect(HashSet<Cell> c)
    {
        int current = 0;
        GameObject parent = new GameObject();
        parent.name = "CellRing";
        foreach (Cell cell in c)
        {
            cell.transform.SetParent(parent.transform);
            if (cell.attachedUnit)
            {
                cell.attachedUnit.transform.SetParent(parent.transform);
                cell.attachedUnit.GetComponent<TacticsAttributes>().TakeDamage(earthquakeDamage, true);
            }

            GameObject dust = Instantiate(dustPrefab, cell.transform.position, Quaternion.identity);
            dust.transform.position += Vector3.up * movementDepth / 2;
            Destroy(dust, 3f);
        }
        Vector3 ystart = parent.transform.position;
        float yEnd = parent.transform.position.y + movementDepth;
        while (current < numberOfOscillations)
        {
            current++;
            while (parent.transform.position.y < yEnd - .05f)
            {
                parent.transform.position =
                    Vector3.MoveTowards(parent.transform.position,
                    new Vector3(parent.transform.position.x, yEnd, parent.transform.position.z),
                    Time.deltaTime * earthquakeSpeed);
                yield return null;
            }
            //hasPeaked = true;
            while (parent.transform.position.y > ystart.y + 0.02f)
            {
                parent.transform.position =
                    Vector3.MoveTowards(parent.transform.position,
                    new Vector3(parent.transform.position.x, ystart.y, parent.transform.position.z),
                    Time.deltaTime * earthquakeSpeed);
                yield return null;
            }
        }

        GameStateManager.isAnyoneAttacking = false;
        parent.transform.position = ystart;
        GameObject grid = GameObject.FindGameObjectWithTag("Grid");
        foreach (Cell cell in c)
        {
            cell.transform.SetParent(grid.transform);
            if (cell.attachedUnit)
            {
                cell.attachedUnit.transform.SetParent(null);
            }
        }
        Destroy(parent);
    }

    public void PerformShoot(Cell c, int howManyShots, bool isBigShot)
    {
        if (!isBigShot)
        {
            SmallShoot(c.attachedUnit, howManyShots);
        } else
        {
            BigShot(c.attachedUnit);
        }

    }

    public void SmallShoot(GameObject target, int howManyShots)
    {
        if (standardShotCost*howManyShots > attributes.actionPoints)
        {
            print("Not Enough AP");
            return;
        }
        GameStateManager.isAnyoneAttacking = true;
        StartCoroutine(SmallShootCoroutine(target, howManyShots));
    }

    public void BigShot(GameObject target)
    {
        if (bigShotCost > attributes.actionPoints)
        {
            print("Not Enough AP");
            return;
        }
        GameStateManager.isAnyoneAttacking = true;
        StartCoroutine(BigShootCoroutine(target));
    }

    public IEnumerator SmallShootCoroutine(GameObject target, int shotCount)
    {
        yield return attributes.TurnTowardsTarget(target.transform.position);
        attributes.DecrementActionPoints(shotCount * standardShotCost);
        GameObject rock = Instantiate(rockPrefab, transform.position + Vector3.down/1.5f + (transform.forward/2), Quaternion.identity);
        rock.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);
        rock.transform.LookAt(new Vector3(target.transform.position.x, rock.transform.position.y, target.transform.position.z));
        Vector3 rockTarget = new Vector3(rock.transform.position.x, transform.position.y, rock.transform.position.z) + Vector3.up;
        Transform effectSpawnPoint = rock.transform.Find("EffectSpawnPoint");
        int count = 0;
        while (Vector3.Distance(rock.transform.position, rockTarget) > 0.05f)
        {
            count++;
            if (count == 35) //roughly when emerging from ground
            {
                Destroy(Instantiate(dustPrefabLarge, effectSpawnPoint.position, Quaternion.identity), 2f);
            }
            rock.transform.position = Vector3.Lerp(rock.transform.position, rockTarget, Time.deltaTime * 2);
            yield return null;
        }
        Transform spawnPoint = rock.transform.Find("BulletSpawnPoint");
        for (int i = 0; i < shotCount; i++)
        {
            GameObject projectile = Instantiate(shotPrefab, spawnPoint.position, Quaternion.identity);
            Destroy(Instantiate(dustPrefabLarge, spawnPoint.position, Quaternion.identity), 2f);
            projectile.GetComponent<ProjectileAttributes>().SetProjectileTarget(target, attributes.cell);
            projectile.GetComponent<ProjectileAttributes>().damage = standardShotDamage;
            rock.transform.localScale -= new Vector3(1f/shotCount * scaleModifier, 1f / shotCount * scaleModifier, 1f / shotCount * scaleModifier);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(rock);
        GameStateManager.isAnyoneAttacking = false;
    }

    public IEnumerator BigShootCoroutine(GameObject target)
    {

        yield return attributes.TurnTowardsTarget(target.transform.position);
        attributes.DecrementActionPoints(bigShotCost);
        GameObject rock = Instantiate(rockPrefab, transform.position + Vector3.down / 1.5f + (transform.forward / 2), Quaternion.identity);
        ProjectileAttributes rockProjectile = rock.GetComponent<ProjectileAttributes>();
        rockProjectile.damage = bigShotDamage;
        rockProjectile.enabled = false;
        rock.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);
        rock.transform.LookAt(new Vector3(target.transform.position.x, rock.transform.position.y, target.transform.position.z));
        Vector3 rockTarget = new Vector3(rock.transform.position.x, transform.position.y, rock.transform.position.z) + Vector3.up;
        Transform effectSpawnPoint = rock.transform.Find("EffectSpawnPoint");
        int count = 0;
        while (Vector3.Distance(rock.transform.position, rockTarget) > 0.05f)
        {
            count++;
            if (count == 35) //roughly when emerging from ground
            {
                Destroy(Instantiate(dustPrefabLarge, effectSpawnPoint.position, Quaternion.identity), 2f);
            }
            rock.transform.position = Vector3.Lerp(rock.transform.position, rockTarget, Time.deltaTime * 2);
            yield return null;
        }
        rockProjectile.GetComponent<ProjectileAttributes>().SetProjectileTarget(target, attributes.cell);
        rockProjectile.bounceHeight = 2.5f;
        rockProjectile.hitDistance = 0.5f;
        rockProjectile.enabled = true;
    }

    public void SelectLaunchTeammate()
    {
        GameStateManager.DeselectAllCells();
        attributes.Deselect();
        GameStateManager.Select(attributes);
        FindSelectableCells(attributes.cell);
        isListeningForTeammateLaunch = true;
    }

    public void FindSelectableCells(Cell cellParam)
    {
        GameStateManager.ResetCellBools();
        GameStateManager.ComputeAdjList();
        Queue<Cell> process = new Queue<Cell>();
        process.Enqueue(cellParam);
        if (cellParam)
        {
            cellParam.visited = true;
        }
        while (process.Count > 0)
        {
            Cell c = process.Dequeue();

            c.isInAbilityRange = true;
            attributes.selectableCells.Add(c);

            if (c.distance < launchTeammateRange)
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

    public void PerformTeammateAbility(GameObject teammate)
    {
        GameStateManager.ResetCellInfoWithoutParent();
        GameStateManager.ResetCellBools();
        GameStateManager.activeLaunchUnit = teammate;
        LineRenderer lr = teammate.GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lr.SetPosition(0, teammate.transform.position);
        lr.enabled = true;
        TacticsMove teammateTM = teammate.GetComponent<TacticsMove>();
        TacticsAttributes teammateAttributes = teammate.GetComponent<TacticsAttributes>();
        teammateTM.teamBounceCells.Add(teammateAttributes.cell);
        teammateTM.FindSelectableCells(teammateAttributes.cell);
    }

    Queue<Vector3> bouncePosns;
    public IEnumerator LaunchCoroutine(GameObject teammate, Cell destination)
    {
        yield return attributes.TurnTowardsTarget(teammate.transform.position);
        LaunchTeammateButton.interactable = false;
        launchCooldownCurrent = launchCooldown;
        GameStateManager.isAnyoneMoving = true;
        GameStateManager.DeselectAllCells();
        Vector3 c = new Vector3(destination.transform.position.x, teammate.transform.position.y, destination.transform.position.z);
        teammate.transform.LookAt(c);
        if (bouncePosns.Count == 0)
        {
            bouncePosns = new Queue<Vector3>();
            List<Vector3> bezPoints = BezierCurveLineRenderer.GetCurvePoints(teammate.transform.position, destination.transform.position, 5);
            foreach (Vector3 vec in bezPoints)
            {
                bouncePosns.Enqueue(vec);
            }
        }

        Cell start = teammate.GetComponent<TacticsAttributes>().cell;
        start.attachedUnit = null;
        float yEndDown = start.yCoordinate - .4f;
        float yEndUp = start.yCoordinate + .4f;
        float yStart = start.yCoordinate;

        teammate.transform.SetParent(start.transform);
        while (start.transform.position.y > yEndDown + .05f)
        {
            start.transform.position =
                Vector3.MoveTowards(start.transform.position,
                new Vector3(start.transform.position.x, yEndDown, start.transform.position.z),
                Time.deltaTime * earthquakeSpeed/4);
            yield return null;
        }
        //hasPeaked = true;
        while (start.transform.position.y < yEndUp - 0.05f)
        {
            start.transform.position =
                Vector3.MoveTowards(start.transform.position,
                new Vector3(start.transform.position.x, yEndUp, start.transform.position.z),
                Time.deltaTime * earthquakeSpeed);
            yield return null;
        }
        teammate.transform.SetParent(null);
        teammate.GetComponent<Animator>().SetBool("isBouncing", true);
        teammate.GetComponent<LineRenderer>().enabled = false;
        GameStateManager.activeLaunchUnit = null;
        int count = 0;
        while (bouncePosns.Count > 0)
        {
            count++;
            teammate.transform.position = Vector3.MoveTowards(teammate.transform.position, bouncePosns.Peek(), Time.deltaTime * 4.5f);
            if (Vector3.Distance(start.transform.position, new Vector3(start.transform.position.x, yStart, start.transform.position.z)) > 0)
            {
                start.transform.position =
                Vector3.MoveTowards(start.transform.position,
                new Vector3(start.transform.position.x, yStart, start.transform.position.z),
                Time.deltaTime * earthquakeSpeed / 4);
            }
            if (Vector3.Distance(bouncePosns.Peek(), teammate.transform.position) <= .05)
            {
                bouncePosns.Dequeue();
            }
            yield return null;
        }
        teammate.GetComponent<TacticsAttributes>().cell = destination;
        teammate.GetComponent<Animator>().SetBool("isBouncing", false);
        GameStateManager.isAnyoneMoving = false;
        GameStateManager.DeselectAllUnits();
    }

    public void ListenForTeammateLaunch()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                print(hit.collider.tag);
                if (hit.collider.CompareTag("Cell"))
                {
                    Cell c = hit.collider.GetComponent<Cell>();
                    if (c.isSelectable && !c.attachedUnit)
                    {
                        StartCoroutine(LaunchCoroutine(GameStateManager.activeLaunchUnit, c));
                    }
                }
                else if (hit.collider.CompareTag(tag))
                {
                    PerformTeammateAbility(hit.collider.gameObject);
                }
            } 
        }
    }

    public void Deselect()
    {
        isListeningForTeammateLaunch = false;
    }

}
