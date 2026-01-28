using UnityEngine;

namespace InGame.Entity.Damage
{
    /// <summary>
    /// ダメージ条件の基底クラス（ScriptableObject）
    /// Inspectorで設定可能なダメージ条件を作成可能
    /// </summary>
    public abstract class DamageConditionBase : ScriptableObject, IDamageCondition
    {
        public abstract bool IsSatisfied(DamageContext context);

        /// <summary>
        /// デフォルトでは基本ダメージをそのまま返す
        /// </summary>
        public virtual int CalculateDamage(DamageContext context, int baseDamage)
        {
            return baseDamage;
        }
    }
}
