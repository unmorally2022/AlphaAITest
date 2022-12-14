using System.Collections;
using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;

public static class PlayerIOManager
{
    private static string GameID = "alphaai-zjthdqzhfkc1xte5bc8vbg";

    private static Connection pioconnection;
    
    private static Client client;

    public delegate void OnMessageDelegate(int num);
    public static OnMessageDelegate onMessageDelegate;

    public delegate void OnConnectedToServer();
    public static OnConnectedToServer onConnectedToServer;

    public delegate void OnConnectToServerError(PlayerIOClient.ErrorCode errorCode);
    public static OnConnectToServerError onConnectToServerError;

    //public delegate void OnDisconnectFromServer(object sender, string reason);
    //public static OnDisconnectFromServer onDisconnectFromServer;

    // room 
    public delegate void OnJoinedRoom();
    public static OnJoinedRoom onJoinedRoom;

    public delegate void OnCreatedRoom(string CallbackSuccess);
    public static OnCreatedRoom onCreatedRoom;

    public delegate void OnCreateRoomError(PlayerIOClient.ErrorCode errorCode);
    public static OnCreateRoomError onCreateRoomError;

    public delegate void OnJoinRoomError(PlayerIOClient.ErrorCode errorCode);
    public static OnJoinRoomError onJoinRoomError;

    public delegate void OnCreateJoinRoomError(PlayerIOClient.ErrorCode errorCode);
    public static OnCreateJoinRoomError onCreateJoinRoomError;

    public delegate void OnDisconnectedFromRoom(object sender, string reason);
    public static OnDisconnectedFromRoom onDisconnectedFromRoom;

    public delegate void OnMessage(Message message);
    public static OnMessage onMessage;

    public static float SmoothNetworkInterpolation = 10;

    public static string PlayerIOid;
    
    static PlayerIOManager()
    {
        // Create a uniq user id from UTC time
        PlayerIOid = AppManager.PlayerIOid;
        Debug.Log(string.Format("Player IO User Id {0}", PlayerIOid));
        
        //userName = "Name" + System.DateTime.UtcNow.ToString(@"yyyyMddhhmmss");
    }

