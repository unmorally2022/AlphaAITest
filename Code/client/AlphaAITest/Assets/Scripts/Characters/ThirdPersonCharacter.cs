using UnityEngine;

namespace AlphaAITest.Characters.ThirdPerson
{
    //[RequireComponent(typeof(Rigidbody))]
    //[RequireComponent(typeof(CapsuleCollider))]
    //[RequireComponent(typeof(Animator))]
    public class ThirdPersonCharacter : MonoBehaviour
    {
        [SerializeField] protected float m_MovingTurnSpeed = 360;
        [SerializeField] protected float m_StationaryTurnSpeed = 180;
        [SerializeField] protected float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] protected float m_GravityMultiplier = 2f;
        [SerializeField] protected float m_MoveSpeedMultiplier = 1f;
        [SerializeField] protected float m_AnimSpeedMultiplier = 1f;
        [SerializeField] protected float m_GroundCheckDistance = 0.1f;

        protected Rigidbody m_Rigidbody;
        public Animator m_Animator;
        protected bool m_IsGrounded;
        protected float m_OrigGroundCheckDistance;
        protected const float k_Half = 0.5f;
        protected float m_TurnAmount;
        protected float m_ForwardAmount;
        protected Vector3 m_GroundNormal;
        //float m_CapsuleHeight;
        //Vector3 m_CapsuleCenter;
        protected CapsuleCollider m_Capsule;
        //bool m_Crouching;

        /// <>
        [SerializeField]
        private int iType;
        /// </>


        public void setiType(int newiType){
            iType = newiType;
        }

        public int getiType()
        {
            return iType;
        }

    }


}
