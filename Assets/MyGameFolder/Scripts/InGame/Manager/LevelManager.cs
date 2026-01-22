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

    }
}