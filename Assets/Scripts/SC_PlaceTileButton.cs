using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is an utility class for tiles placing buttons on the board
 */
public class SC_PlaceTileButton : MonoBehaviour
{
    public int index;

    // Removes the clicked button and starting the logic of the placement
    public void OnClick()
    {
        GetComponentInParent<SC_BoardTile>().PlacingClicked(this.transform, index);
        GetComponentInParent<SC_BoardTile>().RemoveButton(index);
    }

    // If the button is coliided with a tile or the board borders => remove button
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "GameObject")
        {
            GetComponentInParent<SC_BoardTile>().RemoveButton(index);
        }
    }
}
