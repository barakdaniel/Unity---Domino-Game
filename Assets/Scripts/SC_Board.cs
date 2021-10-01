using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Board : MonoBehaviour
{
    public GameObject placementPrefab;
    private bool firstCard;

    private Dictionary<string, SC_BaseBoardTile> baseBoardTiles;
    private GameObject pre_boardTile;

    private List<SC_BoardTile> placedTiles;

    #region Singleton

    static SC_Board instance;

    public static SC_Board Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SP_Board").GetComponent<SC_Board>();

            return instance;
        }
    }

    #endregion Singleton

    #region MonoBehaviour
    private void Awake()
    {
        Init();
    }
    #endregion

    #region Logic
    private void Init()
    {
        pre_boardTile = Resources.Load("Prefabs/BoardTile") as GameObject;
        baseBoardTiles = new Dictionary<string, SC_BaseBoardTile>();
        firstCard = true;
        placedTiles = new List<SC_BoardTile>();

        for (int i = 1; i < 29; i++)
        {
            SC_BaseBoardTile tile = Resources.Load("BoardTiles/BoardTile_" + i) as SC_BaseBoardTile;
            if (tile != null)
                baseBoardTiles.Add("BoardTile_" + i, tile);
        }
    }

    // Inits the logic of placing a given tile
    public void PlaceTile(string tileToPlace)
    {
        if(firstCard == true)
        {
            GameObject _o = Instantiate(pre_boardTile);
            _o.transform.SetParent(GameObject.Find("SP_Board").transform, false);
            _o.transform.transform.localPosition = new Vector3(0, 0, 0);
            _o.GetComponent<SC_BoardTile>().SetTileData(baseBoardTiles["BoardTile_" + tileToPlace]);

            placedTiles.Add(_o.GetComponent<SC_BoardTile>());

            firstCard = false;

            SC_GameLogic.Instance.placingTile = false;
            SC_GameLogic.Instance.RemoveFromHand();

            SC_GameLogic.Instance.SendPlacingToOpponent(_o.transform, tileToPlace);
            return;
        }
        
        foreach(SC_BoardTile tile in placedTiles)
        {
            tile.OpenButtons(baseBoardTiles["BoardTile_" + tileToPlace].upValue, baseBoardTiles["BoardTile_" + tileToPlace].downValue);
        }
    }

    // Places the tile in the given position on  the board, and rotates it if needed
    // Also closes all the logic of placing the tile
    public void PlacingDone(Transform _pos, int _index, int _upVal, int _downVal)
    {
        GameObject _o = Instantiate(pre_boardTile);
        _o.transform.SetParent(GameObject.Find("SP_Board").transform, false);
        _o.transform.position = _pos.position;
        _o.transform.rotation = _pos.rotation;

        // In case the tile needed to be rotated to match the placement, rotates it, also deletes the button of the placement
        SC_BaseBoardTile tile = baseBoardTiles["BoardTile_" + SC_GameLogic.Instance.tileToPlace];
        if (_downVal == tile.downValue || _upVal == tile.downValue)
        {
            _o.transform.Rotate(0, 0, 180);
            _o.GetComponent<SC_BoardTile>().RemoveButton(3);
        }
        else
            _o.GetComponent<SC_BoardTile>().RemoveButton(2);

        _o.GetComponent<SC_BoardTile>().SetTileData(tile);

        // Add the tile to the board's list and remove it from hand
        placedTiles.Add(_o.GetComponent<SC_BoardTile>());
        SC_GameLogic.Instance.placingTile = false;
        SC_GameLogic.Instance.RemoveFromHand();

        // Closes all tiles placement buttons
        foreach (SC_BoardTile placedTile in placedTiles)
        {
            placedTile.OpenButtons(-1, -1);
        }

        SC_GameLogic.Instance.SendPlacingToOpponent(_o.transform);
    }

    // Closes the placing buttons of the tiles
    public void closePlacingButtons()
    {
        foreach (SC_BoardTile placedTile in placedTiles)
        {
            placedTile.OpenButtons(-1, -1);
        }
    }

    // Place a tile from the list on board if possible
    // Returns the index of the placed tile in case of success or -1 for failure
    public int PlaceForAI(List<int> tiles)
    {
        if (firstCard == true)
        {
            GameObject _o = Instantiate(pre_boardTile);
            _o.transform.SetParent(GameObject.Find("SP_Board").transform, false);
            _o.transform.transform.localPosition = new Vector3(0, 0, 0);
            _o.GetComponent<SC_BoardTile>().SetTileData(baseBoardTiles["BoardTile_" + tiles[0]]);

            placedTiles.Add(_o.GetComponent<SC_BoardTile>());

            firstCard = false;
            return 0;
        }

        foreach (SC_BoardTile tile in placedTiles)
        {
            int i = 0;

            foreach (int tileIndex in tiles)
            {
                SC_GameLogic.Instance.tileToPlace = tileIndex.ToString();
                if (tile.CheckPossiblePlacing(baseBoardTiles["BoardTile_" + tileIndex].upValue, baseBoardTiles["BoardTile_" + tileIndex].downValue) == true)
                    return i;

                i++;
            }
        }
        return -1;
    }

    // Given data of the opponnent placement, place the tile on the board
    public void PlaceForOpponent(string tileToPlace, Vector3 pos, Vector3 rot)
    {
        firstCard = false;

        GameObject _o = Instantiate(pre_boardTile);
        _o.transform.SetParent(GameObject.Find("SP_Board").transform, false);
        _o.transform.position = pos;

        _o.transform.Rotate(rot);

        SC_BaseBoardTile tile = baseBoardTiles["BoardTile_" + tileToPlace];
        _o.GetComponent<SC_BoardTile>().SetTileData(tile);
        placedTiles.Add(_o.GetComponent<SC_BoardTile>());
        SC_GameLogic.Instance.placingTile = false;
    }

    public void ResetBoard()
    {
        if (placedTiles == null)
            return;

        foreach (SC_BoardTile tile in placedTiles)
        {
            Destroy(tile);
        }

        Init();
    }
    #endregion
}
