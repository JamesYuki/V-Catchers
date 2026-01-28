using UnityEngine;

namespace InGame.Entity.State
{
    /// <summary>
    /// エンティティのステート基底クラス（State Pattern）
    /// 各ステートはこのクラスを継承して振る舞いを定義
    /// </summary>
    public abstract class EntityState
    {
        protected EntityController m_Controller;
        protected EntityStateMachine m_StateMachine;

        /// <summary>
        /// ステートの種類
        /// </summary>
        public abstract EntityStateType StateType { get; }

        /// <summary>
        /// このステートで許可される行動カテゴリ
        /// </summary>
        public abstract ActionCategory AllowedActions { get; }

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize(EntityController controller, EntityStateMachine stateMachine)
        {
            m_Controller = controller;
            m_StateMachine = stateMachine;
        }

        /// <summary>
        /// ステートに入った時に呼ばれる
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// ステート中に毎フレーム呼ばれる
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// ステート中にFixedUpdateで呼ばれる
        /// </summary>
        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// ステートから出る時に呼ばれる
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 指定した行動が許可されているか
        /// </summary>
        public bool CanPerformAction(ActionCategory action)
        {
            return (AllowedActions & action) != 0;
        }
    }
}
