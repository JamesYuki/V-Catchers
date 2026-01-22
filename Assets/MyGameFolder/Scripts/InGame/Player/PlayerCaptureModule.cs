using InGame.Manager;
using UnityEngine;

namespace InGame.Player
{
    public class PlayerCaptureModule : MonoBehaviour, IPlayerModule
    {
        private PlayerInputModule m_PlayerInputModule;

        public void Setup(PlayerController player)
        {
            player.GetModule(out m_PlayerInputModule);
        }

        public void StartModule()
        {
        }

        public void UpdateModule()
        {
            if (m_PlayerInputModule.GetClickedObject2D(out GameObject clickedObject))
            {
                AppLogger.Log("Clicked on object: " + clickedObject.name);
                clickedObject.TryGetComponent<InGame.Gimmick.ICapturable>(out var capturable);
                if (capturable != null)
                {
                    capturable.Capture(out var virus);

                    ServiceLocator.Service<LevelManager>()?.AddExp(virus.Exp);
                }
            }
        }

        public void FixedUpdateModule()
        {
            // 物理演算関連の更新があればここに記述
        }

        public void DestroyModule()
        {
            // クリーンアップ処理があればここに記述
        }
    }
}
