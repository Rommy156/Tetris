using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    //reference properties of the piece
    public TetronimoData data;
    public Board board;
    public Vector2Int position;
    public Vector2Int[] cells;
    public bool freeze = false;
    int activeCellCount = -1;


    // Initialize the piece with board and tetronimo data, and pass arguments to set the properties
    // searches the board's tetronimo array for the matching tetronimo data
    public void Initialize(Board board, Tetronimo tetronimo) 
    {
        //set reference to Board object
        this.board = board;
        //search for the tetronimo data in the board's tetronimo array
        { for (int i = 0; i < board.tetronimos.Length; i++)
                if (board.tetronimos[i].tetronimo == tetronimo)
                {
                    this.data = board.tetronimos[i];
                    break;
                }
        }
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++)   cells[i] = data.cells[i];

        //set starting position
        position = board.startPosition;

        activeCellCount = cells.Length;
    }
    private void Update()
    {
        //if game is over do not process any input
        if (board.tetrisManager.gameOver) return;
        
        //every frame clear the piece from the board 
        if (freeze) return;

        board.Clear(this);

        HandleRotation();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else 
        {
            //if we hard drop do not allow other movement
            //movement
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            //rotation input
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { Rotate(1); }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) { Rotate(-1); }
        }
        //set the piece back on the board at the new position
        board.Set(this);


        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }

    }
    void HandleRotation()
    {
        if (freeze || !board.isPositionValid(this, position + Vector2Int.down)) return;

        board.rotationTimer += Time.deltaTime;
        if (board.rotationTimer >= board.rotationInterval)
        {
            board.rotationTimer = 0f;
            Rotate(1);
        }
    }

    public void Rotate(int direction)
    {
        Vector2Int[] originalCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            originalCells[i] = cells[i];
        }
        ApplyRotation(direction);

        if (!board.isPositionValid(this, position))
        {
            if (!TryWallKick())
            {
                RevertRotation(originalCells);

            }
            else { Debug.Log("Walll kick successful"); }
        }

    }
    void RevertRotation(Vector2Int[] temporaryCells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = temporaryCells[i];
        }
    }
    bool TryWallKick()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>()

            //Vector2Int[] wallKickOffsets = new Vector2Int[]
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.down,
                new Vector2Int(-1, -1), //diagonal left down
                new Vector2Int(1, -1) //diagonal right down
            };

        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);

        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
            {

            }
        }

        return false;
    }
            //check if the new rotated position is valid
            //rotation
    void ApplyRotation(int direction)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, 90 * direction);


        bool isSpecial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;
        //additional check for D tetronimo
        bool isSpecial2 = data.tetronimo == Tetronimo.D;

        for (int i = 0; i < cells.Length; i++)
            {
                //local celll postion
                //Vector2Int cellPosition = cells[i];
                //cast to Vector3 because Quaternion multiplication requires Vector3
                Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

                if (isSpecial)
                {
                    cellPosition.x -= 0.5f;
                    cellPosition.y -= 0.5f;
                }
                else if (isSpecial2)
            {
                cellPosition.x -= .33333f;
                cellPosition.y -= .3333f;
            }
            //rotate the cell position
            Vector3 result = rotation * cellPosition;

                //place back into cells data
                if (isSpecial)
                {
                    cells[i].x = Mathf.CeilToInt(result.x);
                    cells[i].y = Mathf.CeilToInt(result.y);

                }
                else
                {
                    cells[i].x = Mathf.RoundToInt(result.x);
                    cells[i].y = Mathf.RoundToInt(result.y);
                }
            }

            
        } 
        
       
    


    public void HardDrop()
    {
        while (Move(Vector2Int.down)) { }
        freeze = true;
    }


    // Move the piece by a given translation vector
    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool isValid = board.isPositionValid(this, newPosition);
        if (isValid)
        {
            position = newPosition;
        }
        return isValid;
    }

    public void ReduceActiveCount()
    {
        activeCellCount -=1;
        if (activeCellCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
