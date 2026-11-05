using UnityEngine;

namespace NebulaDefender
{
    public abstract class DamageTarget : MonoBehaviour
    {
        public abstract Team Team { get; }
        public abstract bool IsAlive { get; }
        public abstract void Hit(int damage, Vector3 point);
    }
}
