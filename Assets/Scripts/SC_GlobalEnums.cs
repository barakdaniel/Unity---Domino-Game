using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_GlobalEnums
{
    public enum Screens
    {
        MainMenu, Loading, Multiplayer, StudentInfo, Settings, Game
    };

    public enum CurTurn
    {
        Yours, Opponent, GameOver
    };

    // Refers to the hand slots
    public enum SlotState
    {
        Empty, Occupied
    };

    public enum GameMode
    {
        SinglePlayer, MultiPlayer
    };
}
