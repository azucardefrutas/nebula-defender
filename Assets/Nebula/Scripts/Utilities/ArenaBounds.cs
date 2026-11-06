using UnityEngine;

namespace NebulaDefender
{
    public sealed class ArenaBounds : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;
        public Vector2 HalfSize => balance.arenaHalfSize;
        public bool OpenSpace => balance.openSpace;
        public void Configure(GameBalance value) => balance = value;
        public static Vector3 Wrapped(Vector3 p, Vector2 half)
        {
            p.x = Mathf.Repeat(p.x + half.x, 2 * half.x) - half.x;
            p.z = Mathf.Repeat(p.z + half.y, 2 * half.y) - half.y;
            p.y = 0;
            return p;
        }
        public void Contain(Rigidbody body)
        {
            if (OpenSpace) return;
            var p = body.position;
            if (Mathf.Abs(p.x) <= HalfSize.x && Mathf.Abs(p.z) <= HalfSize.y) return;
            if (balance.wrapEdges) body.position = Wrapped(p, HalfSize);
            else
            {
                p.x = Mathf.Clamp(p.x, -HalfSize.x, HalfSize.x); p.z = Mathf.Clamp(p.z, -HalfSize.y, HalfSize.y);
                body.position = p; body.linearVelocity = Vector3.zero;
            }
        }
        public bool Outside(Vector3 p, float margin = 2) => !OpenSpace && (Mathf.Abs(p.x) > HalfSize.x + margin || Mathf.Abs(p.z) > HalfSize.y + margin);
    }
}
