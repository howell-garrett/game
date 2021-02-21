using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public GameObject cellPrefab;
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
                Cell c = new Cell(i, j);
                row.Add(c);
                //CreateCell(x, z);
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
                CreateCell(i, j);
            }
        }
    }

    void CreateCell(int x, int z)
    {
        Vector3 position;
        position.x = x;
        position.y = 0f;
        position.z = z; ;

        GameObject cell = Instantiate<GameObject>(cellPrefab);
     
        cell.GetComponent<Cell>().xCoordinate = x;
        cell.GetComponent<Cell>().yCoordinate = z; //y in 2d z in 3d
        gameBoard[x][z] = cell.GetComponent<Cell>();
        cell.transform.SetParent(transform, false);
        cell.transform.position = position;
    }
}
