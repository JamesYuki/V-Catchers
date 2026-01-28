using UnityEngine;
using InGame.Enemy;

namespace InGame.UI.View
{
    /// <summary>
    /// 敵HP表示View（ワールドスペースUI用）
    /// 敵の頭上などに表示するHPバー
    /// </summary>
    public class EnemyHealthView : HealthView
    {
        [Header("ワールドスペース設定")]
        [SerializeField] private Vector3 m_Offset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private bool m_FaceCamera = true;
        [SerializeField] private bool m_HideWhenFull = true;
        [SerializeField] private float m_ShowDuration = 3f;

        [Header("参照")]
        [SerializeField] private EnemyHealthModule m_TargetHealthModule;

        private Transform m_TargetTransform;
        private Camera m_MainCamera;
        private CanvasGroup m_CanvasGroup;
        private float m_ShowTimer;
        private bool m_IsVisible;

        protected override void Awake()
        {
            base.Awake();
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            m_MainCamera = Camera.main;
            
            // ターゲットが設定されていない場合、親からEnemyHealthModuleを探す
            if (m_TargetHealthModule == null)
            {
                m_TargetHealthModule = GetComponentInParent<EnemyHealthModule>();
            }

            if (m_TargetHealthModule != null)
            {
                BindToEnemy(m_TargetHealthModule);
            }
        }

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateVisibility();
            
            if (m_FaceCamera && m_MainCamera != null)
            {
                transform.rotation = m_MainCamera.transform.rotation;
            }
        }

        /// <summary>
        /// 敵のHealthModuleにバインド
        /// </summary>
        public void BindToEnemy(EnemyHealthModule healthModule)
        {
            // 以前のバインドを解除
            if (m_TargetHealthModule != null)
            {
                m_TargetHealthModule.OnHealthChanged -= OnHealthChanged;
                m_TargetHealthModule.OnDeath -= OnEnemyDeath;
            }

            m_TargetHealthModule = healthModule;
            
            if (m_TargetHealthModule != null)
            {
                m_TargetTransform = m_TargetHealthModule.transform;
                m_TargetHealthModule.OnHealthChanged += OnHealthChanged;
                m_TargetHealthModule.OnDeath += OnEnemyDeath;

                // 初期値をセット
                SetHealthImmediate(m_TargetHealthModule.CurrentHealth, m_TargetHealthModule.MaxHealth);
                
                // 最大HPなら非表示
                if (m_HideWhenFull && m_TargetHealthModule.HealthRatio >= 1f)
                {
                    SetVisible(false);
                }
            }
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealth(currentHealth, maxHealth);
            
            // ダメージを受けたら表示
            if (m_HideWhenFull)
            {
                ShowTemporarily();
            }
        }

        private void OnEnemyDeath()
        {
            // 死亡時は非表示
            SetVisible(false);
        }

        /// <summary>
        /// 一時的に表示
        /// </summary>
        private void ShowTemporarily()
        {
            m_ShowTimer = m_ShowDuration;
            SetVisible(true);
        }

        /// <summary>
        /// 位置を更新
        /// </summary>
        private void UpdatePosition()
        {
            if (m_TargetTransform != null)
            {
                transform.position = m_TargetTransform.position + m_Offset;
            }
        }

        /// <summary>
        /// 表示/非表示を更新
        /// </summary>
        private void UpdateVisibility()
        {
            if (m_HideWhenFull && m_IsVisible)
            {
                m_ShowTimer -= Time.deltaTime;
                
                // HP満タンかつタイマー切れなら非表示
                if (m_ShowTimer <= 0f && m_TargetHealthModule != null && m_TargetHealthModule.HealthRatio >= 1f)
                {
                    SetVisible(false);
                }
            }
        }

        /// <summary>
        /// 表示状態を設定
        /// </summary>
        private void SetVisible(bool visible)
        {
            m_IsVisible = visible;
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        /// <summary>
        /// オフセット位置を設定
        /// </summary>
        public void SetOffset(Vector3 offset)
        {
            m_Offset = offset;
        }

        private void OnDestroy()
        {
            if (m_TargetHealthModule != null)
            {
                m_TargetHealthModule.OnHealthChanged -= OnHealthChanged;
                m_TargetHealthModule.OnDeath -= OnEnemyDeath;
            }
        }
    }
}
