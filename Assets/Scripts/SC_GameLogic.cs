using AssemblyCSharp;
using System;
using System.Collections;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SC_GameLogic : MonoBehaviour
{
    private Dictionary<string, GameObject> gameObjects;
    public SC_GlobalEnums.CurTurn curTurn;
    private SC_GlobalEnums.GameMode gameMode;
    private List<int> deck;
    private List<int> deckAI;
    public bool placingTile;
    public string tileToPlace;
    private int pickedHandIndex;

    public bool opponentWantToRestart;
    public bool restartMultiplayer;

    private int multiplayerTimer;
    private float timer;
    private float waitTime;

    public Sprite[] tileSprites;


    #region Singleton

    static SC_GameLogic instance;

    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_GameLogic").GetComponent<SC_GameLogic>();

            return instance;
        }
    }

    #endregion Singleton

    #region Events
    private void OnEnable()
    {
        SC_Tile.OnSlotClicked += OnSlotClicked;
    }

    private void OnDisable()
    {
        SC_Tile.OnSlotClicked -= OnSlotClicked;
    }

    // Event for a click on a tile in the hand slots
    private void OnSlotClicked(string _index, int handIndex)
    {
        if(curTurn != SC_GlobalEnums.CurTurn.Yours)
        {
            return;
        }

        _index = _index.Substring(_index.Length - 2);
        if(_index[0] == '_')
          _index = _index.Substring(1);
      
        tileToPlace = _index;
        placingTile = true;
        pickedHandIndex = handIndex;

        gameObjects["SP_Board"].GetComponent<SC_Board>().PlaceTile(tileToPlace);
    }
    #endregion

    #region MonoBehaviour
    void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        if (gameMode == SC_GlobalEnums.GameMode.SinglePlayer && curTurn == SC_GlobalEnums.CurTurn.Opponent)
        {
            if (timer >= waitTime)
            {
                MakeAIMove();
                timer = 0f;
                curTurn = SC_GlobalEnums.CurTurn.Yours;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = "Your turn";
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        if(gameMode == SC_GlobalEnums.GameMode.MultiPlayer)
        {
            int timeLeft = (29 - ((int)Time.time - multiplayerTimer));
            gameObjects["Txt_TurnTime"].GetComponent<Text>().text = "Turn time: " + timeLeft;
            if(curTurn == SC_GlobalEnums.CurTurn.Yours && timeLeft <= 0)
            {
                DrawCard();
            }

            if (restartMultiplayer && opponentWantToRestart)
            {
                WarpClient.GetInstance().startGame();
                gameObjects["Screen_GameOver"].SetActive(false);
            }
        }
    }
    #endregion

    #region Logic

    private void Init()
    {
        timer = 0f;
        waitTime = 4f;

        gameObjects = new Dictionary<string, GameObject>();
        GameObject[] _objects = GameObject.FindGameObjectsWithTag("GameObject");
        foreach (GameObject g in _objects)
            gameObjects.Add(g.name, g);

        gameObjects["Screen_GameOver"].SetActive(false);

    }

    // Set all data for a new game
    public void InitGame(SC_GlobalEnums.GameMode _gameMode=SC_GlobalEnums.GameMode.SinglePlayer)
    {
        opponentWantToRestart = false;
        restartMultiplayer = false;
        gameMode = _gameMode;
        deck = Enumerable.Range(0, 28).ToList();
        placingTile = false;

        for (int i = 0; i < 16; i++) // Set the hand slots to disabled
        {
            if (i < 10)
                gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);
            else
                gameObjects["Btn_HandSlot" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);
        }

        if(gameMode == SC_GlobalEnums.GameMode.SinglePlayer)
        {
            gameObjects["Txt_TurnTime"].GetComponent<Text>().text = "";

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                curTurn = SC_GlobalEnums.CurTurn.Yours;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = "Your turn";
            }
            else
            {
                curTurn = SC_GlobalEnums.CurTurn.Opponent;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = "Opponent turn";
            }

            for (int i = 0; i < 6; i++)
                gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[GetTileFromDeck()]);

            deckAI = new List<int>();
            for (int i = 0; i < 6; i++)
                deckAI.Add(GetTileFromDeck() + 1);
        }

        else if(gameMode == SC_GlobalEnums.GameMode.MultiPlayer)
        {
            multiplayerTimer = (int)Time.time;

            if (curTurn == SC_GlobalEnums.CurTurn.Yours)
            {
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = "Your turn";

                int[] tilesTaken = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    tilesTaken[i] = GetTileFromDeck();
                    gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[tilesTaken[i]]);
                }

                //Inform Other Player of the Tiles Picked
                Dictionary<string, object> _toSer = new Dictionary<string, object>();
                _toSer.Add("Action", "TilesPicked");
                Array.Sort(tilesTaken);
                for(int i=0; i<6; i++)
                    _toSer.Add("Value" + i, tilesTaken[i]);

                string _toSend = MiniJSON.Json.Serialize(_toSer);
                WarpClient.GetInstance().sendMove(_toSend);
            }
            else if(curTurn == SC_GlobalEnums.CurTurn.Opponent)
            {
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = "Opponent turn";
            }
        }

        SC_Board.Instance.ResetBoard();
    }

    // After placing the tile on board, remove it from the hand, also check win condition
    public void RemoveFromHand()
    {
        if (curTurn == SC_GlobalEnums.CurTurn.Opponent) return;

        // Change hand slot state and sprite
        if(pickedHandIndex < 10)
            gameObjects["Btn_HandSlot0" + pickedHandIndex.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);
        else
            gameObjects["Btn_HandSlot" + pickedHandIndex.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);

        // Pass turn and change message
        curTurn = SC_GlobalEnums.CurTurn.Opponent;
        if (gameObjects["Txt_Status"] != null)
            gameObjects["Txt_Status"].GetComponent<Text>().text = "Opponent turn";

        // Check win condition
        if (IsGameOver() == true)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;

            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You won";

            gameObjects["Screen_GameOver"].SetActive(true);

            /* Inform opponent of your won */
            Dictionary<string, object> _toSer = new Dictionary<string, object>();
            _toSer.Add("Action", "GameOver");
            _toSer.Add("Value", "win");

            string _toSend = MiniJSON.Json.Serialize(_toSer);
            WarpClient.GetInstance().sendMove(_toSend);
        }
    }

    // Draws a card from the deck
    public void DrawCard()
    {
        gameObjects["SP_Board"].GetComponent<SC_Board>().closePlacingButtons();

        if(deck.Count == 0)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;
            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You lost";
            gameObjects["Screen_GameOver"].SetActive(true);

            /* Inform opponent of your lose */
            Dictionary<string, object> _toSer = new Dictionary<string, object>();
            _toSer.Add("Action", "GameOver");
            _toSer.Add("Value", "lose");

            string _toSend = MiniJSON.Json.Serialize(_toSer);
            WarpClient.GetInstance().sendMove(_toSend);

            return;
        }

        if(curTurn != SC_GlobalEnums.CurTurn.Yours)
        {
            return;
        }

        int tileFromDeck = GetTileFromDeck();
        for (int i = 0; i<16; i++)
        {
            if (i < 10)
            {
                if (gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().state == SC_GlobalEnums.SlotState.Empty)
                {
                    gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[tileFromDeck]);
                    curTurn = SC_GlobalEnums.CurTurn.Opponent;
                    if (gameObjects["Txt_Status"] != null)
                        gameObjects["Txt_Status"].GetComponent<Text>().text = "Opponent turn";
                    break;
                }
            }
            else
            {
                if (gameObjects["Btn_HandSlot" + i.ToString()].GetComponent<SC_Tile>().state == SC_GlobalEnums.SlotState.Empty)
                {
                    gameObjects["Btn_HandSlot" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[tileFromDeck]);
                    curTurn = SC_GlobalEnums.CurTurn.Opponent;
                    if (gameObjects["Txt_Status"] != null)
                        gameObjects["Txt_Status"].GetComponent<Text>().text = "Opponent turn";
                    break;
                }
            }
        }

        //Inform Other Player of the tile drawn, in case of online game only
        if (gameMode == SC_GlobalEnums.GameMode.MultiPlayer)
        {
            Dictionary<string, object> _toSer = new Dictionary<string, object>();
            _toSer.Add("Action", "DrawTile");
            _toSer.Add("Value", tileFromDeck);
            string _toSend = MiniJSON.Json.Serialize(_toSer);
            WarpClient.GetInstance().sendMove(_toSend);
        }

        gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: " + deck.Count;
    }

    // For single player mode
    private void MakeAIMove()
    {
        if (gameObjects["SP_Board"] != null)
        {
            int indexPlaced = gameObjects["SP_Board"].GetComponent<SC_Board>().PlaceForAI(deckAI);
            if (indexPlaced == -1)
            {
                deckAI.Add(GetTileFromDeck() + 1);
                gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: " + deck.Count;
            }
            else
                deckAI.RemoveAt(indexPlaced);
        }

        if (IsGameOver() == true)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;

            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "Opponent won";

            gameObjects["Screen_GameOver"].SetActive(true);
        }
    }

    public void RestartGame()
    {
        if(gameMode == SC_GlobalEnums.GameMode.MultiPlayer)
        {
            WarpClient.GetInstance().SendChat("Restart");
            restartMultiplayer = true;
        }
        else
        {
            gameObjects["Screen_GameOver"].SetActive(false);
            InitGame(gameMode);
        }
    }

    // Set the next turn for the multiplayer mode
    public void UpdateMultiplayerTurn(string _nextTurn)
    {
        if (_nextTurn == SC_GlobalVariables.userId)
            curTurn = SC_GlobalEnums.CurTurn.Yours;
        else
            curTurn = SC_GlobalEnums.CurTurn.Opponent;
    }

    // Get opponent move in multiplayer mode
    public void MoveCompleted(MoveEvent _Move)
    {
        multiplayerTimer = (int)Time.time;

        if (_Move.getSender() == SC_GlobalVariables.userId)
            return;

        Dictionary<string, object> _data = MiniJSON.Json.Deserialize(_Move.getMoveData()) as Dictionary<string, object>;

        /* Players Picks tiles from deck at the start of the game */
        if ((string)_data["Action"] == "TilesPicked")        
        {
            if (curTurn == SC_GlobalEnums.CurTurn.Yours)
            {
                for (int i = 0; i < 6; i++)
                {
                    int toRemove = int.Parse(_data["Value" + i].ToString());
                    deck.Remove(toRemove);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    int toRemove = int.Parse(_data["Value" + i].ToString());
                    deck.RemoveAt(toRemove - i);
                }

                int[] tilesTaken = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    tilesTaken[i] = GetTileFromDeck();
                    gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[tilesTaken[i]]);
                }

                //Inform Other Player of the Tiles Picked
                Dictionary<string, object> _toSer = new Dictionary<string, object>();
                _toSer.Add("Action", "TilesPicked");
                Array.Sort(tilesTaken);
                for (int i = 0; i < 6; i++)
                    _toSer.Add("Value" + i, tilesTaken[i]);

                string _toSend = MiniJSON.Json.Serialize(_toSer);
                WarpClient.GetInstance().sendMove(_toSend);
            }

            gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: 16";
        }

        /* Opponent has drawn a tile from the deck */
        if ((string)_data["Action"] == "DrawTile")
        {
            int tileDrawn = int.Parse(_data["Value"].ToString());
            deck.Remove(tileDrawn);
            curTurn = SC_GlobalEnums.CurTurn.Yours;
            gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: " + deck.Count;
            gameObjects["Txt_Status"].GetComponent<Text>().text = "Your turn";
        }

        /* Opponent has placed a tile on board */
        if ((string)_data["Action"] == "TilePlaced")
        {
            int tilePlaced = int.Parse(_data["Value"].ToString());

            int posX = (int) float.Parse(_data["posX"].ToString());
            int posY = (int) float.Parse(_data["posY"].ToString());
            int posZ = (int) float.Parse(_data["posZ"].ToString());

            float rotX = float.Parse(_data["rotX"].ToString());
            float rotY = float.Parse(_data["rotY"].ToString());
            float rotZ = float.Parse(_data["rotZ"].ToString());

            Vector3 pos = new Vector3(posX, posY, posZ);
            Vector3 rot = new Vector3(rotX, rotY, rotZ);

            SC_Board.Instance.PlaceForOpponent(tilePlaced.ToString(), pos, rot);

            curTurn = SC_GlobalEnums.CurTurn.Yours;
            gameObjects["Txt_Status"].GetComponent<Text>().text = "Your turn";
        }

        /* GameOver detected by opponent */
        if ((string)_data["Action"] == "GameOver")
        {
            string enemyState = _data["Value"].ToString();
            curTurn = SC_GlobalEnums.CurTurn.GameOver;

            if (enemyState == "win")
            {
                if (gameObjects["Txt_GameOverStatus"] != null)
                    gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You lost";
            }
            else
            {
                if (gameObjects["Txt_GameOverStatus"] != null)
                    gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You won";
            }

            gameObjects["Screen_GameOver"].SetActive(true);

            WarpClient.GetInstance().stopGame();
        }
    }

    // Send placement data to the opponent
    public void SendPlacingToOpponent(Transform tileTransform, string tileVal = "")
    {
        if (gameMode != SC_GlobalEnums.GameMode.MultiPlayer)
            return;

        //Inform Other Player of the tile placed
        Dictionary<string, object> _toSer = new Dictionary<string, object>();
        _toSer.Add("Action", "TilePlaced");
        if(tileVal == "")
            _toSer.Add("Value", tileToPlace);
        else
            _toSer.Add("Value", tileVal);

        _toSer.Add("posX", tileTransform.position.x);
        _toSer.Add("posY", tileTransform.position.y);
        _toSer.Add("posZ", tileTransform.position.z);

        _toSer.Add("rotX", tileTransform.rotation.eulerAngles.x);
        _toSer.Add("rotY", tileTransform.rotation.eulerAngles.y);
        _toSer.Add("rotZ", tileTransform.rotation.eulerAngles.z);

        string _toSend = MiniJSON.Json.Serialize(_toSer);
        WarpClient.GetInstance().sendMove(_toSend);
    }

    #endregion

    #region GameUtils

    // Draws a random tile from the deck
    public int GetTileFromDeck()
    {
        int deckIndex = UnityEngine.Random.Range(0, deck.Count);
        int tileIndex = deck[deckIndex];
        deck.RemoveAt(deckIndex);
        return tileIndex;
    }

    // Check if one of the players is out of tiles
    private bool IsGameOver()     {
        if(gameMode == SC_GlobalEnums.GameMode.SinglePlayer && deckAI.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < 16; i++)
        {
            if (i < 10)
                if (gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().state != SC_GlobalEnums.SlotState.Empty)
                    return false;
            if (i >= 10)
                if (gameObjects["Btn_HandSlot" + i.ToString()].GetComponent<SC_Tile>().state != SC_GlobalEnums.SlotState.Empty)
                    return false;
        }
        return true;
    }
    #endregion
}
