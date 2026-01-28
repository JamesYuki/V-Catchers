using UnityEngine;
using InGame.Entity;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵のHP管理モジュール
    /// </summary>
    public class EnemyHealthModule : EnemyModuleBase, IDamageable
    {
        [Header("HP設定")]
        [SerializeField] private HealthData m_HealthData;

        [Header("ダメージ時の色変更")]
        [SerializeField] private Color m_DamageColor = Color.red;
        [SerializeField] private float m_DamageColorDuration = 0.1f;

        [Header("死亡設定")]
        [SerializeField] private float m_DestroyDelay = 0.5f;
        [SerializeField] private bool m_DestroyOnDeath = true;

        private int m_CurrentHealth;
        private float m_InvincibilityTimer;
        private bool m_IsInvincible;
        private SpriteRenderer m_SpriteRenderer;
        private Color m_OriginalColor;
        private float m_DamageColorTimer;

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

        #region IDamageable Implementation

        public int CurrentHealth => m_CurrentHealth;
        public int MaxHealth => m_HealthData != null ? m_HealthData.MaxHealth : 50;
        public bool IsAlive => m_CurrentHealth > 0;
        public float HealthRatio => MaxHealth > 0 ? (float)m_CurrentHealth / MaxHealth : 0f;

        #endregion

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (m_SpriteRenderer != null)
            {
                m_OriginalColor = m_SpriteRenderer.color;
            }
            InitializeHealth();
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
            UpdateDamageColor();
        }

        private void UpdateInvincibility()
        {
            if (m_IsInvincible)
            {
                m_InvincibilityTimer -= Time.deltaTime;
                if (m_InvincibilityTimer <= 0f)
                {
                    m_IsInvincible = false;
                }
            }
        }

        private void UpdateDamageColor()
        {
            if (m_DamageColorTimer > 0f)
            {
                m_DamageColorTimer -= Time.deltaTime;
                if (m_DamageColorTimer <= 0f && m_SpriteRenderer != null)
                {
                    m_SpriteRenderer.color = m_OriginalColor;
                }
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

            // ダメージ演出
            PlayDamageEffect();
            ShowDamageColor();

            // 無敵時間開始
            StartInvincibility();

            // 死亡判定
            if (!IsAlive)
            {
                HandleDeath();
            }
        }

        private void ShowDamageColor()
        {
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_DamageColor;
                m_DamageColorTimer = m_DamageColorDuration;
            }
        }

        private void StartInvincibility()
        {
            if (m_HealthData != null && m_HealthData.InvincibilityDuration > 0f)
            {
                m_IsInvincible = true;
                m_InvincibilityTimer = m_HealthData.InvincibilityDuration;
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
        /// 敵死亡時の処理
        /// </summary>
        private void OnDeathProcess()
        {
            // ステートマシンを死亡状態に
            m_EnemyController?.OnDeath();

            Debug.Log($"[EnemyHealthModule] {gameObject.name} has died!");

            // TODO: スコア加算、ドロップアイテム生成など
            // ServiceLocator.Service<ScoreManager>().AddScore(enemy.ScoreValue);

            if (m_DestroyOnDeath)
            {
                // Colliderを無効化して当たり判定をなくす
                var colliders = GetComponents<Collider2D>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }

                // 遅延してGameObject削除
                Destroy(gameObject, m_DestroyDelay);
            }
        }

        /// <summary>
        /// HPをリセット
        /// </summary>
        public void ResetHealth()
        {
            InitializeHealth();
            m_EnemyController?.OnRespawn();
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_OriginalColor;
            }
            OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// 即死させる
        /// </summary>
        public void Kill()
        {
            bool wasInvincible = m_IsInvincible;
            m_IsInvincible = false;
            TakeDamage(m_CurrentHealth);
        }
    }
}
