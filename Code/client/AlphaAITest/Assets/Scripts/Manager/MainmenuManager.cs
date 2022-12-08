﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainmenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PanelAlert;
    [SerializeField]
    private Text TextAlert, TextAlertTitle;

    [SerializeField]
    private GameObject PanelLoading;
    [SerializeField]
    private Text TextLoading;

    [SerializeField]
    private InputField IF_username, IF_RoomNameToCreate, IF_RoomNameToJoin;

    // Start is called before the first frame update
    void Start()
    {
        PanelLoading.SetActive(false);
        PanelAlert.SetActive(false);

        AppManager.UserName = "plr" + AppManager.RandomString(4);
        IF_username.text = AppManager.UserName;

        showLoading("Connecting to server");
        AuthToServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void AuthToServer()
    {
        PlayerIOManager.onConnectedToServer = OnConnectedToServer;
        //PlayerIOManager.onDisconnectFromServer = OnDisconnecFromServer;
        PlayerIOManager.AuthToServer();
    }

    private void OnConnectedToServer()
    {
        hideLoading();        
    }

    private void showAlert(string title, string msg) {
        TextAlertTitle.text = title;
        TextAlert.text = msg;
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
            hideLoading();
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
            AppManager.LoadYourAsyncScene("GamePlay");
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
            AppManager.LoadYourAsyncScene("GamePlay");
            //no action here because server will broadcast player join, and it will process when received message
        };
        //PlayerIOManager.onDisconnectedFromRoom = OnDisconnectFromRoom;
        //PlayerIOManager.onCreateJoinRoomError = OnCreateJoinRoomError;
        PlayerIOManager.JoinRoomWithId(RoomId);
    }

    //GUI -------------------
    public void GUI_OKAlert() {
        PanelAlert.SetActive(false);
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
                showAlert("Game Name", "Game name cannot be empty, please give it name");
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

        }
    }
    //GUI END -------------------

    private bool checkName(string s) {
        string pattern = @"[\p{P}\p{S}-[._]]"; // added \p{S} to get ^,~ and ` (among others)
                                               //string test = @"_""'a:;%^&*~`bc!@#.,?";
        System.Text.RegularExpressions.MatchCollection mx = System.Text.RegularExpressions.Regex.Matches(s, pattern);

        if (s.Length <= 0)
        {
            showAlert("Wrong name", "You must give a name before play multiplayer");
            return false;
        }
        else if (s.Length > 10)
        {
            showAlert("Wrong name", "Name length cannot be more than 10, please correct");
            return false;
        }
        else if (s.Contains(" "))
        {
            showAlert("Wrong name", "Name cannot contains space, please correct");
            return false;
        }
        else if (mx.Count > 0)
        {
            showAlert("Wrong name", "Name cannot contains punctuation, please correct");            
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
                showAlert("Game Exist", "Game exist, use another game name");                
                break;
            case PlayerIOClient.ErrorCode.RoomIsFull:
                showAlert("Game Full", "Game full, join another game");                
                break;
            case PlayerIOClient.ErrorCode.MissingRoomId:
                showAlert("Game doesn't exist", "Cannot find game");                
                break;
            case PlayerIOClient.ErrorCode.UnknownRoom:
                showAlert("Game doesn't exist", "Game does't exist, try join another");                
                break;
            case PlayerIOClient.ErrorCode.UnknownRoomType:
                showAlert("Game doesn't exist", "Game does't exist, try join another type");                
                break;
            case PlayerIOClient.ErrorCode.NotASearchColumn:
                showAlert("Game doesn't exist", "Game does't exist, try join another game type");                
                break;
            default:
                Debug.Log(errorCode);
                showAlert("Unknown", "Unknown error");                
                break;
        }
    }
}