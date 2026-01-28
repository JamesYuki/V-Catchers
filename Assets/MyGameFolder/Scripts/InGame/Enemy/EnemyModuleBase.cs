using UnityEngine;
using InGame.Entity;

namespace InGame.Enemy
{
    /// <summary>
    /// Enemyモジュールの基底クラス
    /// IEnemyModuleを実装し、EnemyControllerへの参照を保持
    /// </summary>
    public abstract class EnemyModuleBase : MonoBehaviour, IEnemyModule
    {
        protected EnemyController m_EnemyController;

        public EnemyController EnemyController => m_EnemyController;

        public virtual void Setup(EntityController controller)
        {
            m_EnemyController = controller as EnemyController;
            if (m_EnemyController == null)
            {
                Debug.LogError($"[{GetType().Name}] EnemyControllerが見つかりません");
            }
        }

        public virtual void StartModule() { }
        public virtual void UpdateModule() { }
        public virtual void FixedUpdateModule() { }
        public virtual void DestroyModule() { }
    }
}
