using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainmenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PanelRoom;

    [SerializeField]
    private GameObject PanelAlert;
    [SerializeField]
    private Text TextAlert, TextAlertTitle;
    private delegate void ButtonAlertOkClick();
    private ButtonAlertOkClick buttonAlertOkClick;
    private delegate void ButtonAlertCancelClick();
    private ButtonAlertCancelClick buttonAlertCancelClick;
    [SerializeField]
    private GameObject buttonOkAlert, buttonCancelAlert;


    [SerializeField]
    private GameObject PanelLoading;
    [SerializeField]
    private Text TextLoading;

    private int iType;

    [SerializeField]
    private InputField IF_username, IF_RoomNameToCreate, IF_RoomNameToJoin;

    // Start is called before the first frame update
    void Start()
    {
        PanelLoading.SetActive(false);
        PanelAlert.SetActive(false);

        AppManager.PlayerIOName = "plr" + AppManager.RandomString(4);
        IF_username.text = AppManager.PlayerIOName;

        showLoading("Connecting to server");
        AuthToServer();


        //debug
        iType = 0;//female
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void AuthToServer()
    {
        PlayerIOManager.onConnectedToServer = OnConnectedToServer;
        PlayerIOManager.onConnectToServerError = delegate(PlayerIOClient.ErrorCode errorCode) {
            hideLoading();
            PanelRoom.SetActive(false);            
            showAlert("Error", errorCode.ToString(), delegate () { }, null);
    	};
        //PlayerIOManager.onDisconnectFromServer = OnDisconnecFromServer;
        PlayerIOManager.AuthToServer();
    }

    private void OnConnectedToServer()
    {
        hideLoading();        
    }

    private void showAlert(string title, string msg, ButtonAlertOkClick newButtonAlertOkClick, ButtonAlertCancelClick newButtonAlertCancelClick) {
        TextAlertTitle.text = title;
        TextAlert.text = msg;

        if (newButtonAlertOkClick == null) {
            buttonOkAlert.SetActive(false);
        }
        else {
            buttonOkAlert.SetActive(true);
            buttonAlertOkClick = newButtonAlertOkClick;
        }

        if (newButtonAlertCancelClick == null)
        {
            buttonCancelAlert.SetActive(false);
        }
        else {
            buttonCancelAlert.SetActive(true);
            buttonAlertCancelClick = newButtonAlertCancelClick;
        }
            

        PanelAlert.SetActive(true);
    }

    private void showLoading(string msg)
    {
        TextLoading.text = msg;
        PanelLoading.SetActive(true);
    }

    private void hideLoading() {
        PanelLoading.SetActive(false);
    }

    private void CreateRoom(string RoomId)
    {
        showLoading("Joining Game " + RoomId);       

        PlayerIOManager.onCreateRoomError = delegate (PlayerIOClient.ErrorCode errorCode)
        {
            hideLoading();
            RoomErrorMessage(errorCode);
        };
        PlayerIOManager.onCreatedRoom = delegate (string SuccessCallback)
        {            
            JoinRoomWithId(RoomId);
        };
        PlayerIOManager.CreateRoom(RoomId);
    }

    private void JoinRandomRoom()
    {
        showLoading( "Joining Game");
        
        PlayerIOManager.onJoinRoomError = delegate (PlayerIOClient.ErrorCode errorCode)
        {
            hideLoading();
            RoomErrorMessage(errorCode);
        };
        PlayerIOManager.onJoinedRoom = delegate ()
        {            
            StartCoroutine(  AppManager.LoadYourAsyncScene("GamePlay"));
            //no action here because server will broadcast player join, and it will process when received message
        };
        //PlayerIOManager.onDisconnectedFromRoom = OnDisconnectFromRoom;
        //PlayerIOManager.onCreateJoinRoomError = OnCreateJoinRoomError;
        PlayerIOManager.JoinRandomRoom();
    }

    private void JoinRoomWithId(string RoomId)
    {
        showLoading("Joining Game " + RoomId);
        
        PlayerIOManager.onJoinRoomError = delegate (PlayerIOClient.ErrorCode errorCode)
        {
            hideLoading();
            RoomErrorMessage(errorCode);
        };
        PlayerIOManager.onJoinedRoom = delegate ()
        {
            StartCoroutine(AppManager.LoadYourAsyncScene("GamePlay"));
            //no action here because server will broadcast player join, and it will process when received message
        };
        //PlayerIOManager.onDisconnectedFromRoom = OnDisconnectFromRoom;
        //PlayerIOManager.onCreateJoinRoomError = OnCreateJoinRoomError;
        PlayerIOManager.JoinRoomWithId(RoomId);
    }

    //GUI -------------------
    public void GUI_ExitGame()
    {
        showAlert("Asklamation","Are you sure want to quit game?", delegate {
            Debug.Log("quitgame");
            Application.Quit();
        },delegate { });        
    }

    public void GUI_OKAlert() {
        PanelAlert.SetActive(false);
        if (buttonAlertOkClick != null)
            buttonAlertOkClick();
    }

    public void GUI_NoAlert()
    {
        PanelAlert.SetActive(false);
        if (buttonAlertCancelClick != null)
            buttonAlertCancelClick();
    }

    public void GUI_JoinRandom() {
        if (checkName(IF_username.text))
        {
            JoinRandomRoom();
        }
    }
    public void GUI_CreateJoinRoom() {
        if (checkName(IF_username.text))
        {
            if (IF_RoomNameToCreate.text.Length <= 0)
            {                
                showAlert("Game Name", "Game name cannot be empty, please give it name", delegate () { }, null);
            }
            else
            {
                CreateRoom(IF_RoomNameToCreate.text);
            }
        }
    }
    public void GUI_JoinRoom() {
        if (checkName(IF_username.text))
        {
            JoinRoomWithId(IF_RoomNameToJoin.text);
        }
    }
    //GUI END -------------------

    private bool checkName(string s) {
        string pattern = @"[\p{P}\p{S}-[._]]"; // added \p{S} to get ^,~ and ` (among others)
                                               //string test = @"_""'a:;%^&*~`bc!@#.,?";
        System.Text.RegularExpressions.MatchCollection mx = System.Text.RegularExpressions.Regex.Matches(s, pattern);

        if (s.Length <= 0)
        {            
            showAlert("Wrong name", "You must give a name before play multiplayer", delegate () { }, null);
            return false;
        }
        else if (s.Length > 10)
        {
            showAlert("Wrong name", "Name length cannot be more than 10, please correct", delegate () { }, null);
            return false;
        }
        else if (s.Contains(" "))
        {
            showAlert("Wrong name", "Name cannot contains space, please correct", delegate () { }, null);
            return false;
        }
        else if (mx.Count > 0)
        {
            showAlert("Wrong name", "Name cannot contains punctuation, please correct", delegate () { }, null);            
            return false;
        }
        else
        {
            return true;
        }
    }

    private void RoomErrorMessage(PlayerIOClient.ErrorCode errorCode)
    {
        switch (errorCode)
        {
            case PlayerIOClient.ErrorCode.RoomAlreadyExists:
                showAlert("Game Exist", "Game exist, use another game name", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.RoomIsFull:
                showAlert("Game Full", "Game full, join another game", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.MissingRoomId:
                showAlert("Game doesn't exist", "Cannot find game", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.UnknownRoom:
                showAlert("Game doesn't exist", "Game does't exist, try join another", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.UnknownRoomType:
                showAlert("Game doesn't exist", "Game does't exist, try join another type", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.NotASearchColumn:
                showAlert("Game doesn't exist", "Game does't exist, try join another game type", delegate () { }, null);                
                break;
            case PlayerIOClient.ErrorCode.NoServersAvailable:
                showAlert("Game doesn't exist", "No Server available at this time", delegate () { }, null);
                break;            
            default:
                Debug.Log(errorCode);
                showAlert("Unknown", "Unknown error", delegate () { }, null);                
                break;
        }
    }
}
