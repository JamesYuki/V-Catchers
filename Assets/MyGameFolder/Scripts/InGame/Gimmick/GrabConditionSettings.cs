using UnityEngine;

namespace InGame.Gimmick
{
    /// <summary>
    /// 掴み条件の設定（ScriptableObject）
    /// 速度、接地状態などの条件を設定可能
    /// </summary>
    [CreateAssetMenu(fileName = "GrabConditionSettings", menuName = "MyGame/Gimmick/Grab Condition Settings")]
    public class GrabConditionSettings : ScriptableObject, IGrabCondition
    {
        [Header("速度条件")]
        [SerializeField, Tooltip("この速度以上は掴めない（0で無効）")]
        private float m_MaxGrabbableVelocity = 5f;

        [Header("接地条件")]
        [SerializeField, Tooltip("空中のオブジェクトは掴めない")]
        private bool m_RequireGrounded = true;

        [SerializeField, Tooltip("接地判定のレイキャスト距離")]
        private float m_GroundCheckDistance = 0.2f;

        [SerializeField, Tooltip("地面とみなすレイヤー")]
        private LayerMask m_GroundLayers = ~0; // デフォルトは全レイヤー

        [Header("距離条件")]
        [SerializeField, Tooltip("最低距離（これより近いと掴めない）")]
        private float m_MinGrabDistance = 0f;

        [Header("高さ条件")]
        [SerializeField, Tooltip("Grabberより上にあるオブジェクトのみ掴める")]
        private bool m_RequireAboveGrabber = false;

        /// <summary>
        /// 最大掴み可能速度
        /// </summary>
        public float MaxGrabbableVelocity => m_MaxGrabbableVelocity;

        /// <summary>
        /// 接地が必要か
        /// </summary>
        public bool RequireGrounded => m_RequireGrounded;

        public bool CanGrab(IGrabbable grabbable, IGrabber grabber)
        {
            if (grabbable == null) return false;

            // 既に掴まれている
            if (grabbable.IsGrabbed) return false;

            var rb = grabbable.Rigidbody;
            var grabbableObj = grabbable.GrabbableObject;

            if (grabbableObj == null) return false;

            // 速度チェック
            if (m_MaxGrabbableVelocity > 0f && rb != null)
            {
                if (rb.linearVelocity.magnitude > m_MaxGrabbableVelocity)
                {
                    return false;
                }
            }

            // 接地チェック
            if (m_RequireGrounded)
            {
                if (!IsGrounded(grabbableObj, rb))
                {
                    return false;
                }
            }

            // 距離チェック
            if (m_MinGrabDistance > 0f && grabber != null)
            {
                float distance = Vector3.Distance(grabbableObj.transform.position, grabber.GrabberPosition);
                if (distance < m_MinGrabDistance)
                {
                    return false;
                }
            }

            // 高さチェック
            if (m_RequireAboveGrabber && grabber != null)
            {
                if (grabbableObj.transform.position.y <= grabber.GrabberPosition.y)
                {
                    return false;
                }
            }

            // IGrabbableConditionProviderがあればその条件もチェック
            var conditionProvider = grabbableObj.GetComponent<IGrabbableConditionProvider>();
            if (conditionProvider != null && !conditionProvider.CanBeGrabbed)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 接地判定
        /// </summary>
        private bool IsGrounded(GameObject obj, Rigidbody2D rb)
        {
            if (obj == null) return false;

            // Rigidbody2Dの速度で判定（落下中は掴めない）
            if (rb != null && rb.linearVelocity.y < -1f)
            {
                return false;
            }

            // レイキャストで地面判定
            var collider = obj.GetComponent<Collider2D>();
            if (collider != null)
            {
                Vector2 origin = (Vector2)obj.transform.position + Vector2.down * (collider.bounds.extents.y);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, m_GroundCheckDistance, m_GroundLayers);
                return hit.collider != null;
            }

            return true; // Colliderがない場合は接地とみなす
        }
    }
}
