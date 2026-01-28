using InGame.Entity;

namespace InGame.Player
{
    /// <summary>
    /// Player専用モジュールのインターフェース
    /// IEntityModuleを継承し、PlayerControllerへの参照を取得可能
    /// </summary>
    public interface IPlayerModule : IEntityModule
    {
        /// <summary>
        /// PlayerControllerへの参照を取得
        /// </summary>
        PlayerController PlayerController { get; }
    }
}