using UnityEngine;
using InGame.Player;

namespace InGame.Gimmick
{

    public interface IGimmick
    {
        void Activate();
        void Deactivate();
    }

    public interface IGrabbable
    {
        Vector3 GetGrapplePoint();
        void GrapStart(object grabber);
        void Grap(object grabber);
        void GrapEnd(object grabber);
        Vector3 Velocity { get; set; }
        bool IsGrabed { get; set; }
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
