using UnityEngine;

namespace InGame.Entity.Damage
{
    /// <summary>
    /// レイヤーベースのダメージ条件
    /// 特定のレイヤーのオブジェクトにのみダメージを与える
    /// </summary>
    [CreateAssetMenu(fileName = "LayerDamageCondition", menuName = "MyGame/Damage/Layer Condition")]
    public class LayerDamageCondition : DamageConditionBase
    {
        [Header("対象レイヤー")]
        [SerializeField, Tooltip("ダメージを与える対象のレイヤーマスク")]
        private LayerMask m_TargetLayers;

        [Header("除外レイヤー")]
        [SerializeField, Tooltip("ダメージを与えない除外レイヤーマスク")]
        private LayerMask m_ExcludeLayers;

        public override bool IsSatisfied(DamageContext context)
        {
            if (context.Target == null) return false;

            int targetLayer = context.Target.layer;
            int targetLayerMask = 1 << targetLayer;

            // 除外レイヤーに含まれていたらfalse
            if ((m_ExcludeLayers.value & targetLayerMask) != 0)
            {
                return false;
            }

            // 対象レイヤーに含まれていたらtrue（0の場合は全レイヤー対象）
            if (m_TargetLayers.value == 0)
            {
                return true;
            }

            return (m_TargetLayers.value & targetLayerMask) != 0;
        }
    }
}
