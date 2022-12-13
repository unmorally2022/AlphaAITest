using System;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

namespace AlphaAITest.Characters.ThirdPerson
{
    //[RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        
        public ThirdPersonCharacterSP m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.


        public AppManager.CharacterState characterState;
        //[SerializeField]
        private string PlayerIOId, PlayerIOName;

        private InputField IFChat;
        private GamePlayManager gamePlayManager;


        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            //get input field to ignore input when input field is focus
            IFChat = GameObject.Find("InputField Chat").GetComponent<InputField>();
            gamePlayManager = GameObject.Find("Manager").GetComponent<GamePlayManager>();
            // get the third person character ( this should never be null due to require component )
            //m_Character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {
            if (characterState == AppManager.CharacterState.ready && AppManager.gameplayState == AppManager.GameplayState.play)
            {
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            if (IFChat.isFocused == true)
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit1"))
                {
                    gamePlayManager.GUI_sendChat();
                    //UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(IFChat.gameObject);
                    //IFChat.Select();
                    Debug.Log("IFChat focus");
                }
                //if (Input.GetKeyDown(KeyCode.Return))
                //{
                //    gamePlayManager.GUI_sendChat();
                //    Debug.Log("IFChat focus");
                //}
            }

            if (characterState == AppManager.CharacterState.ready && AppManager.gameplayState == AppManager.GameplayState.play)
            {
                //send movement to server
                SendMovement();

                if (IFChat.isFocused == true)
                {
                    ////if (CrossPlatformInputManager.GetButtonUp("return")) {                        
                        
                    ////}
                    //if (Input.GetKeyUp(KeyCode.Return)) {
                    //    gamePlayManager.GUI_sendChat();
                    //    Debug.Log("IFChat focus");
                    //}
                }
                else
                {
                    //Debug.Log("IFChat not focus");
                    // read inputs
                    float h = CrossPlatformInputManager.GetAxis("Horizontal");
                    float v = CrossPlatformInputManager.GetAxis("Vertical");

                    // calculate move direction to pass to character
                    if (m_Cam != null)
                    {
                        // calculate camera relative direction to move:
                        m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                        m_Move = v * m_CamForward + h * m_Cam.right;
                    }
                    else
                    {
                        // we use world-relative directions in the case of no main camera
                        m_Move = v * Vector3.forward + h * Vector3.right;
                    }

                    // pass all parameters to the character control script
                    m_Character.Move(m_Move, m_Jump);
                    m_Jump = false;
                }
            }
        }

        private void SendMovement()
        {
            if (PlayerIOId == PlayerIOManager.PlayerIOid)
            {
                PlayerIOManager.SendMsg("Move",
                    m_Character.transform.position.x,
                    m_Character.gameObject.transform.position.y,
                    m_Character.gameObject.transform.position.z,
                    m_Character.gameObject.transform.eulerAngles.x,
                    m_Character.gameObject.transform.eulerAngles.y,
                    m_Character.gameObject.transform.eulerAngles.z,
                    m_Character.m_Animator.GetFloat("Forward"),
                    m_Character.m_Animator.GetFloat("Turn"),
                    m_Character.m_Animator.GetBool("OnGround"),
                    m_Character.m_Animator.GetFloat("Jump")
                    );
            }
        }

        public void SetPlayerIOId(string newPlayerIOId)
        {
            PlayerIOId = newPlayerIOId;
        }

        public void SetPlayerIOName(string newPlayerIOName)
        {
            PlayerIOName = newPlayerIOName;
        }

        public string getPlayerIOId()
        {
            return PlayerIOId;
        }

        public string getPlayerIOName() {
            return PlayerIOName;
        }
    }
}
