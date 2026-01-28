using TMPro;
using UnityEngine;

namespace InGame.UI.View
{
    /// <summary>
    /// ダメージ数値のポップアップ表示
    /// ダメージを受けた位置に数値を表示してフェードアウト
    /// </summary>
    public class DamagePopupView : MonoBehaviour
    {
        [Header("表示設定")]
        [SerializeField] private TextMeshProUGUI m_DamageText;
        [SerializeField] private float m_LifeTime = 1f;
        [SerializeField] private float m_MoveSpeed = 2f;
        [SerializeField] private Vector3 m_MoveDirection = Vector3.up;
        [SerializeField] private AnimationCurve m_FadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private AnimationCurve m_ScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 0.2f, 1f);

        [Header("色設定")]
        [SerializeField] private Color m_NormalDamageColor = Color.white;
        [SerializeField] private Color m_CriticalDamageColor = Color.yellow;
        [SerializeField] private Color m_HealColor = Color.green;

        private float m_Timer;
        private Vector3 m_StartPosition;
        private Vector3 m_OriginalScale;
        private CanvasGroup m_CanvasGroup;
        private Camera m_MainCamera;
        private bool m_FaceCamera = true;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            m_OriginalScale = transform.localScale;
        }

        private void Start()
        {
            m_MainCamera = Camera.main;
            m_StartPosition = transform.position;
            m_Timer = 0f;
        }

        private void Update()
        {
            m_Timer += Time.deltaTime;
            float normalizedTime = m_Timer / m_LifeTime;

            // 移動
            transform.position = m_StartPosition + m_MoveDirection * m_MoveSpeed * m_Timer;

            // スケール
            float scale = m_ScaleCurve.Evaluate(normalizedTime);
            transform.localScale = m_OriginalScale * scale;

            // フェード
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = m_FadeCurve.Evaluate(normalizedTime);
            }

            // カメラに向ける
            if (m_FaceCamera && m_MainCamera != null)
            {
                transform.rotation = m_MainCamera.transform.rotation;
            }

            // 寿命切れで削除
            if (m_Timer >= m_LifeTime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ダメージ表示を初期化
        /// </summary>
        public void Setup(int damage, bool isCritical = false)
        {
            if (m_DamageText != null)
            {
                m_DamageText.text = damage.ToString();
                m_DamageText.color = isCritical ? m_CriticalDamageColor : m_NormalDamageColor;
            }
        }

        /// <summary>
        /// 回復表示を初期化
        /// </summary>
        public void SetupHeal(int healAmount)
        {
            if (m_DamageText != null)
            {
                m_DamageText.text = $"+{healAmount}";
                m_DamageText.color = m_HealColor;
            }
        }

        /// <summary>
        /// カスタムテキストで初期化
        /// </summary>
        public void SetupCustom(string text, Color color)
        {
            if (m_DamageText != null)
            {
                m_DamageText.text = text;
                m_DamageText.color = color;
            }
        }

        /// <summary>
        /// ダメージポップアップを生成するファクトリメソッド
        /// </summary>
        public static DamagePopupView Create(GameObject prefab, Vector3 position, int damage, bool isCritical = false)
        {
            if (prefab == null) return null;

            var popup = Instantiate(prefab, position, Quaternion.identity).GetComponent<DamagePopupView>();
            if (popup != null)
            {
                popup.Setup(damage, isCritical);
            }
            return popup;
        }

        /// <summary>
        /// 回復ポップアップを生成するファクトリメソッド
        /// </summary>
        public static DamagePopupView CreateHeal(GameObject prefab, Vector3 position, int healAmount)
        {
            if (prefab == null) return null;

            var popup = Instantiate(prefab, position, Quaternion.identity).GetComponent<DamagePopupView>();
            if (popup != null)
            {
                popup.SetupHeal(healAmount);
            }
            return popup;
        }
    }
}
