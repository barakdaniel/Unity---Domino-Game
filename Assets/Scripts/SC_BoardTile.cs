using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This class refers to the tiles placed on the board
 * Each tile hold 4 buttons for chaining tiles to him
 */
public class SC_BoardTile : MonoBehaviour
{
    public Image tile;
    public int upValue, downValue;
    public SC_BaseBoardTile tileData;
    public Button[] placingButtons; // buttons index order: Right, Left, Up, Downs;
    private bool[] validButtons;

    #region MonoBehaviour
    void Awake()
    {
        InitBoardTile();
    }

    private void OnDestroy()
    {
        Destroy(transform.gameObject);
    }
    #endregion

    #region Logic
    private void InitBoardTile()
    {
        validButtons = new bool[4];

        tile.sprite = tileData.tile;
        upValue = tileData.upValue;
        downValue = tileData.downValue;

        int i = 0;
        foreach(Button btn in placingButtons)
        {
            btn.image.enabled = false;
            validButtons[i] = true;
            i++;
        }        
    }

    // Set values and sprite
    public void SetTileData(SC_BaseBoardTile _tileData)
    {
        tileData = _tileData;

        tile.sprite = tileData.tile;
        upValue = tileData.upValue;
        downValue = tileData.downValue;
    }

    // Open all relevent button of the tiles according to its values
    public void OpenButtons(int up, int down)
    {
        if(up == upValue || down == upValue)
        {
            if(validButtons[0] != false)
                placingButtons[0].image.enabled = true;
            if (validButtons[1] != false)
                placingButtons[1].image.enabled = true;
            if (validButtons[2] != false)
                placingButtons[2].image.enabled = true;
        }
        else
        {
            if (validButtons[0] != false)
                placingButtons[0].image.enabled = false;
            if (validButtons[1] != false)
                placingButtons[1].image.enabled = false;
            if (validButtons[2] != false)
                placingButtons[2].image.enabled = false;
        }

        if (down == downValue || up == downValue)
        {
            if (validButtons[0] != false)
                placingButtons[0].image.enabled = true;
            if (validButtons[1] != false)
                placingButtons[1].image.enabled = true;
            if (validButtons[3] != false)
                placingButtons[3].image.enabled = true;
        }
        else
        {
            if (validButtons[3] != false)
                placingButtons[3].image.enabled = false;
        }
    }

    // Return if a placing is possible with the given values
    public bool CheckPossiblePlacing(int up, int down)
    {
        if (up == upValue || down == upValue)
        {
            if (validButtons[0] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[0].transform, 0, upValue, downValue);
                return true;
            }
            else if(validButtons[1] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[1].transform, 1, upValue, downValue);
                return true;
            }
            else if (validButtons[2] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[2].transform, 2, upValue, downValue);
                return true;
            }
        }
        if(down == downValue || up == downValue)
        {
            if (validButtons[0] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[0].transform, 0, upValue, downValue);
                return true;
            }
            else if (validButtons[1] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[1].transform, 1, upValue, downValue);
                return true;
            }
            else if (validButtons[3] == true)
            {
                SC_Board.Instance.PlacingDone(placingButtons[3].transform, 3, upValue, downValue);
                return true;
            }
        }


        return false;
    }

    // Placing button clicked event handler
    public void PlacingClicked(Transform _pos, int _index)
    {
        SC_Board.Instance.PlacingDone(_pos, _index, upValue, downValue);
    }

    // Remove one of the buttons
    public void RemoveButton(int _index)
    {
        placingButtons[_index].image.enabled = false;
        validButtons[_index] = false;
    }
    #endregion
}
