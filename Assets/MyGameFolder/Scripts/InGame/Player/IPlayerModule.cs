
namespace InGame.Player
{
    public interface IPlayerModule
    {
        void Setup(PlayerController playerController);
        void StartModule();
        void UpdateModule();
        void FixedUpdateModule();
        void DestroyModule();
    }
}