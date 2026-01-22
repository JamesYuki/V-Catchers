using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Gimmick
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Virus : MonoBehaviour, ICapturable
    {
        [SerializeField] private float m_Lifetime = 5f;

        [SerializeField] private int m_Exp = 1;

        public int Exp => m_Exp;

        private void Start()
        {
            Destroy(gameObject, m_Lifetime);
        }

        public void Capture(out Virus virus)
        {
            virus = this;
            Destroy(gameObject);
        }
    }
}
