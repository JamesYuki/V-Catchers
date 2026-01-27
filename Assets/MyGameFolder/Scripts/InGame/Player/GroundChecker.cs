using UnityEngine;

namespace InGame
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField, Header("地面判定距離")]
        private float m_GroundCheckDistance = 0.2f;

        [SerializeField, Header("地面レイヤー")]
        private LayerMask m_GroundLayer;

        [SerializeField, Header("接地判定位置")]
        private Transform m_GroundCheck;

        public bool IsGrounded
        {
            get
            {
                if (m_GroundCheck == null)
                {
                    return false;
                }
                return Physics2D.OverlapCircle(m_GroundCheck.position, m_GroundCheckDistance, m_GroundLayer);
            }
        }
    }
}