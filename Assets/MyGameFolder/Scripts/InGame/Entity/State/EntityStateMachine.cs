using System;
using System.Collections.Generic;
using UnityEngine;
using InGame.Entity.State;

namespace InGame.Entity
{
    /// <summary>
    /// エンティティのステート管理クラス（State Pattern）
    /// 各ステートクラスを管理し、ステート遷移時にOnEnter/OnExitを呼び出す
    /// </summary>
    [Serializable]
    public class EntityStateMachine
    {
        [SerializeField, Tooltip("現在のステート（デバッグ用表示）")]
        private EntityStateType m_CurrentStateType = EntityStateType.Normal;

        private EntityController m_Controller;
        private EntityState m_CurrentState;
        private Dictionary<Type, EntityState> m_States = new Dictionary<Type, EntityState>();

        /// <summary>
        /// ステート変更時のイベント（前のステート, 新しいステート）
        /// </summary>
        public event Action<EntityStateType, EntityStateType> OnStateChanged;

        /// <summary>
        /// 現在のステート
        /// </summary>
        public EntityState CurrentState => m_CurrentState;

        /// <summary>
        /// 現在のステートタイプ
        /// </summary>
        public EntityStateType CurrentStateType => m_CurrentState?.StateType ?? EntityStateType.Normal;

        /// <summary>
        /// 生存しているか（Dead以外）
        /// </summary>
        public bool IsAlive => m_CurrentState?.StateType != EntityStateType.Dead;

        /// <summary>
        /// 行動可能か
        /// </summary>
        public bool CanAct => m_CurrentState?.AllowedActions != ActionCategory.None;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(EntityController controller)
        {
            m_Controller = controller;
            m_States.Clear();

            // 基本ステートを登録
            RegisterState(new NormalState());
            RegisterState(new DeadState());
            RegisterState(new StunnedState());
            RegisterState(new KnockbackState());
            RegisterState(new InvincibleState());

            // 初期ステートに移行
            ChangeState<NormalState>();
        }

        /// <summary>
        /// ステートを登録
        /// </summary>
        public void RegisterState(EntityState state)
        {
            state.Initialize(m_Controller, this);
            m_States[state.GetType()] = state;
        }

        /// <summary>
        /// カスタムステートを登録
        /// </summary>
        public void RegisterState<T>(T state) where T : EntityState
        {
            state.Initialize(m_Controller, this);
            m_States[typeof(T)] = state;
        }

        /// <summary>
        /// 更新（毎フレーム呼ぶ）
        /// </summary>
        public void Update()
        {
            m_CurrentState?.OnUpdate();
            m_CurrentStateType = m_CurrentState?.StateType ?? EntityStateType.Normal;
        }

        /// <summary>
        /// FixedUpdate
        /// </summary>
        public void FixedUpdate()
        {
            m_CurrentState?.OnFixedUpdate();
        }

        /// <summary>
        /// ステートを変更（型指定）
        /// </summary>
        public void ChangeState<T>() where T : EntityState
        {
            if (!m_States.TryGetValue(typeof(T), out var newState))
            {
                AppLogger.LogWarning($"[EntityStateMachine] State {typeof(T).Name} is not registered");
                return;
            }

            ChangeStateInternal(newState);
        }

        /// <summary>
        /// ステートを変更（EntityStateType指定）
        /// </summary>
        public void ChangeState(EntityStateType stateType)
        {
            EntityState newState = stateType switch
            {
                EntityStateType.Normal => GetState<NormalState>(),
                EntityStateType.Dead => GetState<DeadState>(),
                EntityStateType.Stunned => GetState<StunnedState>(),
                EntityStateType.Knockback => GetState<KnockbackState>(),
                EntityStateType.Invincible => GetState<InvincibleState>(),
                _ => GetState<NormalState>()
            };

            if (newState != null)
            {
                ChangeStateInternal(newState);
            }
        }

        /// <summary>
        /// ステート変更の内部処理
        /// </summary>
        private void ChangeStateInternal(EntityState newState)
        {
            if (m_CurrentState == newState) return;

            // 死亡状態からは復帰できない（ResetStateを使用）
            if (m_CurrentState?.StateType == EntityStateType.Dead &&
                newState.StateType != EntityStateType.Normal)
            {
                return;
            }

            var previousStateType = m_CurrentState?.StateType ?? EntityStateType.Normal;

            // 前のステートのOnExit
            m_CurrentState?.OnExit();

            // 新しいステートに切り替え
            m_CurrentState = newState;
            m_CurrentStateType = newState.StateType;

            // 新しいステートのOnEnter
            m_CurrentState.OnEnter();

            OnStateChanged?.Invoke(previousStateType, m_CurrentState.StateType);
        }

        /// <summary>
        /// ステートを取得
        /// </summary>
        public T GetState<T>() where T : EntityState
        {
            if (m_States.TryGetValue(typeof(T), out var state))
            {
                return state as T;
            }
            return null;
        }

        /// <summary>
        /// 死亡状態に移行
        /// </summary>
        public void SetDead()
        {
            ChangeState<DeadState>();
        }

        /// <summary>
        /// スタン状態に移行
        /// </summary>
        public void SetStunned(float duration = 1f)
        {
            if (m_CurrentState?.StateType == EntityStateType.Dead) return;

            var stunnedState = GetState<StunnedState>();
            stunnedState?.SetDuration(duration);
            ChangeState<StunnedState>();
        }

        /// <summary>
        /// ノックバック状態に移行
        /// </summary>
        public void SetKnockback(Vector2 force, float duration = 0.3f)
        {
            if (m_CurrentState?.StateType == EntityStateType.Dead) return;

            var knockbackState = GetState<KnockbackState>();
            knockbackState?.SetKnockback(force, duration);
            ChangeState<KnockbackState>();
        }

        /// <summary>
        /// 無敵状態に移行
        /// </summary>
        public void SetInvincible(float duration, float blinkInterval = 0.1f)
        {
            if (m_CurrentState?.StateType == EntityStateType.Dead) return;

            var invincibleState = GetState<InvincibleState>();
            invincibleState?.SetDuration(duration, blinkInterval);
            ChangeState<InvincibleState>();
        }

        /// <summary>
        /// 通常状態に戻す
        /// </summary>
        public void SetNormal()
        {
            if (m_CurrentState?.StateType == EntityStateType.Dead) return;
            ChangeState<NormalState>();
        }

        /// <summary>
        /// ステートを完全にリセット（リスポーン時など）
        /// </summary>
        public void ResetState()
        {
            var previousStateType = m_CurrentState?.StateType ?? EntityStateType.Normal;
            m_CurrentState?.OnExit();
            m_CurrentState = GetState<NormalState>();
            m_CurrentState?.OnEnter();
            OnStateChanged?.Invoke(previousStateType, EntityStateType.Normal);
        }

        /// <summary>
        /// 指定した行動が現在のステートで許可されているか
        /// </summary>
        public bool CanPerformAction(ActionCategory action)
        {
            return m_CurrentState?.CanPerformAction(action) ?? false;
        }

        /// <summary>
        /// 現在のステートが指定したタイプかどうか
        /// </summary>
        public bool IsState(EntityStateType stateType)
        {
            return m_CurrentState?.StateType == stateType;
        }

        /// <summary>
        /// 現在のステートが指定した型かどうか
        /// </summary>
        public bool IsState<T>() where T : EntityState
        {
            return m_CurrentState is T;
        }

        /// <summary>
        /// 無敵状態かどうか
        /// </summary>
        public bool IsInvincible => m_CurrentState?.StateType == EntityStateType.Invincible;
    }
}
