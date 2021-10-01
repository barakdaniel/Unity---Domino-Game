using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * This class refers to the hand slots of the player.
 */
public class SC_Tile : MonoBehaviour
{
    public delegate void SlotClickedHandler(string _index, int handIndex);
    public static event SlotClickedHandler OnSlotClicked;

    public int index;
    public Image slotImage;

    public SC_GlobalEnums.SlotState state;

    #region MonoBehaviour
    private void Awake()
    {
        index = int.Parse(name.Substring(name.Length - 2));
    }
    #endregion

    #region Logic
    public void Click()
    {
        if (OnSlotClicked != null)
            OnSlotClicked(slotImage.sprite.name, index);
    }

    // Changes the slot state to 'Empty' or 'Occupied', if the new state is not Empty, then set a sprite as well.
    public void ChangeSlotState(SC_GlobalEnums.SlotState _newState, Sprite _newSprite)
    {
        if(slotImage != null)
        {
            if (_newState == SC_GlobalEnums.SlotState.Empty)
            {
                slotImage.enabled = false;
            }
            else
            {
                slotImage.sprite = _newSprite;
                slotImage.enabled = true;
            }

            state = _newState;
        }
    }

    public bool isEmpty()
    {
        if (state == SC_GlobalEnums.SlotState.Empty)
            return true;
        else
            return false;
    }
    #endregion
}
