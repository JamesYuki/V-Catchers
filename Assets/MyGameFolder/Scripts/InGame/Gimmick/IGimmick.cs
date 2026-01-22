using UnityEngine;
using InGame.Player;

namespace InGame.Gimmick
{

    public interface IGimmick
    {
        void Activate();
        void Deactivate();
    }

    public interface IGrabable
    {
        Vector3 GetGrapplePoint();
        void Grap(PlayerController player);
    }

    public interface IDamageable
    {
        void TakeDamage(int amount);
    }

    public interface ICapturable
    {
        void Capture(out Virus virus);
    }
}
