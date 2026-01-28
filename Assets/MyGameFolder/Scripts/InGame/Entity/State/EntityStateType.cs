namespace InGame.Entity
{
    /// <summary>
    /// エンティティの基本ステート
    /// </summary>
    public enum EntityStateType
    {
        /// <summary>
        /// 通常状態（全ての行動が可能）
        /// </summary>
        Normal,

        /// <summary>
        /// スタン状態（行動不可）
        /// </summary>
        Stunned,

        /// <summary>
        /// ノックバック中（移動のみ制限）
        /// </summary>
        Knockback,

        /// <summary>
        /// 死亡状態（全ての行動が不可）
        /// </summary>
        Dead,

        /// <summary>
        /// 無敵状態（ダメージを受けないが行動は可能）
        /// </summary>
        Invincible
    }

    /// <summary>
    /// 行動カテゴリ（ステートによって制限される）
    /// </summary>
    [System.Flags]
    public enum ActionCategory
    {
        None = 0,
        Movement = 1 << 0,      // 移動
        Jump = 1 << 1,          // ジャンプ
        Grab = 1 << 2,          // 掴み
        Throw = 1 << 3,         // 投げ
        Attack = 1 << 4,        // 攻撃
        Interact = 1 << 5,      // インタラクション
        All = ~0                // 全て
    }
}
