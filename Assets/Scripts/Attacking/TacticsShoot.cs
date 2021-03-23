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
        List<List<Cell>> board = Grid.gameBoard;
        for (int i = 0; i < board.Count; i++)
        {
            for (int j = 0; j < board[i].Count; j++)
            {
                Cell c = board[i][j].GetComponent<Cell>();
                c.FindNeighbors(0);
            }
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
        Debug.Log(target);
        projectile = Instantiate(projectilePrefab, transform.position + (Vector3.up * .7f), Quaternion.identity);
        if (!target.GetComponent<TacticsAttributes>().ReturnCurrentCell().isSafeWhenShot(attributes.ReturnCurrentCell())) //if enemy is not behind cover
        {
            //target = enemy;
            projectile.GetComponent<ProjectileAttributes>().willMiss = false;
        } else
        {
            Cell coverCell = target.GetComponent<TacticsAttributes>().ReturnCurrentCell().GetCoverCell(attributes.ReturnCurrentCell());
            target = coverCell.gameObject;
            projectile.GetComponent<ProjectileAttributes>().willMiss = true;
        }
        
    }

    public void SetUpShot(GameObject targetUnit)
    {
        SpawnBullet(targetUnit);
        isShooting = true;
        GameStateManager.isAnyoneAttacking = true;
        attributes.anim.SetTrigger("Attack");
    }

    public void Shoot()
    {

        if (projectile == null)
        {
            GameStateManager.isAnyoneAttacking = false;
            GameStateManager.DeselectAllUnits();
            attributes.actionPoints--;
            attributes.shootSelected = false;
            isShooting = false;
            return;
        }

        if (Vector3.Distance(projectile.transform.position, target.transform.position + (Vector3.up * .7f)) >= 0.05f)
        {

            CalculateHeading(target.transform.position);
            SetHorizontalVelocity();

            transform.forward = heading;
            projectile.transform.position += velocity * Time.deltaTime;
        } else
        {
            GameStateManager.isAnyoneAttacking = false;
            GameStateManager.DeselectAllUnits();
            Transform t = projectile.transform;
            attributes.actionPoints--;
            attributes.shootSelected = false;
            Instantiate(impactPrefab, t.position, t.rotation);
            Destroy(projectile);
            isShooting = false;
            target.GetComponent<TacticsAttributes>().TakeDamage(damage);
        }
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * 3;
    }
}
