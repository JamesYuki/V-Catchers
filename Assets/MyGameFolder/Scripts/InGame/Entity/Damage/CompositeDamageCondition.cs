using UnityEngine;

namespace InGame.Entity.Damage
{
    /// <summary>
    /// 複合ダメージ条件
    /// 複数の条件を組み合わせて評価（AND/OR）
    /// </summary>
    [CreateAssetMenu(fileName = "CompositeDamageCondition", menuName = "MyGame/Damage/Composite Condition")]
    public class CompositeDamageCondition : DamageConditionBase
    {
        public enum LogicType
        {
            And,    // 全ての条件を満たす
            Or      // いずれかの条件を満たす
        }

        [Header("論理タイプ")]
        [SerializeField]
        private LogicType m_LogicType = LogicType.And;

        [Header("条件リスト")]
        [SerializeField]
        private DamageConditionBase[] m_Conditions;

        public override bool IsSatisfied(DamageContext context)
        {
            if (m_Conditions == null || m_Conditions.Length == 0)
            {
                return true;
            }

            switch (m_LogicType)
            {
                case LogicType.And:
                    foreach (var condition in m_Conditions)
                    {
                        if (condition != null && !condition.IsSatisfied(context))
                        {
                            return false;
                        }
                    }
                    return true;

                case LogicType.Or:
                    foreach (var condition in m_Conditions)
                    {
                        if (condition != null && condition.IsSatisfied(context))
                        {
                            return true;
                        }
                    }
                    return false;

                default:
                    return true;
            }
        }

        public override int CalculateDamage(DamageContext context, int baseDamage)
        {
            int damage = baseDamage;

            // 全ての条件のダメージ計算を適用（乗算）
            if (m_Conditions != null)
            {
                foreach (var condition in m_Conditions)
                {
                    if (condition != null)
                    {
                        damage = condition.CalculateDamage(context, damage);
                    }
                }
            }

            return damage;
        }
    }
}
