using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public GameObject cellPrefab;
    public GameObject blockedPrefab;
    public GameObject fullCoverPrefab;
    public static int gameBoardWidth, gameBoardHeight;

    public static List<List<Cell>> gameBoard;
    public static bool isPlayerTurn = true;

    List<Cell> cells;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(1 / Time.unscaledDeltaTime);
    }

    //[MenuItem("Asdf/Build Game board #%w")]
    public void Awake()
    {
        gameBoard = new List<List<Cell>>();
        cells = new List<Cell>();
        gameBoardHeight = 0;
        gameBoardWidth = 0;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Cell"))
        {
            Cell c = go.GetComponent<Cell>();
            c.SetLocationToEqualWorldSpace();
            cells.Add(c);
            if (c.zCoordinate > gameBoardHeight)
            {
                gameBoardHeight = c.zCoordinate;

            }
            if (c.xCoordinate > gameBoardWidth)
            {
                gameBoardWidth = c.xCoordinate;
            }
        }
        for (int i = 0; i <= gameBoardWidth; i++)
        {
            gameBoard.Add(new List<Cell>());
            for (int j = 0; j <= gameBoardHeight; j++)
            {
                gameBoard[i].Add(null);
            }
        }
        foreach (Cell c in cells)
        {
            gameBoard[c.xCoordinate][c.zCoordinate] = c; 
        }
        
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
        position.y = cell.yCoordinate;
        position.z = cell.zCoordinate; // * 0.9f;

        if (isOdd)
        {
            //position.x += 0.512f; these comments are remanants from hexagon board
        }
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

        if (cell.isBlocked)
        {
            GameObject cover = Instantiate<GameObject>(fullCoverPrefab);
            cover.GetComponent<BlockedCell>().cell = cellGameObject.GetComponent<Cell>();
            cover.transform.position = cellGameObject.transform.position;
            cover.transform.position += new Vector3(0,  cellGameObject.GetComponent<Collider>().bounds.extents.y, 0);
            cover.transform.position += new Vector3(0,  cover.GetComponent<Collider>().bounds.extents.y, 0);
        }
    }
}
