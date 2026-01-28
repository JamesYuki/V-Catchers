using UnityEngine;
using InGame.Entity;

namespace InGame.Player
{
    /// <summary>
    /// プレイヤーのHP管理モジュール
    /// </summary>
    public class PlayerHealthModule : PlayerModuleBase, IDamageable
    {
        [Header("HP設定")]
        [SerializeField] private HealthData m_HealthData;

        [Header("無敵時の点滅設定")]
        [SerializeField] private float m_BlinkInterval = 0.1f;

        private int m_CurrentHealth;
        private float m_InvincibilityTimer;
        private bool m_IsInvincible;
        private float m_BlinkTimer;
        private SpriteRenderer m_SpriteRenderer;

        /// <summary>
        /// HP変更時のイベント（現在HP, 最大HP）
        /// </summary>
        public event System.Action<int, int> OnHealthChanged;

        /// <summary>
        /// ダメージを受けた時のイベント（ダメージ量, ダメージ元）
        /// </summary>
        public event System.Action<int, GameObject> OnDamaged;

        /// <summary>
        /// 死亡時のイベント
        /// </summary>
        public event System.Action OnDeath;

        /// <summary>
        /// 回復時のイベント（回復量）
        /// </summary>
        public event System.Action<int> OnHealed;

        #region IDamageable Implementation

        public int CurrentHealth => m_CurrentHealth;
        public int MaxHealth => m_HealthData != null ? m_HealthData.MaxHealth : 100;
        public bool IsAlive => m_CurrentHealth > 0;
        public float HealthRatio => MaxHealth > 0 ? (float)m_CurrentHealth / MaxHealth : 0f;

        #endregion

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            InitializeHealth();
        }

        public override void StartModule()
        {
            // ServiceLocatorに登録してUIからアクセス可能に
            ServiceLocator.Register<PlayerHealthModule>(this);
        }

        public override void DestroyModule()
        {
            ServiceLocator.Unregister<PlayerHealthModule>();
        }

        private void InitializeHealth()
        {
            m_CurrentHealth = MaxHealth;
            m_InvincibilityTimer = 0f;
            m_IsInvincible = false;
        }

        public override void UpdateModule()
        {
            UpdateInvincibility();
        }

        private void UpdateInvincibility()
        {
            if (m_IsInvincible)
            {
                m_InvincibilityTimer -= Time.deltaTime;

                // 点滅処理
                m_BlinkTimer += Time.deltaTime;
                if (m_BlinkTimer >= m_BlinkInterval)
                {
                    m_BlinkTimer = 0f;
                    if (m_SpriteRenderer != null)
                    {
                        m_SpriteRenderer.enabled = !m_SpriteRenderer.enabled;
                    }
                }

                if (m_InvincibilityTimer <= 0f)
                {
                    m_IsInvincible = false;
                    OnInvincibilityEnd();
                }
            }
        }

        private void OnInvincibilityEnd()
        {
            // 点滅を解除して表示
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.enabled = true;
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage, GameObject damageSource = null)
        {
            if (!IsAlive || m_IsInvincible || damage <= 0)
            {
                return;
            }

            m_CurrentHealth = Mathf.Max(0, m_CurrentHealth - damage);

            OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
            OnDamaged?.Invoke(damage, damageSource);

            // ダメージエフェクト
            PlayDamageEffect();

            // 無敵時間開始
            StartInvincibility();

            // 死亡判定
            if (!IsAlive)
            {
                HandleDeath();
            }
        }

        /// <summary>
        /// 回復する
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0)
            {
                return;
            }

            int previousHealth = m_CurrentHealth;
            m_CurrentHealth = Mathf.Min(MaxHealth, m_CurrentHealth + amount);
            int actualHeal = m_CurrentHealth - previousHealth;

            if (actualHeal > 0)
            {
                OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
                OnHealed?.Invoke(actualHeal);
            }
        }

        private void StartInvincibility()
        {
            if (m_HealthData != null && m_HealthData.InvincibilityDuration > 0f)
            {
                m_IsInvincible = true;
                m_InvincibilityTimer = m_HealthData.InvincibilityDuration;
                m_BlinkTimer = 0f;
            }
        }

        private void PlayDamageEffect()
        {
            if (m_HealthData?.DamageEffectPrefab != null)
            {
                Instantiate(m_HealthData.DamageEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke();
            PlayDeathEffect();
            OnDeathProcess();
        }

        private void PlayDeathEffect()
        {
            if (m_HealthData?.DeathEffectPrefab != null)
            {
                Instantiate(m_HealthData.DeathEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
        private void OnDeathProcess()
        {
            // ゲームオーバー処理（必要に応じてGameManagerなどに通知）
            Debug.Log("[PlayerHealthModule] Player has died!");
            
            // TODO: GameManagerにゲームオーバーを通知
            // ServiceLocator.Service<GameManager>().OnPlayerDeath();
        }

        /// <summary>
        /// HPをリセット（リスポーン等で使用）
        /// </summary>
        public void ResetHealth()
        {
            InitializeHealth();
            OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
        }
    }
}
