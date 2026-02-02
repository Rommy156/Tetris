using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    //variables for the board
    public TetronimoData[] tetronimos;

    public Vector2Int boardSize; 
    public Vector2Int startPosition;
    public Piece piecePrefab;
    public Tilemap tilemap;
    public TetrisManager tetrisManager;
    public float dropInterval = 0.5f;

    public float rotationInterval = 2f;
    public int autoRotationDiretion = 1;

    public float rotationTimer = 0f;
    Piece activePiece;
    float time = 0.0f;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();

    
    //board boundaries
    int left { get { return -boardSize.x / 2; } }
    int right { get { return boardSize.x / 2; } }
    int bottom { get { return -boardSize.y / 2; } }
    int top { get { return boardSize.y / 2; } }


    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        //do not update if game is over
        if (tetrisManager.gameOver) return;
        //handle piece dropping over time
        //rotate piece as it drops

        time += Time.deltaTime;
        rotationTimer += Time.deltaTime;

        if (rotationTimer >= rotationInterval )
        {
            
            rotationTimer = 0.0f;

            Clear(activePiece);

            //rotate the piece
            activePiece.Rotate(autoRotationDiretion);

            //if rotation is not valid rotate back
            if (!isPositionValid(activePiece, activePiece.position))
            {
                activePiece.Rotate(-autoRotationDiretion);
            }

            bool moveResult = activePiece.Move(Vector2Int.down);

            Set(activePiece);

            if (!moveResult )
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }
    public void SpawnPiece()
    {
        //make rotation direction random
        autoRotationDiretion = Random.value > 0.5f ? 1 : -1;

        activePiece = Instantiate(piecePrefab, transform);
        Tetronimo t = (Tetronimo)Random.Range(0, tetronimos.Length);
        activePiece.Initialize(this, t);
        CheckEndGame();
        Set(activePiece);
    }

     void CheckEndGame()
    {
        if (!isPositionValid(activePiece, activePiece.position))
        {
            //signal to end game
            tetrisManager.SetGameOver(true);
        }
    }
    public void UpdateGameOver()
    {
        //reset the board if game is not over
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        Piece [] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces)
        {
            Destroy(piece.gameObject);
        }
        activePiece = null;

        tilemap.ClearAllTiles();
        this.pieces.Clear();
        SpawnPiece();
    }
    void SetTile(Vector3Int cellPosition,  Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            //piece.ReduceActiveCount();
            pieces.Remove(cellPosition);
        }

        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);
            pieces[cellPosition] = piece;
        }
        
    }
    //set a piece on the board
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }
    public bool isPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //calculate cell position on the board
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);
            //check if cell is within boundaries
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;

    }
    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {

                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }
        //set score
        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);

        int rowsShiftDown = 0;
        foreach (int y in destroyedLines)
            {
                ShiftRowsDown (y- rowsShiftDown);
                rowsShiftDown++;
        }

    }

    public void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            //clear the tile at cell position
            if (pieces.ContainsKey(cellPosition))
            {
                Piece activePiece = pieces[cellPosition];
                activePiece.ReduceActiveCount();
                SetTile(cellPosition, null);
            }
                            
        }
        
    }
    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                //if cell position is in dictionary 
                if (pieces.ContainsKey(cellPosition))
                {
                    //store the tile temporaryily
                    Piece currentPiece = pieces[cellPosition];
                    //clear the position
                    SetTile(cellPosition, null);
                    //set the tile one row down
                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    public bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int pos = new Vector3Int(x, y, 0);
            if (!tilemap.HasTile(pos)) return false;
        }
        return true;
    }

}
