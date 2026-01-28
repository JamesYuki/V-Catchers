namespace InGame.Entity
{
    /// <summary>
    /// Player, Enemy等のEntity共通モジュールインターフェース
    /// </summary>
    public interface IEntityModule
    {
        /// <summary>
        /// モジュールの初期化（Awake時に呼ばれる）
        /// </summary>
        void Setup(EntityController controller);

        /// <summary>
        /// モジュールの開始（Start時に呼ばれる）
        /// </summary>
        void StartModule();

        /// <summary>
        /// 毎フレーム呼ばれる
        /// </summary>
        void UpdateModule();

        /// <summary>
        /// 固定フレームレートで呼ばれる（物理演算向け）
        /// </summary>
        void FixedUpdateModule();

        /// <summary>
        /// モジュールの破棄時に呼ばれる
        /// </summary>
        void DestroyModule();
    }
}
