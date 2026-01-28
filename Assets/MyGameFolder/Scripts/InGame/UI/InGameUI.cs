using TMPro;
using UnityEngine;
using InGame.UI.View;

namespace InGame.UI
{
    /// <summary>
    /// インゲームUIの統括クラス
    /// 各種Viewの管理と初期化を担当
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        [Header("レベル表示")]
        [SerializeField] private TextMeshProUGUI m_CurrentLevelText;

        [Header("HP表示")]
        [SerializeField] private PlayerHealthView m_PlayerHealthView;

        [Header("ダメージポップアップ")]
        [SerializeField] private GameObject m_DamagePopupPrefab;

        void Start()
        {
            ServiceLocator.Register<InGameUI>(this);
            UpdateCurrentLevel(1);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InGameUI>();
        }

        /// <summary>
        /// 現在レベルを更新
        /// </summary>
        public void UpdateCurrentLevel(int level)
        {
            if (m_CurrentLevelText != null)
            {
                m_CurrentLevelText.text = "現在Lv. " + level.ToString();
            }
        }

        /// <summary>
        /// PlayerHealthViewを取得
        /// </summary>
        public PlayerHealthView PlayerHealthView => m_PlayerHealthView;

        /// <summary>
        /// ダメージポップアップを表示
        /// </summary>
        public void ShowDamagePopup(Vector3 worldPosition, int damage, bool isCritical = false)
        {
            if (m_DamagePopupPrefab != null)
            {
                DamagePopupView.Create(m_DamagePopupPrefab, worldPosition, damage, isCritical);
            }
        }

        /// <summary>
        /// 回復ポップアップを表示
        /// </summary>
        public void ShowHealPopup(Vector3 worldPosition, int healAmount)
        {
            if (m_DamagePopupPrefab != null)
            {
                DamagePopupView.CreateHeal(m_DamagePopupPrefab, worldPosition, healAmount);
            }
        }
    }
}