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
    public GameObject bigProjectilePrefab;
    public GameObject impactPrefab;
    public int damage = 150;
    public int shotCost = 2;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    public void Init()
    {
        attributes = GetComponent<TacticsAttributes>();
    }

    public void FindSelectableCells(Cell cellParam, bool usingShootAbility)
    {
        GameStateManager.ResetCellBools();
        GameStateManager.ComputeAdjList();
        int fireRange = shootRange;
        if (usingShootAbility)
        {
            fireRange = GetComponent<AbilityAttributes>().GetShootAbilityRange();
        }
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
            if (usingShootAbility)
            {
                c.isInAbilityRange = true;
            }
            attributes.selectableCells.Add(c);

            if (c.distance < fireRange)
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
    public void SpawnBullet(GameObject enemy, bool isBigShot)
    {
        target = enemy;
        if (!isBigShot)
        {
            projectile = Instantiate(projectilePrefab, transform.position + (Vector3.up * .7f), Quaternion.identity);
        } else
        {
            projectile = Instantiate(bigProjectilePrefab, transform.position + (Vector3.up * .7f), Quaternion.identity);
        }
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

    public void PerformShoot(Cell c, int howManyShots, bool isBigShot)
    {
        //Dynamic Dispatch wasn't working pain
        if (GetComponent<AbilityAttributes>() is AbilityAttributes)
        {
            //GetComponent<EarthAttacks>().PerformShoot(c, howManyShots, isBigShot);
            GetComponent<AbilityAttributes>().PerformShoot(c, howManyShots, isBigShot);
            return;
        }
        print("subuwu");
        StartCoroutine(ShootCoroutine(this, c, howManyShots, isBigShot));
    }

    IEnumerator ShootCoroutine(TacticsShoot ps, Cell c, int howManyShots, bool isBigShot)
    {
        int count = 0;
        while (count < howManyShots)
        {

            ps.SetUpShot(c.attachedUnit, isBigShot);
            count++;
            yield return new WaitForSeconds(.1f);
            //timer = 0;
        }
    }

    public void SetUpShot(GameObject targetUnit, bool isBigShot)
    {
        SpawnBullet(targetUnit, isBigShot);
        isShooting = true;
        GameStateManager.isAnyoneAttacking = true;
        attributes.anim.SetTrigger("Attack");
        attributes.actionPoints -= shotCost;
    }
    public bool HasLineOfSight(Cell target)
    {
        GameStateManager.ChangeUnitsRaycastLayer(false);
        GameStateManager.ChangeNeighboringCoverLayer(attributes.cell, false);
        GameStateManager.ChangeNeighboringCoverLayer(target, false);
        RaycastHit hit;
        Vector3 targetVec = new Vector3(target.transform.position.x, target.transform.position.y + .5f, target.transform.position.z);
        Vector3 rightVec = transform.position + Vector3.right + Vector3.up*.5f;
        Vector3 leftVec = transform.position + Vector3.left + Vector3.up * .5f;
        Vector3 upVec = transform.position + Vector3.forward + Vector3.up * .5f;
        Vector3 downVec = transform.position + Vector3.back + Vector3.up * .5f;
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
            print(downVec);
            print(targetVec);
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
