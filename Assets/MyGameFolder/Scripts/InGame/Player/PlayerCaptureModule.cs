using InGame.Manager;
using InGame.Entity;
using UnityEngine;

namespace InGame.Player
{
    public class PlayerCaptureModule : PlayerModuleBase
    {
        private PlayerInputModule m_PlayerInputModule;

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            m_PlayerController.TryGetModule(out m_PlayerInputModule);
        }

        public override void StartModule()
        {
        }

        public override void UpdateModule()
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

        public override void FixedUpdateModule()
        {
            // 物理演算関連の更新があればここに記述
        }

        public override void DestroyModule()
        {
            // クリーンアップ処理があればここに記述
        }
    }
}
