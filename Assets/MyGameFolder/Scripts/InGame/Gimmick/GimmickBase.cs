using UnityEngine;

namespace InGame.Gimmick
{
    public abstract class GimmickBase : MonoBehaviour, IGimmick
    {
        protected DisposableGroup m_DisposableGroup = new();

        private int m_OriginalLayer = -1;

        public virtual void Activate()
        {
            m_DisposableGroup = new();
        }

        public virtual void Deactivate()
        {
            m_DisposableGroup.Dispose();
            Destroy(gameObject);
        }

        /// <summary>
        /// 指定レイヤーに変更し、元レイヤーを保存
        /// </summary>
        public void SetGrabLayer(string layerName)
        {
            if (this == null || gameObject == null)
            {
                return;
            }

            m_OriginalLayer = gameObject.layer;
            int grabLayer = LayerMask.NameToLayer(layerName);
            if (grabLayer >= 0) gameObject.layer = grabLayer;
        }

        /// <summary>
        /// 元のレイヤーに戻す
        /// </summary>
        public void RestoreLayer()
        {
            if (m_OriginalLayer >= 0)
            {
                gameObject.layer = m_OriginalLayer;
                m_OriginalLayer = -1;
            }
        }

        public virtual Vector3 GetGrapplePoint()
        {
            return transform.position;
        }
    }
}
