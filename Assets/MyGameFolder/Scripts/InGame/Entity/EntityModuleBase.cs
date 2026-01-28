using UnityEngine;

namespace InGame.Entity
{
    /// <summary>
    /// モジュールの基底クラス（オプショナル）
    /// 空実装を提供し、必要なメソッドのみオーバーライド可能
    /// </summary>
    public abstract class EntityModuleBase : MonoBehaviour, IEntityModule
    {
        protected EntityController m_Controller;

        public virtual void Setup(EntityController controller)
        {
            m_Controller = controller;
        }

        public virtual void StartModule() { }
        public virtual void UpdateModule() { }
        public virtual void FixedUpdateModule() { }
        public virtual void DestroyModule() { }
    }
}
