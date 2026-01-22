using UnityEngine;
namespace InGame.Player
{
    public class PlayerMovementModule : MonoBehaviour, IPlayerModule
    {
        private Rigidbody2D m_Rigidbody2D;
        private PlayerInputModule m_PlayerInputModule;

        [SerializeField, Header("移動速度")]
        private float m_Speed = 5f;

        [SerializeField, Header("ジャンプ力")]
        private float m_JumpForce = 10f;

        [SerializeField, Header("地面判定距離")]
        private float m_GroundCheckDistance = 0.2f;

        [SerializeField, Header("地面レイヤー")]
        private LayerMask m_GroundLayer;

        [Header("接地判定設定")]
        public Transform m_GroundCheck;
        private bool m_IsGrounded = false;
        private float m_MoveInput = 0f;

        public void Setup(PlayerController playerController)
        {
            m_Rigidbody2D = playerController.GetComponent<Rigidbody2D>();
            playerController.GetModule(out m_PlayerInputModule);
        }

        public void StartModule()
        {

        }

        public void UpdateModule()
        {
            m_MoveInput = m_PlayerInputModule.GetMove().x;

            m_IsGrounded = Physics2D.OverlapCircle(m_GroundCheck.position, m_GroundCheckDistance, m_GroundLayer);

            if (m_PlayerInputModule.GetJump() && m_IsGrounded)
            {
                m_Rigidbody2D.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
            }
        }

        public void FixedUpdateModule()
        {
            m_Rigidbody2D.linearVelocity = new Vector2(m_MoveInput * m_Speed, m_Rigidbody2D.linearVelocity.y);
        }
        public void DestroyModule()
        {
            m_Rigidbody2D = null;
        }
    }
}