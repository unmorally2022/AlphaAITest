using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AlphaAITest.Characters.ThirdPerson;
using System.Linq;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private Transform CharactersWorldParent;

    [SerializeField]
    private ThirdPersonCharacterSP[] PrefabThirdPersonCharactersSP;
    [SerializeField]
    private ThirdPersonCharacterMP[] PrefabThirdPersonCharactersMP;

    [SerializeField]
    private ThirdPersonUserControlMP PrefabThirdPersonUserControlMP;

    //dictionary variable to hold each otehr player
    [SerializeField]
    private Dictionary<string, ThirdPersonUserControlMP> thirdPersonUserControlMPs;

    [SerializeField]
    private ThirdPersonUserControl thirdPersonUserControl;

    [SerializeField]
    private Transform[] SpawnPoint;

    [SerializeField]
    private UnityStandardAssets.Cameras.FreeLookCam freeLookCam;

    [SerializeField]
    private Text TextRoomStatus, TextTimeLeft;

    [SerializeField]
    private GameObject PanelCharSelection;

    private int selectedCharacter;

    //alert
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
    //alert end

    [SerializeField]
    private GameObject PanelChat;
    [SerializeField]
    private Text TextChat;
    [SerializeField]
    private InputField IFChat;

    [SerializeField]
    private GameObject PanelScore;
    [SerializeField]
    private Text TextScore;

    [SerializeField]
    private GameObject PrefabCoin;
    [SerializeField]
    private Transform CoinParent;

    [SerializeField]
    private Dictionary<int, GameObject> Coins;

    [SerializeField]
    private Transform[] CoinSpawnPoint;

    [SerializeField]
    private GameObject PanelResult;
    [SerializeField]
    private ResultChild1 PrefabResultChild1;
    [SerializeField]
    private Transform ResultChild1Parent;

    [SerializeField]
    private Animator[] AnimatorCharSelection;
    [SerializeField]
    private UnityStandardAssets.Cameras.LookAtCamera[] LookAtCameraSelection;

    private bool LeaveByPlayer = false;

    private void Awake()
    {
        AppManager.gameplayState = AppManager.GameplayState.iddle;
        thirdPersonUserControlMPs = new Dictionary<string, ThirdPersonUserControlMP>();
        Coins = new Dictionary<int, GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;

        LeaveByPlayer = false;

        //set delegate to listen to message
        PlayerIOManager.onMessage = OnMessage;
        PlayerIOManager.onDisconnectedFromRoom = OnDisconnectFromRoom;

        PanelCharSelection.SetActive(true);
        TextTimeLeft.gameObject.SetActive(false);
        PanelAlert.SetActive(false);
        PanelScore.SetActive(false);
        PanelResult.SetActive(false);

        thirdPersonUserControl.SetPlayerIOId(AppManager.PlayerIOid);
        thirdPersonUserControl.SetPlayerIOName(AppManager.PlayerIOName);

        TextRoomStatus.gameObject.SetActive(false);


        selectedCharacter = 0;
        GUI_SelectGender(selectedCharacter);

        //request other player
        PlayerIOManager.SendMsg("RequestPlayers");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void showAlert(string title, string msg, ButtonAlertOkClick newButtonAlertOkClick, ButtonAlertCancelClick newButtonAlertCancelClick)
    {
        TextAlertTitle.text = title;
        TextAlert.text = msg;

        if (newButtonAlertOkClick == null)
        {
            buttonOkAlert.SetActive(false);
        }
        else
        {
            buttonOkAlert.SetActive(true);
            buttonAlertOkClick = newButtonAlertOkClick;
        }

        if (newButtonAlertCancelClick == null)
        {
            buttonCancelAlert.SetActive(false);
        }
        else
        {
            buttonCancelAlert.SetActive(true);
            buttonAlertCancelClick = newButtonAlertCancelClick;
        }


        PanelAlert.SetActive(true);
    }

    private void InitForStartGame()
    {
        StartCoroutine(IE_HideTextRoomStatus());
        StartGame();
    }

    private IEnumerator IE_HideTextRoomStatus()
    {
        TextRoomStatus.text = "Game Started";
        yield return new WaitForSeconds(1);
        TextTimeLeft.gameObject.SetActive(true);
        TextRoomStatus.gameObject.SetActive(false);
    }

    private void StartGame()
    {
        PanelScore.SetActive(true);
        AppManager.gameplayState = AppManager.GameplayState.play;
    }

    public void updateScore(int newScore)
    {
        TextScore.text = string.Format("Score : {0}", newScore.ToString());
    }

    private void FinishGame()
    {
        TextTimeLeft.text = "00:00";
        PanelResult.SetActive(true);
        AppManager.gameplayState = AppManager.GameplayState.iddle;
        thirdPersonUserControl.characterState = AppManager.CharacterState.finish;
    }

    //playerio event
    private void OnMessage(PlayerIOClient.Message msg)
    {
        //if (PlayerIOManager.joinedroom)
        //{
        if (msg.Type == "RequestPlayers")
        {

            Debug.Log(msg);

            //if other player is ready then create controller and the object                
            //create thirdpersoncontrollermp
            if (thirdPersonUserControlMPs.ContainsKey(msg.GetString(0)) == false)
            {
                ThirdPersonUserControlMP newThirdPersonUserControlMP = Instantiate(PrefabThirdPersonUserControlMP);
                thirdPersonUserControlMPs.Add(msg.GetString(0), newThirdPersonUserControlMP);

                //ThirdPersonUserControlMP _thirdPersonUserControlMP = thirdPersonUserControlMPs[msg.GetString(0)].GetComponent<ThirdPersonUserControlMP>();
                //_thirdPersonUserControlMP.Init(CharactersWorldParent, msg.GetString(0), msg.GetString(1));

                newThirdPersonUserControlMP.Init(CharactersWorldParent, msg.GetString(0), msg.GetString(1));

                if (msg.GetBoolean(2) == true)
                {
                    //msg.Type = OthersJoined, 5 entries
                    //msg[0] = PL_AlphaAI_20221211055819(6)
                    //msg[1] = plr1c12(6)
                    //msg[2] = False(8)
                    //msg[3] = 0(0)
                    //msg[4] = 0(0)
                    //pl.Send("OthersJoined", pl1.UserId, pl1.name, pl1.isReady, pl1.itype, pl.SpawnPointIndex);                        
                    initThirdPersonUserControlMP(msg.GetString(0), msg.GetInt(3), msg.GetInt(4), msg.GetBoolean(2));
                }
                else
                {

                }
            }
            else
            {
                Debug.Log("thirdPersonUserControlMPs created");
            }

        }
        else if (msg.Type == "UpdateScore")
        {
            updateScore(msg.GetInt(0));
        }
        else if (msg.Type == "SetReady")
        {
            Debug.Log(msg);
            if (msg.GetString(0) == PlayerIOManager.PlayerIOid)
            {
                //create this character object
                //go to spawn point
                int spawnIndex = msg.GetInt(3);
                thirdPersonUserControl.m_Character.gameObject.transform.position = SpawnPoint[spawnIndex].transform.position;
                freeLookCam.transform.position = thirdPersonUserControl.m_Character.transform.position;
                if (msg.GetBoolean(2) == true)
                {
                    thirdPersonUserControl.characterState = AppManager.CharacterState.ready;
                }
                else
                {
                    thirdPersonUserControl.characterState = AppManager.CharacterState.notready;
                }
            }
            else
            {
                //create other character object
                //PlayerIOManager.SendMsg("SetReady", thirdPersonUserControl.m_Character.getiType(), true);
                //Broadcast("SetReady", player.ConnectUserId, player.itype, player.isReady, player.SpawnPointIndex);
                initThirdPersonUserControlMP(msg.GetString(0), msg.GetInt(1), msg.GetInt(3), msg.GetBoolean(2));
            }
        }
        else if (msg.Type == "WaitingOtherPlayer")
        {
            //Debug.Log("WaitingOtherPlayer");
            if (TextRoomStatus.gameObject.activeSelf)
                TextRoomStatus.text = "Waiting For Other Player \n" + msg.GetInt(0).ToString();
        }
        else if (msg.Type == "StartingGame")
        {
            //Debug.Log("StartingGame");
            TextRoomStatus.text = "Starting Game In " + msg.GetInt(0).ToString();
        }
        else if (msg.Type == "StartGame")
        {
            //Debug.Log("Start The Game");
            InitForStartGame();
        }
        else if (msg.Type == "InGameCountDown")
        {
            //Debug.Log("InGameCountDown");
            string stime = "";
            int Secconds = msg.GetInt(0);

            float minutes = Mathf.Floor(Secconds / 60);
            float seconds = Mathf.RoundToInt(Secconds % 60);

            if (minutes < 10)
                stime = "0" + minutes.ToString();
            else
                stime = minutes.ToString();
            if (seconds < 10)
                stime = stime + ":" + "0" + Mathf.RoundToInt(seconds).ToString();
            else
                stime = stime + ":" + Mathf.RoundToInt(seconds).ToString();

            TextTimeLeft.text = stime;

        }
        else if (msg.Type == "GameFinish")
        {
            FinishGame();
        }
        else if (msg.Type == "GameResult")
        {
            Debug.Log("GameResult");
            ResultChild1 resultChild1 = Instantiate(PrefabResultChild1, ResultChild1Parent);
            resultChild1.Init(msg.GetInt(1), msg.GetInt(4), msg.GetString(3));

            if (msg.GetInt(1) > 0 && msg.GetString(2) == thirdPersonUserControl.getPlayerIOId())
            {
                thirdPersonUserControl.m_Character.m_Animator.SetBool("Win", true);
            }
            else if (msg.GetInt(1) > 0 && msg.GetString(2) == thirdPersonUserControlMPs[msg.GetString(2)].getPlayerIOId())
            {
                thirdPersonUserControlMPs[msg.GetString(2)].m_CharacterMP.m_Animator.SetBool("Win", true);
            }
            //StopGame(msg);

        }
        else if (msg.Type == "PlayerLeft")
        {
            Debug.Log(string.Format("{0}:{1}", "PlayerLeft", msg));
            if (msg.GetString(0) != PlayerIOManager.PlayerIOid)
            {
                if (thirdPersonUserControlMPs.ContainsKey(msg.GetString(0)))
                {
                    if (thirdPersonUserControlMPs[msg.GetString(0)].m_CharacterMP != null)
                    {
                        Destroy(thirdPersonUserControlMPs[msg.GetString(0)].m_CharacterMP.gameObject);
                    }

                    //destroy the controller
                    Destroy(thirdPersonUserControlMPs[msg.GetString(0)].gameObject);

                    //remove from dictionary
                    thirdPersonUserControlMPs.Remove(msg.GetString(0));
                }
            }
        }
        else if (msg.Type == "CoinHit")
        {
            //hide coin
            if (msg.GetInt(1) == 0)
            {
                Coins[msg.GetInt(0)].SetActive(false);
            }
        }
        else if (msg.Type == "Coin")
        {
            //Debug.Log(msg);
            GameObject coin = Instantiate(PrefabCoin, CoinParent);
            coin.transform.GetChild(0).GetComponent<Coin>().id = msg.GetInt(0);
            coin.transform.position = CoinSpawnPoint[msg.GetInt(1)].position;
            if (msg.GetInt(2) == 1)
            {
                coin.SetActive(true);
            }

            Coins.Add(msg.GetInt(0), coin);
        }
        else if (msg.Type == "SendChat")
        {
            if (msg.GetString(0) == PlayerIOManager.PlayerIOid)
            {
                string playerIOName = thirdPersonUserControl.getPlayerIOName();
                addChat(string.Format("{0} : {1}", playerIOName, msg.GetString(1)));

                IFChat.Select();
                IFChat.ActivateInputField();
            }
            else
            {
                string playerIOName = thirdPersonUserControlMPs[msg.GetString(0)].getPlayerIOName();
                addChat(string.Format("{0} : {1}", playerIOName, msg.GetString(1)));
            }
        }
        else
        {
            ////Debug.Log(msg);
            if (msg.GetString(0) != PlayerIOManager.PlayerIOid)
            {
                if (thirdPersonUserControlMPs.ContainsKey(msg.GetString(0)))
                {
                    //ThirdPersonUserControlMP _thirdPersonUserControlMP1 = thirdPersonUserControlMPs[msg.GetString(0)].GetComponent<ThirdPersonUserControlMP>();
                    thirdPersonUserControlMPs[msg.GetString(0)].AddMessage(msg);
                }
            }
        }
        //}


    }

    private void initThirdPersonUserControlMP(string _playerIOId, int newiType, int newSpawnIndex, bool newIsReady)
    {
        selectedCharacter = newiType;
        //Debug.Log(_playerIOId);
        //ThirdPersonUserControlMP thirdPersonUserControlMP1 = thirdPersonUserControlMPs[_playerIOId];
        //Debug.Log(thirdPersonUserControlMP1);
        ThirdPersonUserControlMP _thirdPersonUserControlMP1 = thirdPersonUserControlMPs[_playerIOId];
        switch (selectedCharacter)
        {
            case 0:
                _thirdPersonUserControlMP1.m_CharacterMP = Instantiate(PrefabThirdPersonCharactersMP[0], CharactersWorldParent);
                break;
            case 1:
                _thirdPersonUserControlMP1.m_CharacterMP = Instantiate(PrefabThirdPersonCharactersMP[1], CharactersWorldParent);
                break;
        }

        //set position in spawn point
        _thirdPersonUserControlMP1.initPosition(SpawnPoint[newSpawnIndex].transform.position);

        _thirdPersonUserControlMP1.setName();

        //set state
        if (newIsReady)
        {
            _thirdPersonUserControlMP1.characterState = AppManager.CharacterState.ready;
        }
        else
        {
            _thirdPersonUserControlMP1.characterState = AppManager.CharacterState.notready;
        }
    }

    private void OnDisconnectFromRoom(object sender, string reason)
    {
        Debug.Log(string.Format("Disconnect From Game {0}", reason));
        if (LeaveByPlayer)
        {
            StartCoroutine(AppManager.LoadYourAsyncScene("MainMenu"));
        }
        else
        {
            showAlert("Disconnected", "Disconnected From Game", delegate { StartCoroutine(AppManager.LoadYourAsyncScene("MainMenu")); }, null);
        }
    }

    private void sendChat()
    {
        if (IFChat.text.Trim().Length > 0)
        {
            PlayerIOManager.SendMsg("SendChat", IFChat.text);
            IFChat.text = "";
        }
    }

    private void addChat(string chat)
    {
        TextChat.text = TextChat.text + "\n" + chat;
        int numLines = System.Text.RegularExpressions.Regex.Matches(TextChat.text, System.Environment.NewLine).Count;
        if (numLines > 15)
        {
            string str = TextChat.text;
            string output = str;
            while (numLines > 15)
            {
                string[] lines = str
                    .Split(System.Environment.NewLine.ToCharArray())
                    .Skip(1)
                    .ToArray();

                output = string.Join(System.Environment.NewLine, lines);
                numLines = System.Text.RegularExpressions.Regex.Matches(output, System.Environment.NewLine).Count;
                if (numLines > 15)
                    str = output;
            }
            TextChat.text = output;
        }
    }
    //playerio event end

    //GUI element
    public void GUI_SelectGender(int newSelectedGender)
    {
        selectedCharacter = newSelectedGender;
        for (int i = 0; i < AnimatorCharSelection.Length; i++)
        {
            if (i == selectedCharacter)
            {
                AnimatorCharSelection[i].SetBool("Selected", true);
                LookAtCameraSelection[i].enabled = true;
            }
            else
            {
                AnimatorCharSelection[i].SetBool("Selected", false);
                LookAtCameraSelection[i].enabled = false;
            }
        }
    }

    public void GUI_BtnReady()
    {
        //create character
        //selectedCharacter = Random.Range(0,1);


        switch (selectedCharacter)
        {
            case 0:
                thirdPersonUserControl.m_Character = Instantiate(PrefabThirdPersonCharactersSP[0], CharactersWorldParent);
                break;
            case 1:
                thirdPersonUserControl.m_Character = Instantiate(PrefabThirdPersonCharactersSP[1], CharactersWorldParent);
                break;
        }

        //set iType
        thirdPersonUserControl.m_Character.setiType(selectedCharacter);

        //set camera target
        freeLookCam.SetTarget(thirdPersonUserControl.m_Character.transform);
        freeLookCam.enabled = true;
        freeLookCam.transform.position = thirdPersonUserControl.m_Character.transform.position;
        //freeLookCam.setCursorLock(true);
        //PanelCharSelection.SetActive(false);        
        for (int i = 0; i < AnimatorCharSelection.Length; i++)
        {
            Destroy(AnimatorCharSelection[i].transform.parent.gameObject);
            Destroy(AnimatorCharSelection[i].gameObject);
            Destroy(LookAtCameraSelection[i].gameObject);
        }
        //foreach (Transform child in PanelCharSelection.transform)
        //{
        //    Destroy(child.gameObject);
        //}
        Destroy(PanelCharSelection.gameObject);


        TextRoomStatus.gameObject.SetActive(true);

        //send set ready to server; command, iType, ready stat        
        PlayerIOManager.SendMsg("SetReady", thirdPersonUserControl.m_Character.getiType(), true);


        //
    }

    public void GUI_OKAlert()
    {
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

    public void GUI_sendChat()
    {
        sendChat();
    }

    public void GUI_ExitGame()
    {
        showAlert("Asklamation", "Are you sure want to quit from game?", delegate
        {
            LeaveByPlayer = true;
            PlayerIOManager.LeaveRoom();
        }, delegate { });
    }

    public void GUI_Test1()
    {
        PlayerIOManager.SendMsg("test1");
    }

    public void GUI_Test2()
    {
        PlayerIOManager.SendMsg("test2");
    }

    public void GUI_Test3()
    {
        PlayerIOManager.SendMsg("test3");
    }


    //GUI element end;
}
