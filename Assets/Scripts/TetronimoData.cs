using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Enumeration for different tetronimo types. can also use an Integer to reperesent the data.\
// Its a mapping of the tetronimo shapes to an int.
public enum Tetronimo
{
    I , O, T, S, Z, J, L
}

[Serializable]
// Data structure to hold tetronimo information
public struct TetronimoData 
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;

}