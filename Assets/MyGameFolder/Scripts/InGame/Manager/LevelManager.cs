using System;
using UnityEngine;

namespace InGame.Manager
{
    public class LevelManager : MonoBehaviour
    {
        private int m_CurrentLevel = 1;
        private int m_CurrentExp = 0;

        public int CurrentLevel => m_CurrentLevel;

        private Action OnChangeLevel;

        [Header("持ち上げ力設定")]
        [SerializeField, Tooltip("基礎持ち上げ可能重量")]
        private int m_BaseLiftCapacity = 1;

        [SerializeField, Tooltip("レベルごとの持ち上げ力増加量")]
        private int m_LiftCapacityPerLevel = 1;

        /// <summary>
        /// 現在の持ち上げ可能重量（レベルに応じて増加）
        /// </summary>
        public int CurrentLiftCapacity => m_BaseLiftCapacity + (m_CurrentLevel - 1) * m_LiftCapacityPerLevel;

        private void Start()
        {
            ServiceLocator.Register<LevelManager>(this);

            OnChangeLevel += () =>
            {
                var inGameUI = ServiceLocator.Service<UI.InGameUI>();
                if (inGameUI != null)
                {
                    inGameUI.UpdateCurrentLevel(m_CurrentLevel);
                }
            };
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<LevelManager>();
        }

        public void AddExp(int exp)
        {
            AppLogger.Log("経験値獲得: " + exp);
            int expThreshold = 5;
            m_CurrentExp += exp;
            if (m_CurrentExp % expThreshold == 0)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            m_CurrentLevel++;
            OnChangeLevel?.Invoke();
        }

        /// <summary>
        /// 指定した重さを持ち上げられるかどうか判定
        /// </summary>
        /// <param name="weight">対象の重さ</param>
        /// <returns>持ち上げ可能ならtrue</returns>
        public bool CanLift(int weight)
        {
            return CurrentLiftCapacity >= weight;
        }

        /// <summary>
        /// 指定した重さに対する持ち上げやすさを計算（グラデーション）
        /// </summary>
        /// <param name="weight">対象の重さ</param>
        /// <returns>0.0〜1.0+の持ち上げやすさ（1.0で通常、それ以上で余裕あり、0に近いほど重い）</returns>
        public float GetLiftEase(float weight)
        {
            if (weight <= 0f) return 1f;
            return CurrentLiftCapacity / weight;
        }

        /// <summary>
        /// 指定した重さを持ち上げられるかどうか判定（float版）
        /// </summary>
        /// <param name="weight">対象の重さ</param>
        /// <returns>持ち上げ可能ならtrue</returns>
        public bool CanLift(float weight)
        {
            return CurrentLiftCapacity >= weight;
        }

    }
}