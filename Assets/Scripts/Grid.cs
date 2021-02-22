using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public GameObject cellPrefab;
    public GameObject rockPrefab;
    public int gameBoardWidth, gameBoardHeight;

    public static List<List<Cell>> gameBoard;
    public static bool isPlayerTurn = true;

    GameObject[] cells;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {

        gameBoard = new List<List<Cell>>();
        for (int i = 0; i < gameBoardHeight; i++)
        {
            List<Cell> row = new List<Cell>();
            for (int j = 0; j < gameBoardWidth; j++)
            {
                Cell c = new Cell(i, j, 1);
                if (j == 7 && i <= 5) { c.isBlocked = true; } //temp to add height
                if (j == 8 && i <= 5) { c.isBlocked = true; }
                row.Add(c);
            }
            gameBoard.Add(row);
        }
        RenderBoard();
    }

    public void RenderBoard()
    {
        for (int i = 0; i < gameBoard.Count; i++)
        {
            for (int j = 0; j < gameBoard[i].Count; j++)
            {
                CreateCell(gameBoard[i][j]);
            }
        }
    }

    void CreateCell(Cell cell)
    {
        Vector3 position;
        position.x = cell.xCoordinate;
        position.y = 0f + cell.yCoordinate;
        position.z = cell.zCoordinate; ;

        GameObject cellGameObject = Instantiate<GameObject>(cellPrefab);

        cellGameObject.GetComponent<Cell>().xCoordinate = cell.xCoordinate;
        cellGameObject.GetComponent<Cell>().yCoordinate = cell.yCoordinate; //y in 2d z in 3d
        cellGameObject.GetComponent<Cell>().zCoordinate = cell.zCoordinate;
        cellGameObject.GetComponent<Cell>().isBlocked = cell.isBlocked;

        gameBoard[cell.xCoordinate][cell.zCoordinate] = cellGameObject.GetComponent<Cell>();
        cellGameObject.transform.SetParent(transform, false);
        cellGameObject.transform.position = position;

    }
}
