using UnityEngine;

namespace InGame.Entity
{
    /// <summary>
    /// シンプルなダメージソースコンポーネント
    /// 敵や飛び道具などにアタッチして使用
    /// </summary>
    public class DamageSource : MonoBehaviour, IDamageSource
    {
        [SerializeField] private int m_DamageAmount = 1;

        public int DamageAmount => m_DamageAmount;

        /// <summary>
        /// ダメージ量を設定
        /// </summary>
        public void SetDamageAmount(int amount)
        {
            m_DamageAmount = amount;
        }
    }
}
