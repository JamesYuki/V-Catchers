namespace InGame.Entity
{
    /// <summary>
    /// ダメージを与えるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageSource
    {
        /// <summary>
        /// ダメージ量
        /// </summary>
        int DamageAmount { get; }
    }
}
