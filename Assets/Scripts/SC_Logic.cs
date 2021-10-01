using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Logic : MonoBehaviour
{
    private string apiKey = "API Key goes here";        // built for using AppWarp
    private string secretKey = "Secret key goes here";  // built for using AppWarp

    private Dictionary<string, GameObject> unityObjects;
    private Stack<SC_GlobalEnums.Screens> screens_stack;
    private SC_GlobalEnums.Screens currScreen;
    private SC_GlobalEnums.GameMode gameMode;

    private Listener listener;
    private List<string> roomIds;
    private string roomId;
    private int roomIndex;
    private Dictionary<string, object> passedParams;

    private int betValue;

    #region Singleton

    static SC_Logic instance;

    public static SC_Logic Instance
    {
        get
        {
            if(instance == null)
                instance = GameObject.Find("SC_Logic").GetComponent<SC_Logic>();

            return instance;
        }
    }

    #endregion Singleton

    #region MonoBehaviour

    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnChatReceived += OnChatReceived;
        Listener.OnUserLeftRoom += OnUserLeftRoom;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnChatReceived -= OnChatReceived;
        Listener.OnUserLeftRoom -= OnUserLeftRoom;
    }

    void Awake() { Init(); }

    #endregion

    #region SC_Controller

    public void Btn_SinglePlayerLogic()
    {
        gameMode = SC_GlobalEnums.GameMode.SinglePlayer;
        ChangeScreen(SC_GlobalEnums.Screens.Game);
    }

    public void Btn_MultiplayerLogic()
    {
        unityObjects["Btn_StartMultiplayer"].GetComponent<Button>().interactable = true;
        unityObjects["Txt_MatchMaking"].GetComponent<Text>().text = "";
        
        gameMode = SC_GlobalEnums.GameMode.MultiPlayer;
        ChangeScreen(SC_GlobalEnums.Screens.Multiplayer);
    }

    public void Btn_StudentInfoLogic()
    {
        ChangeScreen(SC_GlobalEnums.Screens.StudentInfo);
    }

    internal void Btn_BackToMenu()
    {
        ChangeScreen(SC_GlobalEnums.Screens.MainMenu);

        if(gameMode == SC_GlobalEnums.GameMode.MultiPlayer)
        {
            WarpClient.GetInstance().UnsubscribeRoom(roomId);
            WarpClient.GetInstance().LeaveRoom(roomId);
        }
        if (GameObject.Find("Screen_GameOver") != null)
            GameObject.Find("Screen_GameOver").SetActive(false);
    }

    public void Btn_SettingsLogic()
    {
        ChangeScreen(SC_GlobalEnums.Screens.Settings);
    }

    public void Slider_MusicVolumeLogic()
    {
        unityObjects["Txt_MusicValue"].GetComponent<Text>().text
            = "Music: " + (int)unityObjects["Slider_MusicVolume"].GetComponent<Slider>().value;
    }

    public void Slider_SfxVolumeLogic()
    {
        unityObjects["Txt_SfxValue"].GetComponent<Text>().text
            = "SFX: " + (int)unityObjects["Slider_SfxVolume"].GetComponent<Slider>().value;
    }

    public void Btn_BackLogic()
    {
        SC_GlobalEnums.Screens tempScreen = screens_stack.Pop();
        unityObjects["Screen_" + tempScreen].SetActive(true);
        unityObjects["Screen_" + currScreen].SetActive(false);
        currScreen = tempScreen;
    }

    public void Slider_MultiplayerLogic()
    {
        unityObjects["Txt_BetValue"].GetComponent<Text>().text 
            = "" + (int)unityObjects["Slider_Multiplayer"].GetComponent<Slider>().value + "$";
    }

    public void Btn_StartMultiplayerLogic()
    {
        betValue = (int)unityObjects["Slider_Multiplayer"].GetComponent<Slider>().value;
        passedParams["Password"] = betValue;
        unityObjects["Btn_StartMultiplayer"].GetComponent<Button>().interactable = false;
        unityObjects["Txt_MatchMaking"].GetComponent<Text>().text = "Searching for oppenent...";
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
    }

    public void Btn_CVLogic()
    {
        Application.OpenURL("https://drive.google.com/file/d/1OWLhqiuKeO8eC8-I4eceRmq4eaABAVEC/view?usp=sharing");
    }

    #endregion

    #region Logic

    private void Init()
    {
        passedParams = new Dictionary<string, object>();
        passedParams.Add("Password", "0");

        screens_stack = new Stack<SC_GlobalEnums.Screens>();
        currScreen = SC_GlobalEnums.Screens.MainMenu;

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _objs = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _objs)
        unityObjects.Add(g.name, g);

        unityObjects["Screen_Loading"].SetActive(false);
        unityObjects["Screen_Multiplayer"].SetActive(false);
        unityObjects["Screen_StudentInfo"].SetActive(false);
        unityObjects["Screen_Settings"].SetActive(false);
        unityObjects["Screen_Game"].SetActive(false);
        unityObjects["SC_GameLogic"].SetActive(false);

        if (listener == null)
            listener = new Listener();

        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listener);
        WarpClient.GetInstance().AddChatRequestListener(listener);
        WarpClient.GetInstance().AddUpdateRequestListener(listener);
        WarpClient.GetInstance().AddLobbyRequestListener(listener);
        WarpClient.GetInstance().AddNotificationListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listener);
        WarpClient.GetInstance().AddZoneRequestListener(listener);

        SC_GlobalVariables.userId = System.DateTime.Now.Ticks.ToString();
        WarpClient.GetInstance().Connect(SC_GlobalVariables.userId);
    }

    private void ChangeScreen(SC_GlobalEnums.Screens _newScreen)
    {
        // If the screen has changed, do stack logic
        if (currScreen != _newScreen)
        {
            unityObjects["Screen_" + _newScreen].SetActive(true);
            unityObjects["Screen_" + currScreen].SetActive(false);
            screens_stack.Push(currScreen);
            currScreen = _newScreen;
        }

        // Initing game in case of game screen
        if (currScreen == SC_GlobalEnums.Screens.Game)
        {
            unityObjects["SC_GameLogic"].SetActive(true);
            SC_GameLogic.Instance.InitGame(gameMode);
        }
    }

    private void DoRoomsSearchLogic()
    {
        if (roomIndex < roomIds.Count)
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[roomIndex]);
        else
        {
            WarpClient.GetInstance().CreateTurnRoom("Domino" + System.DateTime.Now.Ticks.ToString(), SC_GlobalVariables.userId, 2, passedParams, 30);
        }
    }

    // Show a feedback message to the user at the bottom of the screen, the message is deleted after 5 seconds
    private IEnumerator ShowFeedback(string msg)
    {
        unityObjects["Txt_Feedback"].GetComponent<Text>().text = msg;
        yield return new WaitForSeconds(5);
        unityObjects["Txt_Feedback"].GetComponent<Text>().text = "";

    }

    #endregion

    #region CallBacks

    private void OnConnect(bool _IsSuccess)
    {
        if (_IsSuccess == true)
        {
            unityObjects["Btn_Multiplayer"].GetComponent<Button>().interactable = true;
            unityObjects["Txt_ServerStatus"].GetComponent<Text>().text = "Connect with ID:\n" + SC_GlobalVariables.userId;
        }
        else 
        {
            unityObjects["Btn_Multiplayer"].GetComponent<Button>().interactable = false;
            unityObjects["Txt_ServerStatus"].GetComponent<Text>().text = "Connection failed";
        }
    }

    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        //If we are still in the multiplayer screen
        if(_IsSuccess == true && currScreen == SC_GlobalEnums.Screens.Multiplayer)
        {
            roomIds = new List<string>();
            foreach(var roomData in eventObj.getRoomsData())
            {
                roomIds.Add(roomData.getId());
            }

            roomIndex = 0;
            DoRoomsSearchLogic();
        }

        else
        {
            unityObjects["Btn_StartMultiplayer"].GetComponent<Button>().interactable = true;
            unityObjects["Txt_MatchMaking"].GetComponent<Text>().text = "";
        }
    }

    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        if(_IsSuccess == true)
        {
            roomId = _RoomId;
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        else
        {
            unityObjects["Btn_StartMultiplayer"].GetComponent<Button>().interactable = true;
            unityObjects["Txt_MatchMaking"].GetComponent<Text>().text = "";
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if(_IsSuccess == false)
        {
            WarpClient.GetInstance().UnsubscribeRoom(roomId);
            roomIndex++;
            DoRoomsSearchLogic();
        }
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        //Check if the room's bet amount equals to my amount
        if(eventObj.getProperties() != null && eventObj.getProperties().ContainsKey("Password") && eventObj.getProperties()["Password"].ToString() == betValue.ToString())
        {
            roomId = eventObj.getData().getId();
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        //If there are more rooms to check, check next room,
        //If there are no rooms left to check, create a new room
        else
        {
            roomIndex++;
            DoRoomsSearchLogic();
        }
    }

    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        if(eventObj.getRoomOwner() == SC_GlobalVariables.userId && SC_GlobalVariables.userId != _UserName)
        {
            WarpClient.GetInstance().startGame();
        }
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        unityObjects["SC_GameLogic"].GetComponent<SC_GameLogic>().UpdateMultiplayerTurn(_NextTurn);
        ChangeScreen(SC_GlobalEnums.Screens.Game);

        unityObjects["Txt_Feedback"].GetComponent<Text>().text = "";
    }

    private void OnMoveCompleted(MoveEvent _Move)
    {
        SC_GameLogic.Instance.MoveCompleted(_Move);
    }

    private void OnChatReceived(ChatEvent eventObj)
    {
        // Used to sync the restart of the game between the users
        if (eventObj.getMessage() == "Restart" && eventObj.getSender() != SC_GlobalVariables.userId)
        {
            SC_GameLogic.Instance.opponentWantToRestart = true;
            StartCoroutine(ShowFeedback("Opponent ready"));
        }
    }

    private void OnUserLeftRoom(RoomData eventObj, string username)
    {
        ChangeScreen(SC_GlobalEnums.Screens.MainMenu);
        WarpClient.GetInstance().UnsubscribeRoom(roomId);
        WarpClient.GetInstance().LeaveRoom(roomId);
        if (GameObject.Find("Screen_GameOver") != null)
            GameObject.Find("Screen_GameOver").SetActive(false);

        StartCoroutine(ShowFeedback("Opponent left"));
    }
    #endregion
}
