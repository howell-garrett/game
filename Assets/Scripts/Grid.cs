using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public GameObject cellPrefab;
    public GameObject blockedPrefab;
    public GameObject fullCoverPrefab;
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
                if (j == 7 && i < 6) { c.isBlocked = true; c.isCovered = true; } //temp to add height
                //if (j == 8 && i <= 5) { c.isBlocked = true; }
                row.Add(c);
            }
            gameBoard.Add(row);
        }
        gameBoard[5][3].isCovered = true;
        gameBoard[5][3].isBlocked = true;

        gameBoard[3][3].isCovered = true;
        gameBoard[3][3].isBlocked = true;

        gameBoard[2][4].isCovered = true;
        gameBoard[2][4].isBlocked = true;

        RenderBoard();
    }

    public void RenderBoard()
    {
        for (int i = 0; i < gameBoard.Count; i++)
        {
            for (int j = 0; j < gameBoard[i].Count; j++)
            {
                bool isOdd = false;
                if (j % 2 != 0)
                {
                    isOdd = true;
                }
                CreateCell(gameBoard[i][j], isOdd);
            }
        }
    }

    void CreateCell(Cell cell, bool isOdd)
    {
        Vector3 position;
        position.x = cell.xCoordinate; // * 1.05f;
        position.y = 0f + cell.yCoordinate;
        position.z = cell.zCoordinate; // * 0.9f;

        if (isOdd)
        {
            //position.x += 0.512f; these comments are remanants from hexagon board
        }
        
       if (cell.isCovered)
        {
            GameObject cellGameObject = Instantiate<GameObject>(fullCoverPrefab);

            cellGameObject.name = cell.xCoordinate + "," + cell.zCoordinate;

            cellGameObject.GetComponent<Cell>().xCoordinate = cell.xCoordinate;
            cellGameObject.GetComponent<Cell>().yCoordinate = cell.yCoordinate;
            cellGameObject.GetComponent<Cell>().zCoordinate = cell.zCoordinate;
            cellGameObject.GetComponent<Cell>().isBlocked = cell.isBlocked;
            cellGameObject.GetComponent<Cell>().isCovered = cell.isCovered;


            gameBoard[cell.xCoordinate][cell.zCoordinate] = cellGameObject.GetComponent<Cell>();
            cellGameObject.transform.SetParent(transform, false);
            cellGameObject.transform.position = position;
        } else
        {
            GameObject cellGameObject = Instantiate<GameObject>(cellPrefab);

            cellGameObject.name = cell.xCoordinate + "," + cell.zCoordinate;

            cellGameObject.GetComponent<Cell>().xCoordinate = cell.xCoordinate;
            cellGameObject.GetComponent<Cell>().yCoordinate = cell.yCoordinate;
            cellGameObject.GetComponent<Cell>().zCoordinate = cell.zCoordinate;
            cellGameObject.GetComponent<Cell>().isBlocked = cell.isBlocked;
            cellGameObject.GetComponent<Cell>().isCovered = cell.isCovered;

            gameBoard[cell.xCoordinate][cell.zCoordinate] = cellGameObject.GetComponent<Cell>();
            cellGameObject.transform.SetParent(transform, false);
            cellGameObject.transform.position = position;
        }
        


    }
}
