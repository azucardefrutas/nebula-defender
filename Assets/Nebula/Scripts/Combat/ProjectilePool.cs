using System.Collections.Generic;
using UnityEngine;

namespace NebulaDefender
{
    public sealed class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile playerPrefab, hostilePrefab;
        private readonly Stack<Projectile> player = new Stack<Projectile>(), hostile = new Stack<Projectile>();
        private readonly List<Projectile> all = new List<Projectile>();
        private readonly Dictionary<Projectile, Team> owners = new Dictionary<Projectile, Team>();
        private int limit;
        public ArenaBounds Arena { get; private set; }
        public int ActiveCount { get; private set; }
        public void Configure(Projectile friendly, Projectile enemy) { playerPrefab = friendly; hostilePrefab = enemy; }
        public void Initialize(ArenaBounds arena, int max)
        {
            Arena = arena; limit = Mathf.Max(0, max);
            // Alternate factions so small budgets still reserve shots for both sides.
            int warmCount = Mathf.Min(64, limit);
            for (int i = 0; i < warmCount; i++) Create(i % 2 == 0 ? Team.Player : Team.Hostile);
        }
        private void Create(Team faction)
        {
            var instance = Instantiate(faction == Team.Player ? playerPrefab : hostilePrefab, transform);
            instance.gameObject.SetActive(false); all.Add(instance); owners.Add(instance, faction);
            (faction == Team.Player ? player : hostile).Push(instance);
        }
        public bool Fire(Team faction, Vector3 p, Vector3 direction, float speed, int damage, float life)
        {
            var stack = faction == Team.Player ? player : hostile;
            if (stack.Count == 0 && all.Count < limit) Create(faction);
            if (stack.Count == 0) return false;
            var shot = stack.Pop(); ActiveCount++;
            shot.Launch(this, faction, p, direction, speed, damage, life); return true;
        }
        public void Return(Projectile shot)
        {
            shot.gameObject.SetActive(false); ActiveCount--;
            (owners[shot] == Team.Player ? player : hostile).Push(shot);
        }
        public void Clear() { foreach (var shot in all) if (shot.gameObject.activeSelf) shot.Release(); }
    }
}
