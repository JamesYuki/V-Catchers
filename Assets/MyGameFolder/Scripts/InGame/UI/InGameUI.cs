using TMPro;
using UnityEngine;

namespace InGame.UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_CurrentLevelText;
        void Start()
        {
            ServiceLocator.Register<InGameUI>(this);

            UpdateCurrentLevel(1);
        }


        private void OnDestroy()
        {
            ServiceLocator.Unregister<InGameUI>();
        }

        public void UpdateCurrentLevel(int level)
        {
            m_CurrentLevelText.text = "現在Lv. " + level.ToString();
        }
    }
}