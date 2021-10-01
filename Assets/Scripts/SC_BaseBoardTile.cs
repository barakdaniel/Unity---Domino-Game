using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Tile", menuName = "BoardTile")]
public class SC_BaseBoardTile : ScriptableObject
{
    public Sprite tile;
    public int upValue, downValue;

    public void PrintTile()
    {
        Debug.Log("Tile up value = " + upValue + " down value = " + downValue);
    }
}
