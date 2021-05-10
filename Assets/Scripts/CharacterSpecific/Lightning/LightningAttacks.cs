using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LightningAttacks : MonoBehaviour, AbilityAttributes
{
    TacticsAttributes attributes;
    [Header("Attract Enemies")]
    public int attractEnemiesRange;
    public int attractedEnemyMoveRange;
    public int attractCost;
    public int attractCooldown;
    int attractCooldownCurrent;
    public float attractedEnemyMoveSpeed;
    public Button attractEnemiesButton;
    public GameObject attractPrefab;
    public Transform castPoint;

    [Header("Lightning Strike")]
    public bool isListeningForLightningStrike;
    public int lightningStrikeRange;
    public int lightningStrikeDamage;
    public int lightningStrikeCost;
    public int lightningStrikeCooldown;
    int lightningStrikeCooldownCurrent;
    public GameObject lightningStrikePrefab;
    public GameObject lightningHandsPrefab;
    public GameObject cloudPrefab;
    public Button lightningStrikeButton;
    GameObject lightningHands;

    [Header("Standard Shot")]
    public GameObject lightningShotPrefab;
    public int standardShotRange;
    public int standardShotCost;
    public int standardShotDamage;
    public GameObject bigShotPrefab;
    public int bigShotRange;
    public int bigShotCost;
    public int bigShotDamage;


    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
        attractCooldownCurrent = 0;
        lightningStrikeCooldownCurrent = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isListeningForLightningStrike)
        {
            ListenForLightningStrike();
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
        if (attractCooldownCurrent > 0)
        {
            attractCooldownCurrent--;
        } else
        {
            attractEnemiesButton.interactable = true;
        }
        if (lightningStrikeCooldownCurrent > 0)
        {
            lightningStrikeCooldownCurrent--;
        } else
        {
            lightningStrikeButton.interactable = true;
        }
    }

    public void Deselect()
    {
        lightningStrikeEnemy = null;
    }

    public int GetShootAbilityRange()
    {
        return 0;
    }

    public void PerformShoot(Cell c, int howManyShots, bool isBigShot)
    {
        int shotCost = standardShotCost;
        if (isBigShot)
        {
            shotCost = bigShotCost;
        }
        if (howManyShots * shotCost > attributes.actionPoints)
        {
            print("Not enough AP");
            return;
        }
        StartCoroutine(ShootCorountine(c, howManyShots, isBigShot, shotCost));
    }

    IEnumerator ShootCorountine(Cell target, int howManyShots, bool isBigShot, int shotCost)
    {
        yield return attributes.TurnTowardsTarget(target.transform.position);
        Directions stepDir = attributes.GetSideStepDirection(target);
        if (stepDir != Directions.Up) //up means no step
        {
            yield return attributes.SideStep(stepDir);
        }
        attributes.anim.SetTrigger("Attack");
        GameObject shotToFire = lightningShotPrefab;
        int shotDamage = standardShotDamage;
        if (isBigShot)
        {
            shotToFire = bigShotPrefab;
            shotDamage = bigShotDamage;
        }
        attributes.DecrementActionPoints(howManyShots * shotCost);
        yield return new WaitForSeconds(.7f);
        for (int i = 0; i < howManyShots; i++)
        {
            GameObject projectile = Instantiate(shotToFire, castPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileAttributes>().SetProjectileTarget(target.attachedUnit, attributes.cell);
            projectile.GetComponent<ProjectileAttributes>().damage = shotDamage;
            yield return new WaitForSeconds(.1f);
        }
        if (stepDir != Directions.Up) //up means no step
        {
            yield return attributes.SideStep(GameStateManager.GetOppositeDirection(stepDir));
        }
    }

    public void LightningStrikeRange()
    {
        GetComponent<PlayerUI>().HideAddtionals();
        Deselect();
        isListeningForLightningStrike = true;
        lightningStrikeCooldownCurrent = lightningStrikeCooldown;
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
            if (c.distance < lightningStrikeRange)
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

    void InstantiateLightningHands()
    {
        lightningHands = Instantiate(lightningHandsPrefab, castPoint);
        lightningHands.transform.position = castPoint.transform.position;
        lightningHands.transform.localScale *= 0.015f;
    }
    GameObject lightningStrikeEnemy;
    void CallDownLightningStrike()
    {
        StartCoroutine(EndStrikeCoroutine());
    }

    IEnumerator EndStrikeCoroutine()
    {
        GameObject strike = Instantiate(lightningStrikePrefab, lightningStrikeEnemy.transform.position, Quaternion.identity);
        strike.transform.eulerAngles = new Vector3(270, 0, 0);
        yield return new WaitForSeconds(0.1f);
        lightningStrikeEnemy.GetComponent<TacticsAttributes>().TakeDamage(lightningStrikeDamage, true);
        Destroy(strike, 2f);
        Destroy(lightningHands, 0.1f);
        yield return new WaitForSeconds(.2f);
        attributes.DecrementActionPoints(lightningStrikeCost);
        GameStateManager.isAnyoneAttacking = false;
    }

    public void PerformShootAbility(GameObject enemy)
    {
        StartCoroutine(PerformLightningStrike(enemy));
    }

    IEnumerator PerformLightningStrike(GameObject enemy)
    {
        GameStateManager.isAnyoneAttacking = true;
        GameStateManager.DeselectAllUnits();
        lightningStrikeButton.interactable = false;
        yield return StartCoroutine(attributes.TurnTowardsTarget(enemy.transform.position));
        attributes.anim.SetTrigger("LightningStrike");
        yield return new WaitForSeconds(0.2f );
        GameObject cloud = Instantiate(cloudPrefab, enemy.transform.position, Quaternion.identity);
        lightningStrikeEnemy = enemy;
        Destroy(cloud, 6);
    }

    public void PerformTeammateAbility(GameObject g)
    {

    }

    public void AttractEnemies()
    {
        attributes.anim.SetTrigger("attract");
    }

    public void AttractEnemiesRange()
    {
        GetComponent<PlayerUI>().HideAddtionals();
        GameStateManager.DeselectAllCells();
        GameStateManager.ComputeAdjList();
        Queue<Cell> process = new Queue<Cell>();
        List<Cell> cellList = new List<Cell>();
        process.Enqueue(attributes.cell);
        List<GameObject> enemies = new List<GameObject>();
        if (attributes.cell)
        {
            attributes.cell.visited = true;
        }
        while (process.Count > 0)
        {
            Cell c = process.Dequeue();
            c.isInAbilityRange = true;
            if (c.distance < attractEnemiesRange)
            {
                foreach (Cell cell in c.adjacencyList)
                {
                    if (!cell.visited)
                    {
                        cell.distance = 1 + c.distance;
                        cellList.Add(c);
                        cell.visited = true;
                        process.Enqueue(cell);
                    }
                }
            }
        }
        foreach (Cell c in cellList)
        {
            c.visited = false;
            c.distance = 0;
        }

    }

    GameObject attractEffect;
    public void AttractEffect()
    {
        attractEffect = Instantiate(attractPrefab, castPoint.transform.position, Quaternion.identity);
        attractEffect.transform.eulerAngles = new Vector3(270, 0, 0);
        StartCoroutine(AttractEnemiesCoroutine());
    }
    public void DestroyAttractEffect()
    {
        Destroy(attractEffect);
    }

    List<GameObject> GetEnemiesInAttractRange()
    {
        GameStateManager.DeselectAllCells();
        GameStateManager.ComputeAdjList();
        Queue<Cell> process = new Queue<Cell>();
        process.Enqueue(attributes.cell);
        List<GameObject> enemies = new List<GameObject>();
        if (attributes.cell)
        {
            attributes.cell.visited = true;
        }
        while (process.Count > 0)
        {
            Cell c = process.Dequeue();
            attributes.selectableCells.Add(c);
            if (c.distance < attractEnemiesRange)
            {
                foreach (Cell cell in c.adjacencyList)
                {
                    if (!cell.visited)
                    {
                        cell.visited = true;
                        cell.distance = 1 + c.distance;
                        process.Enqueue(cell);
                        if (cell.attachedUnit && cell.attachedUnit.tag != tag)
                        {
                            enemies.Add(cell.attachedUnit);
                        }
                    }
                }
            }
        }
        return enemies;
    }

    IEnumerator AttractEnemiesCoroutine()
    {
        GameStateManager.isAnyoneAttacking = true;
        GameStateManager.ResetCellBools();
        GameStateManager.DeselectAllUnits();
        List<GameObject> enemies = GetEnemiesInAttractRange();
        attractCooldownCurrent = attractCooldown;
        foreach(GameObject enemy in enemies)
        {
            TacticsAttributes enemyAtt = enemy.GetComponent<TacticsAttributes>();
            enemyAtt.cell.attachedUnit = null;
            GameStateManager.DeselectAllCells();
            GameStateManager.ComputeAdjList();
            Queue<Cell> process = new Queue<Cell>();
            process.Enqueue(enemyAtt.cell);
            if (enemyAtt.cell)
            {
                enemyAtt.cell.visited = true;
                enemyAtt.cell.attachedUnit = null;
            }
            while (process.Count > 0)
            {
                Cell c = process.Dequeue();
                enemyAtt.selectableCells.Add(c);
                if (c.distance < attractEnemiesRange)
                {
                    foreach (Cell cell in c.adjacencyList)
                    {
                        if (!cell.visited && !cell.isBlocked && c.yCoordinate <= enemyAtt.cell.yCoordinate && 
                            (cell.attachedUnit == null || cell.attachedUnit == gameObject))
                        {
                            cell.parent = c;
                            cell.visited = true;
                            cell.distance = 1 + c.distance;
                            process.Enqueue(cell);
                        }
                    }
                }
            }

            Stack<Cell> path = new Stack<Cell>();
            Cell next = attributes.cell;

            while (next != null)
            {
                path.Push(next);
                next = next.parent;
            }
            Stack<Cell> temp = new Stack<Cell>();
            int count = 0;
            bool yHasDropped = false;
            foreach (Cell c in path)
            {
                if (count <= attractedEnemyMoveRange && count < path.Count - 1 && !c.beingUsed)
                {
                    if (c.yCoordinate == enemyAtt.cell.yCoordinate && !c.attachedUnit)
                    {
                        if (!yHasDropped)
                        {
                            temp.Push(c);
                        }
                    } else
                    {
                        yHasDropped = true;
                    }
                }
                count++;
            }
            print(temp.Count);
            path.Clear();
            bool targetMarked = false;
            foreach (Cell c in temp)
            {
                if (!targetMarked) // mark target as being used
                {
                    targetMarked = true;
                    c.beingUsed = true;
                }
                path.Push(c);
            }
            StartCoroutine(MoveEnemy(enemy, path));
        }
        yield return new WaitForSeconds(4.5f);
        attributes.anim.SetTrigger("attract");
        GameStateManager.isAnyoneAttacking = false;
        ResetBeingUsed();
        attributes.DecrementActionPoints(attractCost);
    }


    IEnumerator MoveEnemy(GameObject enemy, Stack<Cell> path)
    {
        Cell lastInPath = null;
        TacticsMove tm = enemy.GetComponent<TacticsMove>();
        TacticsAttributes attr = enemy.GetComponent<TacticsAttributes>();
        yield return new WaitForSeconds(Random.Range(0,0.75f));
        attr.anim.SetBool("AttractWalk", true);
        Vector3 heading = new Vector3();
        Vector3 velocity = new Vector3();
        while (path.Count > 0)
        {
            Cell c = path.Peek();
            Vector3 target = c.transform.position;
            target.y += c.GetComponent<Collider>().bounds.extents.y;
            if (Vector3.Distance(enemy.transform.position, target) >= 0.05f)
            {
                heading = target - enemy.transform.position;
                heading.Normalize();
                velocity = heading * attractedEnemyMoveSpeed;

                enemy.transform.forward = heading;
                enemy.transform.position += velocity * Time.deltaTime;
                yield return null;
            }
            else
            {

                attr.xPositionCurrent = path.Peek().xCoordinate;
                attr.zPositionCurrent = path.Peek().zCoordinate;
                lastInPath = path.Pop();
            }
        }
        attr.cell = GameStateManager.FindCell(attr.xPositionCurrent, attr.zPositionCurrent);
        attr.cell.attachedUnit = enemy;
        attr.anim.SetBool("AttractWalk", false);
        yield return attr.TurnTowardsTarget(transform.position);
    }

    void ResetBeingUsed()
    {
        foreach (Cell c in GameStateManager.FindAllCells())
        {
            c.beingUsed = false;
        }
    }

    public void ListenForLightningStrike()
    {
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
                }
            }
        }
    }
}