    public static void AuthToServer()
    {
        Debug.Log("Authenticating to server");

        PlayerIO.Authenticate(
            GameID,            //Your game id
            "public",                               //Your connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", PlayerIOid },
            },
            null,                                   //PlayerInsight segments
            delegate (Client _client)
            {
                Debug.Log("Successfully connected to Player.IO");
                client = _client;
                //debug to localhost
                //client.Multiplayer.DevelopmentServer = new ServerEndpoint("192.168.2.7", 8184);

                if (onConnectedToServer != null)
                    onConnectedToServer();
            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error connecting: " + error.ToString());
                if (onConnectToServerError != null)
                    onConnectToServerError(error.ErrorCode);
            }
        );
    }

    public static void CreateRoom(string RoomId)
    {
        client.Multiplayer.CreateRoom(
            RoomId,                    //Room id. If set to null a random roomid is used
            "Death Match",                   //The room type started on the server
            true,                               //Should the room be visible in the lobby?
            null,
            //new Dictionary<string, string> {
            //    { "RoomName", RoomName },
            //},
            delegate (string successCallback)
            {
                Debug.Log(successCallback);

                if (onCreatedRoom != null)
                    onCreatedRoom(successCallback);

            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error Creating Room: " + error.ErrorCode.ToString());
                if (onCreateRoomError != null)
                    onCreateRoomError(error.ErrorCode);
            }
        );

    }

    public static void JoinRoomWithId(string RoomID)
    {
        client.Multiplayer.JoinRoom(
                RoomID,                    //Room id.             
                new Dictionary<string, string> {
                { "userName", AppManager.PlayerIOName },
                },
                delegate (Connection connection)
                {
                    //Debug.Log("Joined Room.");				
                    // We successfully joined a room so set up the message handler
                    pioconnection = connection;
                    pioconnection.OnMessage += handlemessage;
                    pioconnection.OnDisconnect += handleDiscconect;
                    
                    if (onJoinedRoom != null)
                        onJoinedRoom();

                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error joining Room: " + error.ErrorCode.ToString());
                    if (onJoinRoomError != null)
                        onJoinRoomError(error.ErrorCode);
                }
            );
    }

    public static void JoinToRoom(string RoomName)
    {
        client.Multiplayer.ListRooms(
            "Death Match",
            new Dictionary<string, string> {
                { "RoomName", RoomName },
            },
            10,
            0,
            delegate (RoomInfo[] roomInfos)
            {
                if (roomInfos.Length > 0)
                {
                    Debug.Log(roomInfos[0].RoomData);

                    client.Multiplayer.JoinRoom(
                        roomInfos[0].Id,                    //Room id.             
                        new Dictionary<string, string> {
                        { "userName", AppManager.PlayerIOName },
                        },
                        delegate (Connection connection)
                        {
                            //Debug.Log("Joined Room.");				
                            // We successfully joined a room so set up the message handler
                            pioconnection = connection;
                            pioconnection.OnMessage += handlemessage;
                            pioconnection.OnDisconnect += handleDiscconect;
                            //joinedroom = true;

                            if (onJoinedRoom != null)
                                onJoinedRoom();

                        },
                        delegate (PlayerIOError error)
                        {
                            Debug.Log("Error joining Room: " + error.ErrorCode.ToString());
                            if (onJoinRoomError != null)
                                onJoinRoomError(error.ErrorCode);
                        }
                    );
                }
                else {
                    if (onJoinRoomError != null)
                        onJoinRoomError(ErrorCode.MissingRoomId);
                }
                //for (int i = 0; i < roomInfos.Length; i++)
                //{

                //    Debug.Log(roomInfos[i].RoomData);

                //    if (roomInfos[i].RoomData["RoomName"] == RoomName)
                //        RoomId = roomInfos[i].Id;
                //}

            }
            , delegate (PlayerIOError error)
            {
                Debug.Log("Error joining Room: " + error.ErrorCode.ToString());
                //roomInfosLength = -1;
                if (onJoinRoomError != null)
                    onJoinRoomError(ErrorCode.MissingRoomId);
            });

        //    _offset += 5;
        //}
    }

    public static void JoinRandomRoom() {
        //get list of room
        client.Multiplayer.ListRooms(
            "Death Match",
            null,
            10,
            0,
            delegate (RoomInfo[] roomInfos)
            {
                if (roomInfos.Length > 0)
                {
                    Debug.Log(roomInfos[0]);
                    Debug.Log(roomInfos[0].RoomData);

                    client.Multiplayer.JoinRoom(
                        roomInfos[Random.Range(0, roomInfos.Length)].Id,                    //Room id.             
                        new Dictionary<string, string> {
                        { "userName", AppManager.PlayerIOName},
                        },
                        delegate (Connection connection)
                        {
                            //Debug.Log("Joined Room.");				
                            // We successfully joined a room so set up the message handler
                            pioconnection = connection;
                            pioconnection.OnMessage += handlemessage;
                            pioconnection.OnDisconnect += handleDiscconect;
                            //joinedroom = true;

                            if (onJoinedRoom != null)
                                onJoinedRoom();

                        },
                        delegate (PlayerIOError error)
                        {
                            Debug.Log("Error joining Room: " + error.ErrorCode.ToString());
                            if (onJoinRoomError != null)
                                onJoinRoomError(error.ErrorCode);
                        }
                    );
                }
                else
                {
                    //create and join room
                    client.Multiplayer.CreateJoinRoom(
                        null,                    //Room id. If set to null a random roomid is used
                        "Death Match",                   //The room type started on the server
                        true,                               //Should the room be visible in the lobby?
                        null,
                        new Dictionary<string, string> {
                                { "userName", AppManager.PlayerIOName },
                        },
                        delegate (Connection connection)
                        {
                            //Debug.Log("Joined Room.");				
                            // We successfully joined a room so set up the message handler
                            pioconnection = connection;
                            pioconnection.OnMessage += handlemessage;
                            pioconnection.OnDisconnect += handleDiscconect;
                            //joinedroom = true;

                            if (onJoinedRoom != null)
                                onJoinedRoom();
                        },
                        delegate (PlayerIOError error)
                        {
                            Debug.Log("Error Joining Room: " + error.ErrorCode.ToString());
                            if (onCreateJoinRoomError != null)
                                onCreateJoinRoomError(error.ErrorCode);
                        }
                    );
                }

            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error joining Room: " + error.ErrorCode.ToString());
                //roomInfosLength = -1;
                if (onJoinRoomError != null)
                    onJoinRoomError(ErrorCode.MissingRoomId);
            });
    }

    private static void handlemessage(object sender, Message m)
    {
        //Debug.Log(string.Format("msg received : {0}:{1}",sender.ToString(), m.ToString()));
        //msgList.Add(m);
        if (onMessage != null)
        {
            //Debug.Log("onMessage");
            onMessage(m);
        }
    }

    private static void handleDiscconect(object sender, string reason)
    {
        Debug.Log(string.Format("Player IO Disconnected"));
        if (onDisconnectedFromRoom != null)
            onDisconnectedFromRoom(sender, reason);
    }

    public static void SendMsg(string _type, params object[] _parameter)
    {
        //Debug.Log("SendMsg");
        //if (_type == "Chat") {
        //	Debug.Log("send Chat");
        //}
        pioconnection.Send(_type, _parameter);
    }

    public static void LeaveRoom()
    {
        if(pioconnection != null)
        pioconnection.Disconnect();
    }

    public static bool getConnectedToRoom() {
        if (pioconnection == null)
        {
            return false;
        }
        else {
            return pioconnection.Connected;
        }
        
    }

    public static bool isConnectedToserver() {
        if (pioconnection == null)
            return false;
        else
            return pioconnection.Connected;
    }
    
}
