using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Gimmick
{
    public class SpawnerGimmick : MonoBehaviour
    {
        [Header("スポーン設定")]
        [SerializeField] private GameObject m_SpawnPrefab;
        [SerializeField] private int m_SpawnCount = 1;
        [SerializeField] private float m_SpawnRange = 5f;
        [SerializeField] private Vector2 m_CenterOffset = Vector2.zero;
        [SerializeField] private float m_SpawnInterval = 1.0f;

        // 指定範囲内にランダム生成
        public void SpawnRandom()
        {
            if (m_SpawnPrefab == null) return;
            for (int i = 0; i < m_SpawnCount; i++)
            {
                Vector2 randomPos = (Vector2)transform.position + m_CenterOffset + Random.insideUnitCircle * m_SpawnRange;
                randomPos.y = transform.position.y;
                GameObject obj = Instantiate(m_SpawnPrefab, randomPos, Quaternion.identity);
                // 上方向のランダムな速度を与える
                var rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float angle = Random.Range(-45f, 45f); // 上方向±45度
                    float speed = Random.Range(2f, 5f);    // 速度範囲
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                    rb.linearVelocity = dir * speed;
                }
            }
        }

        private async void Start()
        {
            while (true)
            {
                await SpawnLoop();
            }
        }

        private async UniTask SpawnLoop()
        {
            SpawnRandom();

            await UniTask.WaitForSeconds(m_SpawnInterval);
        }
    }
}

