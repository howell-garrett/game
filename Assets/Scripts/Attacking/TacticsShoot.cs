using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsShoot : MonoBehaviour
{
    public TacticsAttributes attributes;
    public bool checkedSelectableCells = false;
    public bool isShooting;
    public int shootRange = 5;
    public GameObject target;
    public GameObject projectilePrefab;
    public GameObject impactPrefab;
    public int damage = 150;
    public int shotCost = 2;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ComputeAdjList()
    {
        Cell[] cells = GameStateManager.FindAllCells();
        foreach (Cell c in cells)
        {
            c.FindNeighbors(0);
        }
    }

    public void FindSelectableCells(Cell cellParam)
    {
        ComputeAdjList();

        Queue<Cell> process = new Queue<Cell>();
        process.Enqueue(cellParam);
        if (cellParam)
        {
            cellParam.visited = true;
        }
        while (process.Count > 0)
        {
            Cell c = process.Dequeue();

            c.isInShootRange = true;
            attributes.selectableCells.Add(c);

            if (c.distance < shootRange)
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

    GameObject projectile;
    public void SpawnBullet(GameObject enemy)
    {
        target = enemy;
        projectile = Instantiate(projectilePrefab, transform.position + (Vector3.up * .7f), Quaternion.identity);
        ProjectileAttributes pa = projectile.GetComponent<ProjectileAttributes>();
        pa.target = enemy.transform;
        pa.velocity = velocity;
        pa.heading = heading;
        pa.damage = damage;
        pa.impactPrefab = impactPrefab;
        if (!target.GetComponent<TacticsAttributes>().ReturnCurrentCell().isSafeWhenShot(attributes.ReturnCurrentCell())) //if enemy is not behind cover
        {
            projectile.GetComponent<ProjectileAttributes>().willMiss = false;
        } else
        {
            Cell coverCell = target.GetComponent<TacticsAttributes>().ReturnCurrentCell().GetCoverCell(attributes.ReturnCurrentCell());
            pa.target = coverCell.transform;
            pa.willMiss = true;
        }
        
    }

    public void SetUpShot(GameObject targetUnit)
    {
        SpawnBullet(targetUnit);
        isShooting = true;
        GameStateManager.isAnyoneAttacking = true;
        attributes.anim.SetTrigger("Attack");
        attributes.actionPoints -= shotCost;
    }
    //test
    public bool HasLineOfSight(Cell target)
    {
        GameStateManager.ChangeUnitsRaycastLayer(false);
        GameStateManager.ChangeNeighboringCoverLayer(attributes.cell, false);
        GameStateManager.ChangeNeighboringCoverLayer(target, false);
        RaycastHit hit;
        Vector3 targetVec = new Vector3(target.transform.position.x, target.transform.position.y + .5f, target.transform.position.z);
        Vector3 rightVec = transform.position + Vector3.right + Vector3.up*.5f;
        Vector3 leftVec = transform.position + Vector3.left + Vector3.up * .5f;
        Vector3 upVec = transform.position + Vector3.up + Vector3.up * .5f;
        Vector3 downVec = transform.position + Vector3.down + Vector3.up * .5f;
        Vector3 currentVec = transform.position + Vector3.up * .5f;
        bool final = false;
        if (!Physics.Raycast(currentVec, targetVec - currentVec, out hit, Vector3.Distance(currentVec, targetVec)))
        {
            print("current cell clear shot");
            final = true;
        }
        else if (!Physics.Raycast(rightVec, targetVec - rightVec, out hit, Vector3.Distance(rightVec, targetVec)))
        {
            print("right side clear shot");
            final = true;
        }
        else if (!Physics.Raycast(leftVec, targetVec - leftVec, out hit, Vector3.Distance(leftVec, targetVec)))
        {
            print("Left side clear shot");
            final = true;
        }
        else if (!Physics.Raycast(upVec, targetVec - upVec, out hit, Vector3.Distance(upVec, targetVec)))
        {
            print("up side clear shot");
            final = true;
        }
        else if (!Physics.Raycast(downVec, targetVec - downVec, out hit, Vector3.Distance(downVec, targetVec)))
        {
            print("down side clear shot");
            final = true;
        }
        GameStateManager.ChangeNeighboringCoverLayer(attributes.cell, true);
        GameStateManager.ChangeNeighboringCoverLayer(target, true);
        GameStateManager.ChangeUnitsRaycastLayer(true);
        if (!final)
        {
            print("no line of sight");
        }
        return final;
    }

    public void Shoot()
    {

        if (GameObject.FindGameObjectsWithTag("Projectile").Length == 0)
        {
            GameStateManager.isAnyoneAttacking = false;
            GameStateManager.DeselectAllUnits();
            isShooting = false;
            return;
        }
    }
}
