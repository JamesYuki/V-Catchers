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

        [Header("接地判定設定")]
        [SerializeField]
        private GroundChecker m_GroundChecker;
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

            if (m_PlayerInputModule.GetJump() && m_GroundChecker != null && m_GroundChecker.IsGrounded)
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