using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsAttributes : MonoBehaviour
{
    public int actionPoints = 2;
    public int attackRange = 2;
    public Cell currentCell;

    [Header("Attack and Defense Attributes")]
    public float health = 10;

    [HideInInspector]
    public int actionPointsReset;
    public float healthReset;
    public List<Cell> selectableCells = new List<Cell>();
    public GameObject damageNumbers;

    [Header("Positional Information")]
    public int xPositionCurrent;
    public int zPositionCurrent;
    public int yPositionCurrent;

    public bool movementSelected = false;
    public bool attackingSelected = false;
    public bool shootSelected = false;
    public bool isSelected = false;
    public Animator anim;
    public SimpleTooltip stt;
    public bool hasBeenShotAt = false;
    // Start is called before the first frame update

    List<GameObject> players; 
    GameObject[] enemies; 
    void Start()
    {
        anim = GetComponent<Animator>();
        stt = GetComponent<SimpleTooltip>();
        players = convert(GameObject.FindGameObjectsWithTag("Player"));
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            players.Add(enemies[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        GameObject activeUnit = GameStateManager.activeUnit;
        if (activeUnit && activeUnit.name != gameObject.name && ReturnCurrentCell().isSelectable && !GameStateManager.isAnyoneAttacking && !GameStateManager.isAnyoneMoving)
        {
            activeUnit.GetComponent<TacticsMove>().MoveToCell(ReturnCurrentCell(), false);
            //GetComponent<PlayerMove>().MoveToCell(ReturnCurrentCell(), false);
        }
    }

    public void Deselect()
    {
        movementSelected = false;
        attackingSelected = false;
        shootSelected = false;
        isSelected = false;
        hasBeenShotAt = false;
    }

    public void GetCurrentCell()
    {
        currentCell = Grid.gameBoard[xPositionCurrent][zPositionCurrent];
        //currentCell =  GetTargetCell(gameObject);
        currentCell.setIsCurrent(true);
    }

    public Cell ReturnCurrentCell()
    {
        return Grid.gameBoard[xPositionCurrent][zPositionCurrent];
    }

    public void RemoveSelectableCells()
    {
        if (currentCell != null)
        {
            currentCell.isCurrent = false;
            currentCell = null;
        }
        foreach (Cell cell in selectableCells)
        {
            cell.ResetBFSVariables();
        }

        selectableCells.Clear();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        anim.SetTrigger("takeDamage");
        ShowFloatingDamage(damage);
    }
    
    void ShowFloatingDamage(int damage)
    {
        GameObject num = Instantiate(damageNumbers, transform.position + (Vector3.up), Quaternion.identity, transform);
        FaceTextMeshToCamera(num, Camera.main.transform);
        num.GetComponent<TextMesh>().text = damage.ToString();
    }

    private void OnMouseExit()
    {
        transform.GetChild(1).gameObject.SetActive(false);
        stt.HideTooltip();
    }

    private void OnMouseOver()
    {
        showHitPercentage();

    }

    private void OnMouseDown()
    {
        if (stt.showing)
        {
            hasBeenShotAt = true;
            stt.HideTooltip();
        }
    }

    void showHitPercentage()
    {
        GameObject anyoneShooting = null;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<TacticsAttributes>().shootSelected)
            {
                anyoneShooting = players[i];
            }
        }
        if (anyoneShooting == null || anyoneShooting.tag == tag)
        {
            return;
        }

        if (!ReturnCurrentCell().isInShootRange)
        {
            return;
        }

        //GameStateManager.SwapUnitLayer(2, players);

        checkIfShotIsBlocked(anyoneShooting.transform);

        //GameStateManager.SwapUnitLayer(0, players);
    }

    List<GameObject> convert(GameObject[] arr)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < arr.Length; i++)
        {
            list.Add(arr[i]);
        }
        return list;
    }

    void checkIfShotIsBlocked(Transform target)
    {
        Cell shooterCell = target.GetComponent<TacticsAttributes>().ReturnCurrentCell();
        Cell defenderCell = ReturnCurrentCell();
        if (!hasBeenShotAt)
        {
            if (defenderCell.isSafeWhenShot(shooterCell))
            {
                stt.infoLeft = ("Hit Percent: 0%" + "\n Enemy Health: " + health); 
                stt.ShowTooltip();
            }
            else
            {
                string left = "Hit Percent: 100%\n " + "Enemy Health: " + health;
                stt.infoLeft = left;
                stt.infoRight = ("Shot Damage Will Deal: " + GetComponent<TacticsShoot>().damage.ToString());
                stt.ShowTooltip();
            }
        }

        return;
    }

    public static void FaceTextMeshToCamera(GameObject textMeshObject, Transform textLookTargetTransform)
    {
        textMeshObject.transform.LookAt(2 * textMeshObject.transform.position - Camera.main.transform.position);
    }
}
