using UnityEngine;
using InGame.Entity;

namespace InGame.Player
{
    public class PlayerMovementModule : PlayerModuleBase
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

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            m_Rigidbody2D = m_PlayerController.GetComponent<Rigidbody2D>();
            m_PlayerController.TryGetModule(out m_PlayerInputModule);
        }

        public override void StartModule()
        {
        }

        public override void UpdateModule()
        {
            // 行動不可状態なら移動入力を無視
            if (!m_PlayerController.CanPerformAction(Entity.ActionCategory.Movement))
            {
                m_MoveInput = 0f;
                return;
            }

            m_MoveInput = m_PlayerInputModule.GetMove().x;

            if (m_PlayerController.CanPerformAction(Entity.ActionCategory.Jump))
            {
                if (m_PlayerInputModule.GetJump() && m_GroundChecker != null && m_GroundChecker.IsGrounded)
                {
                    m_Rigidbody2D.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
                }
            }
        }

        public override void FixedUpdateModule()
        {
            // 死亡状態でも物理は動かす（倒れるアニメーション用）
            if (!m_PlayerController.CanPerformAction(Entity.ActionCategory.Movement))
            {
                // 横移動だけ止める
                m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
                return;
            }

            m_Rigidbody2D.linearVelocity = new Vector2(m_MoveInput * m_Speed, m_Rigidbody2D.linearVelocity.y);
        }

        public override void DestroyModule()
        {
            m_Rigidbody2D = null;
        }
    }
}