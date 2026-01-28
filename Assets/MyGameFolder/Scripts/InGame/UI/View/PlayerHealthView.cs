using UnityEngine;
using InGame.Player;

namespace InGame.UI.View
{
    /// <summary>
    /// プレイヤーHP表示View（画面固定UI）
    /// </summary>
    public class PlayerHealthView : HealthView
    {
        [Header("追加表示設定")]
        [SerializeField] private bool m_ShowMaxHealth = true;
        [SerializeField] private string m_HealthFormat = "HP: {0} / {1}";
        [SerializeField] private Color m_LowHealthColor = Color.red;
        [SerializeField] private float m_LowHealthThreshold = 0.3f;

        private PlayerHealthModule m_PlayerHealthModule;
        private Color m_OriginalTextColor;

        protected override void Awake()
        {
            base.Awake();
            if (m_HealthText != null)
            {
                m_OriginalTextColor = m_HealthText.color;
            }
        }

        private void Start()
        {
            // ServiceLocatorからPlayerHealthModuleを取得してバインド
            TryBindToPlayer();
        }

        private void OnEnable()
        {
            TryBindToPlayer();
        }

        private void OnDisable()
        {
            UnbindFromPlayer();
        }

        /// <summary>
        /// プレイヤーのHealthModuleにバインド
        /// </summary>
        private void TryBindToPlayer()
        {
            try
            {
                m_PlayerHealthModule = ServiceLocator.Service<PlayerHealthModule>();
                BindToPlayer();
            }
            catch (System.InvalidOperationException)
            {
                // PlayerHealthModuleがまだ登録されていない場合は後で再試行
                Invoke(nameof(TryBindToPlayer), 0.1f);
            }
        }

        private void BindToPlayer()
        {
            if (m_PlayerHealthModule != null)
            {
                m_PlayerHealthModule.OnHealthChanged += UpdateHealth;
                m_PlayerHealthModule.OnDeath += OnPlayerDeath;

                // 初期値をセット
                SetHealthImmediate(m_PlayerHealthModule.CurrentHealth, m_PlayerHealthModule.MaxHealth);
            }
        }

        private void UnbindFromPlayer()
        {
            if (m_PlayerHealthModule != null)
            {
                m_PlayerHealthModule.OnHealthChanged -= UpdateHealth;
                m_PlayerHealthModule.OnDeath -= OnPlayerDeath;
            }
        }

        protected override void UpdateHealthText(int currentHealth, int maxHealth)
        {
            if (m_HealthText != null)
            {
                if (m_ShowMaxHealth)
                {
                    m_HealthText.text = string.Format(m_HealthFormat, currentHealth, maxHealth);
                }
                else
                {
                    m_HealthText.text = currentHealth.ToString();
                }

                // 低HP時の色変更
                float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
                m_HealthText.color = ratio <= m_LowHealthThreshold ? m_LowHealthColor : m_OriginalTextColor;
            }
        }

        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
        private void OnPlayerDeath()
        {
            // 死亡時のUI演出（必要に応じて）
            if (m_HealthText != null)
            {
                m_HealthText.text = "DEAD";
                m_HealthText.color = m_LowHealthColor;
            }
        }

        /// <summary>
        /// 手動でPlayerHealthModuleをセット
        /// </summary>
        public void SetPlayerHealthModule(PlayerHealthModule healthModule)
        {
            UnbindFromPlayer();
            m_PlayerHealthModule = healthModule;
            BindToPlayer();
        }
    }
}
