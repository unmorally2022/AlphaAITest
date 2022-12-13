using UnityEngine;
using System.Collections;
namespace AlphaAITest.Characters.ThirdPerson
{
    public class ThirdPersonCharacterMP : ThirdPersonCharacter
    {
        [SerializeField]
        private TMPro.TMP_Text TMP_Text_Name;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void setName(string newName) {
            TMP_Text_Name.text = newName;
        }

        #region called_from_net
        public void setForwardAmount(float newForwardAmount)
        {
            m_ForwardAmount = newForwardAmount;
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        }

        public void setTurnAmount(float newTurnAmount)
        {
            m_TurnAmount = newTurnAmount;
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        }

        public void setOnGround(bool newOnGround)
        {
            m_IsGrounded = newOnGround;
            m_Animator.SetBool("OnGround", m_IsGrounded);
        }

        public void setJumpAmount(float newJumpAmount)
        {
            m_Animator.SetFloat("Jump", newJumpAmount);
        }
        #endregion
    }
}