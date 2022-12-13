using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace AlphaAITest.Characters.ThirdPerson
{
    //[RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControlMP : MonoBehaviour
    {

        public ThirdPersonCharacterMP m_CharacterMP; // A reference to the ThirdPersonCharacter on the object
        //private Vector3 m_Move;
        //private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

        public AppManager.CharacterState characterState;
        [SerializeField]
        private string PlayerIOId, PlayerIOName;

        [SerializeField]
        private Queue<PlayerIOClient.Message> msgStack = new Queue<PlayerIOClient.Message>();

        [SerializeField]
        private GameObject ObjectToFollow;

        [SerializeField]
        private GamePlayManager gamePlayManager;

        private void Start()
        {
        }


        private void Update()
        {
        }

        public string getPlayerIOId()
        {
            return PlayerIOId;
        }

        public string getPlayerIOName()
        {
            return PlayerIOName;
        }

        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            //process message from net
            ProcessMessage();
            if (characterState != AppManager.CharacterState.notready)
            {
                m_CharacterMP.transform.position = Vector3.Lerp(m_CharacterMP.transform.position, ObjectToFollow.transform.position, Time.deltaTime * PlayerIOManager.SmoothNetworkInterpolation);

                Quaternion qr = Quaternion.Euler(ObjectToFollow.transform.eulerAngles);
                m_CharacterMP.transform.rotation = Quaternion.Lerp(m_CharacterMP.transform.rotation, qr, Time.deltaTime * PlayerIOManager.SmoothNetworkInterpolation);
            }
        }

        public void Init(Transform TransformParent, string newPlayerIOid, string newPlayerIOName)
        {
            PlayerIOId = newPlayerIOid;
            PlayerIOName = newPlayerIOName;

            ObjectToFollow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ObjectToFollow.GetComponent<BoxCollider>().enabled = false;            
            ObjectToFollow.GetComponent<MeshRenderer>().enabled = false;
            ObjectToFollow.transform.SetParent(TransformParent);
        }

        public void initPosition(Vector3 newPosition) {
            ObjectToFollow.transform.position = newPosition;
        }

        public void setName()
        {
            m_CharacterMP.setName(PlayerIOName);
        }

        public void AddMessage(PlayerIOClient.Message msg)
        {
            msgStack.Enqueue(msg);
        }

        private void ProcessMessage()
        {
            if (msgStack.Count > 0)
            {
                System.DateTime startTime = System.DateTime.UtcNow;
                System.TimeSpan ts = System.DateTime.UtcNow - startTime;
                while (msgStack.Count > 0 && ts.Milliseconds < 2)
                {
                    PlayerIOClient.Message msg = msgStack.Dequeue();

                    switch (msg.Type)
                    {
                        case "Move":
                            SetPos(new Vector3(msg.GetFloat(1), msg.GetFloat(2), msg.GetFloat(3)));
                            SetRotate(new Vector3(msg.GetFloat(4), msg.GetFloat(5), msg.GetFloat(6)));
                            m_CharacterMP.setForwardAmount(msg.GetFloat(7));
                            m_CharacterMP.setTurnAmount(msg.GetFloat(8));
                            m_CharacterMP.setOnGround(msg.GetBoolean(9));
                            m_CharacterMP.setJumpAmount(msg.GetFloat(10));
                            break;                        
                        //case "Chat":
                        //    Debug.Log("chat");
                        //    break;
                    }
                    //Debug.Log(string.Format("miliseccond {0}",ts.Milliseconds));
                    ts = System.DateTime.UtcNow - startTime;
                }
            }
        }

        //called from other player via network
        private void SetPos(Vector3 position)
        {            
            ObjectToFollow.transform.position = position;
        }

        private void SetRotate(Vector3 rotation)
        {
            ObjectToFollow.transform.eulerAngles = new Vector3(rotation.x, rotation.y, rotation.z);
        }
       
    }
}
