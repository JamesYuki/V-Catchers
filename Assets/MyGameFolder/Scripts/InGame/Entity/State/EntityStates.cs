using UnityEngine;

namespace InGame.Entity.State
{
    /// <summary>
    /// 通常状態 - 全ての行動が可能
    /// </summary>
    public class NormalState : EntityState
    {
        public override EntityStateType StateType => EntityStateType.Normal;
        public override ActionCategory AllowedActions => ActionCategory.All;

        public override void OnEnter()
        {
            AppLogger.Log($"[{m_Controller?.gameObject.name}] Entered Normal State");
        }
    }

    /// <summary>
    /// 死亡状態 - 全ての行動が不可
    /// </summary>
    public class DeadState : EntityState
    {
        public override EntityStateType StateType => EntityStateType.Dead;
        public override ActionCategory AllowedActions => ActionCategory.None;

        public override void OnEnter()
        {
            AppLogger.Log($"[{m_Controller?.gameObject.name}] Entered Dead State");

            // 死亡時の共通処理
            // Rigidbodyの動きを止めるなど
            var rb = m_Controller?.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// スタン状態 - 全ての行動が不可、一定時間後に回復
    /// </summary>
    public class StunnedState : EntityState
    {
        private float m_StunDuration;
        private float m_StunTimer;

        public override EntityStateType StateType => EntityStateType.Stunned;
        public override ActionCategory AllowedActions => ActionCategory.None;

        public StunnedState(float duration = 1f)
        {
            m_StunDuration = duration;
        }

        public void SetDuration(float duration)
        {
            m_StunDuration = duration;
        }

        public override void OnEnter()
        {
            m_StunTimer = m_StunDuration;
            AppLogger.Log($"[{m_Controller?.gameObject.name}] Entered Stunned State for {m_StunDuration}s");

            // スタン時の共通処理
            var rb = m_Controller?.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public override void OnUpdate()
        {
            m_StunTimer -= Time.deltaTime;
            if (m_StunTimer <= 0f)
            {
                // スタン終了、通常状態に戻る
                m_StateMachine.ChangeState<NormalState>();
            }
        }
    }

    /// <summary>
    /// ノックバック状態 - 行動不可、ノックバック物理適用
    /// </summary>
    public class KnockbackState : EntityState
    {
        private Vector2 m_KnockbackForce;
        private float m_KnockbackDuration;
        private float m_KnockbackTimer;

        public override EntityStateType StateType => EntityStateType.Knockback;
        public override ActionCategory AllowedActions => ActionCategory.None;

        public void SetKnockback(Vector2 force, float duration = 0.3f)
        {
            m_KnockbackForce = force;
            m_KnockbackDuration = duration;
        }

        public override void OnEnter()
        {
            m_KnockbackTimer = m_KnockbackDuration;
            AppLogger.Log($"[{m_Controller?.gameObject.name}] Entered Knockback State");

            // ノックバック力を適用
            var rb = m_Controller?.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(m_KnockbackForce, ForceMode2D.Impulse);
            }
        }

        public override void OnUpdate()
        {
            m_KnockbackTimer -= Time.deltaTime;
            if (m_KnockbackTimer <= 0f)
            {
                m_StateMachine.ChangeState<NormalState>();
            }
        }
    }

    /// <summary>
    /// 無敵状態 - 全ての行動が可能、ダメージを受けない
    /// </summary>
    public class InvincibleState : EntityState
    {
        private float m_InvincibilityDuration;
        private float m_InvincibilityTimer;
        private SpriteRenderer m_SpriteRenderer;
        private float m_BlinkTimer;
        private float m_BlinkInterval = 0.1f;

        public override EntityStateType StateType => EntityStateType.Invincible;
        public override ActionCategory AllowedActions => ActionCategory.All;

        public void SetDuration(float duration, float blinkInterval = 0.1f)
        {
            m_InvincibilityDuration = duration;
            m_BlinkInterval = blinkInterval;
        }

        public override void OnEnter()
        {
            m_InvincibilityTimer = m_InvincibilityDuration;
            m_BlinkTimer = 0f;
            m_SpriteRenderer = m_Controller?.GetComponentInChildren<SpriteRenderer>();
            AppLogger.Log($"[{m_Controller?.gameObject.name}] Entered Invincible State for {m_InvincibilityDuration}s");
        }

        public override void OnUpdate()
        {
            m_InvincibilityTimer -= Time.deltaTime;

            // 点滅処理
            if (m_SpriteRenderer != null)
            {
                m_BlinkTimer += Time.deltaTime;
                if (m_BlinkTimer >= m_BlinkInterval)
                {
                    m_BlinkTimer = 0f;
                    m_SpriteRenderer.enabled = !m_SpriteRenderer.enabled;
                }
            }

            if (m_InvincibilityTimer <= 0f)
            {
                m_StateMachine.ChangeState<NormalState>();
            }
        }

        public override void OnExit()
        {
            // 点滅を解除
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.enabled = true;
            }
        }
    }
}
