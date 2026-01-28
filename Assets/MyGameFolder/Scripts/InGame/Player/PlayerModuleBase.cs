using UnityEngine;
using InGame.Entity;

namespace InGame.Player
{
    /// <summary>
    /// Playerモジュールの基底クラス
    /// IPlayerModuleを実装し、PlayerControllerへの参照を保持
    /// </summary>
    public abstract class PlayerModuleBase : MonoBehaviour, IPlayerModule
    {
        protected PlayerController m_PlayerController;

        public PlayerController PlayerController => m_PlayerController;

        public virtual void Setup(EntityController controller)
        {
            m_PlayerController = controller as PlayerController;
            if (m_PlayerController == null)
            {
                Debug.LogError($"[{GetType().Name}] PlayerControllerが見つかりません");
            }
        }

        public virtual void StartModule() { }
        public virtual void UpdateModule() { }
        public virtual void FixedUpdateModule() { }
        public virtual void DestroyModule() { }
    }
}
