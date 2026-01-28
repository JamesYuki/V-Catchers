using InGame.Entity;

namespace InGame.Enemy
{
    /// <summary>
    /// Enemy専用モジュールのインターフェース
    /// IEntityModuleを継承し、EnemyControllerへの参照を取得可能
    /// </summary>
    public interface IEnemyModule : IEntityModule
    {
        /// <summary>
        /// EnemyControllerへの参照を取得
        /// </summary>
        EnemyController EnemyController { get; }
    }
}
