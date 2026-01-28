using System;
using UnityEngine;

namespace InGame.Entity
{
    /// <summary>
    /// Entity共通のHP管理モジュール基底クラス
    /// </summary>
    public abstract class EntityHealthModule : EntityModuleBase, IDamageable
    {
        [Header("HP設定")]
        [SerializeField] protected HealthData m_HealthData;

        protected int m_CurrentHealth;
        protected float m_InvincibilityTimer;
        protected bool m_IsInvincible;

        /// <summary>
        /// HP変更時のイベント（現在HP, 最大HP）
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// ダメージを受けた時のイベント（ダメージ量, ダメージ元）
        /// </summary>
        public event Action<int, GameObject> OnDamaged;

        /// <summary>
        /// 死亡時のイベント
        /// </summary>
        public event Action OnDeath;

        /// <summary>
        /// 回復時のイベント（回復量）
        /// </summary>
        public event Action<int> OnHealed;

        #region IDamageable Implementation

        public int CurrentHealth => m_CurrentHealth;
        public int MaxHealth => m_HealthData != null ? m_HealthData.MaxHealth : 100;
        public bool IsAlive => m_CurrentHealth > 0;
        public float HealthRatio => MaxHealth > 0 ? (float)m_CurrentHealth / MaxHealth : 0f;

        #endregion

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            InitializeHealth();
        }

        /// <summary>
        /// HPの初期化
        /// </summary>
        protected virtual void InitializeHealth()
        {
            m_CurrentHealth = MaxHealth;
            m_InvincibilityTimer = 0f;
            m_IsInvincible = false;
        }

        public override void UpdateModule()
        {
            UpdateInvincibility();
        }

        /// <summary>
        /// 無敵時間の更新
        /// </summary>
        protected virtual void UpdateInvincibility()
        {
            if (m_IsInvincible)
            {
                m_InvincibilityTimer -= Time.deltaTime;
                if (m_InvincibilityTimer <= 0f)
                {
                    m_IsInvincible = false;
                    OnInvincibilityEnd();
                }
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public virtual void TakeDamage(int damage, GameObject damageSource = null)
        {
            if (!IsAlive || m_IsInvincible || damage <= 0)
            {
                return;
            }

            // ダメージ適用
            int actualDamage = CalculateDamage(damage, damageSource);
            m_CurrentHealth = Mathf.Max(0, m_CurrentHealth - actualDamage);

            // イベント発火
            OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
            OnDamaged?.Invoke(actualDamage, damageSource);

            // エフェクト再生
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
        public virtual void Heal(int amount)
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

        /// <summary>
        /// HPを全回復
        /// </summary>
        public virtual void FullHeal()
        {
            Heal(MaxHealth - m_CurrentHealth);
        }

        /// <summary>
        /// ダメージ計算（オーバーライドで防御力等を考慮可能）
        /// </summary>
        protected virtual int CalculateDamage(int baseDamage, GameObject damageSource)
        {
            return baseDamage;
        }

        /// <summary>
        /// 無敵時間を開始
        /// </summary>
        protected virtual void StartInvincibility()
        {
            if (m_HealthData != null && m_HealthData.InvincibilityDuration > 0f)
            {
                m_IsInvincible = true;
                m_InvincibilityTimer = m_HealthData.InvincibilityDuration;
                OnInvincibilityStart();
            }
        }

        /// <summary>
        /// 無敵開始時の処理（オーバーライド用）
        /// </summary>
        protected virtual void OnInvincibilityStart() { }

        /// <summary>
        /// 無敵終了時の処理（オーバーライド用）
        /// </summary>
        protected virtual void OnInvincibilityEnd() { }

        /// <summary>
        /// ダメージエフェクトの再生
        /// </summary>
        protected virtual void PlayDamageEffect()
        {
            if (m_HealthData?.DamageEffectPrefab != null)
            {
                Instantiate(m_HealthData.DamageEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        protected virtual void HandleDeath()
        {
            OnDeath?.Invoke();
            PlayDeathEffect();
            OnDeathProcess();
        }

        /// <summary>
        /// 死亡エフェクトの再生
        /// </summary>
        protected virtual void PlayDeathEffect()
        {
            if (m_HealthData?.DeathEffectPrefab != null)
            {
                Instantiate(m_HealthData.DeathEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// 死亡時の具体的な処理（オーバーライド必須）
        /// </summary>
        protected abstract void OnDeathProcess();

        /// <summary>
        /// HPをリセット（リスポーン等で使用）
        /// </summary>
        public virtual void ResetHealth()
        {
            InitializeHealth();
            OnHealthChanged?.Invoke(m_CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// 強制的にダメージを与える（無敵無視）
        /// </summary>
        public virtual void ForceDamage(int damage)
        {
            bool wasInvincible = m_IsInvincible;
            m_IsInvincible = false;
            TakeDamage(damage);
            if (wasInvincible && IsAlive)
            {
                m_IsInvincible = true;
            }
        }

        /// <summary>
        /// 即死させる
        /// </summary>
        public virtual void Kill()
        {
            ForceDamage(m_CurrentHealth);
        }
    }
}
