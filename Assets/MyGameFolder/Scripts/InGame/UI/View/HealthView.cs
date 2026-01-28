using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.View
{
    /// <summary>
    /// HP表示の基底クラス（View）
    /// </summary>
    public abstract class HealthView : MonoBehaviour
    {
        [Header("テキスト表示")]
        [SerializeField] protected TextMeshProUGUI m_HealthText;
        
        [Header("HPバー")]
        [SerializeField] protected Image m_HealthBarFill;
        [SerializeField] protected Image m_HealthBarDamage;
        
        [Header("アニメーション設定")]
        [SerializeField] protected float m_DamageBarLerpSpeed = 3f;
        [SerializeField] protected float m_TextScalePunchAmount = 1.2f;
        [SerializeField] protected float m_TextScalePunchDuration = 0.1f;

        protected int m_DisplayedHealth;
        protected int m_MaxHealth;
        protected float m_TargetFillAmount;
        protected float m_DamageFillAmount;
        protected bool m_IsPunching;
        protected float m_PunchTimer;
        protected Vector3 m_OriginalTextScale;

        protected virtual void Awake()
        {
            if (m_HealthText != null)
            {
                m_OriginalTextScale = m_HealthText.transform.localScale;
            }
        }

        protected virtual void Update()
        {
            UpdateDamageBar();
            UpdateTextPunch();
        }

        /// <summary>
        /// HP表示を更新
        /// </summary>
        public virtual void UpdateHealth(int currentHealth, int maxHealth)
        {
            bool damaged = currentHealth < m_DisplayedHealth;
            
            m_DisplayedHealth = currentHealth;
            m_MaxHealth = maxHealth;
            m_TargetFillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

            // テキスト更新
            UpdateHealthText(currentHealth, maxHealth);

            // HPバー更新
            UpdateHealthBar(m_TargetFillAmount);

            // ダメージを受けた場合、ダメージバーとテキストパンチ演出
            if (damaged)
            {
                StartTextPunch();
            }
            else
            {
                // 回復時はダメージバーもすぐに更新
                if (m_HealthBarDamage != null)
                {
                    m_DamageFillAmount = m_TargetFillAmount;
                    m_HealthBarDamage.fillAmount = m_DamageFillAmount;
                }
            }
        }

        /// <summary>
        /// HPテキストを更新
        /// </summary>
        protected virtual void UpdateHealthText(int currentHealth, int maxHealth)
        {
            if (m_HealthText != null)
            {
                m_HealthText.text = $"{currentHealth} / {maxHealth}";
            }
        }

        /// <summary>
        /// HPバーを更新
        /// </summary>
        protected virtual void UpdateHealthBar(float fillAmount)
        {
            if (m_HealthBarFill != null)
            {
                m_HealthBarFill.fillAmount = fillAmount;
            }

            // ダメージバーは現在の値を保持（遅れて追従）
            if (m_HealthBarDamage != null && m_DamageFillAmount < fillAmount)
            {
                m_DamageFillAmount = fillAmount;
                m_HealthBarDamage.fillAmount = m_DamageFillAmount;
            }
        }

        /// <summary>
        /// ダメージバーの遅延更新
        /// </summary>
        protected virtual void UpdateDamageBar()
        {
            if (m_HealthBarDamage != null && m_DamageFillAmount > m_TargetFillAmount)
            {
                m_DamageFillAmount = Mathf.Lerp(m_DamageFillAmount, m_TargetFillAmount, 
                    m_DamageBarLerpSpeed * Time.deltaTime);
                
                if (Mathf.Abs(m_DamageFillAmount - m_TargetFillAmount) < 0.001f)
                {
                    m_DamageFillAmount = m_TargetFillAmount;
                }
                
                m_HealthBarDamage.fillAmount = m_DamageFillAmount;
            }
        }

        /// <summary>
        /// テキストパンチ演出開始
        /// </summary>
        protected virtual void StartTextPunch()
        {
            if (m_HealthText != null)
            {
                m_IsPunching = true;
                m_PunchTimer = m_TextScalePunchDuration;
            }
        }

        /// <summary>
        /// テキストパンチ演出更新
        /// </summary>
        protected virtual void UpdateTextPunch()
        {
            if (!m_IsPunching || m_HealthText == null) return;

            m_PunchTimer -= Time.deltaTime;
            float t = 1f - (m_PunchTimer / m_TextScalePunchDuration);
            
            if (t < 0.5f)
            {
                // 拡大
                float scale = Mathf.Lerp(1f, m_TextScalePunchAmount, t * 2f);
                m_HealthText.transform.localScale = m_OriginalTextScale * scale;
            }
            else
            {
                // 縮小
                float scale = Mathf.Lerp(m_TextScalePunchAmount, 1f, (t - 0.5f) * 2f);
                m_HealthText.transform.localScale = m_OriginalTextScale * scale;
            }

            if (m_PunchTimer <= 0f)
            {
                m_IsPunching = false;
                m_HealthText.transform.localScale = m_OriginalTextScale;
            }
        }

        /// <summary>
        /// 即座に値をセット（アニメーションなし）
        /// </summary>
        public virtual void SetHealthImmediate(int currentHealth, int maxHealth)
        {
            m_DisplayedHealth = currentHealth;
            m_MaxHealth = maxHealth;
            m_TargetFillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            m_DamageFillAmount = m_TargetFillAmount;

            UpdateHealthText(currentHealth, maxHealth);
            
            if (m_HealthBarFill != null)
            {
                m_HealthBarFill.fillAmount = m_TargetFillAmount;
            }
            
            if (m_HealthBarDamage != null)
            {
                m_HealthBarDamage.fillAmount = m_DamageFillAmount;
            }
        }
    }
}
