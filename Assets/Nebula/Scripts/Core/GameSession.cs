using System;
using UnityEngine;

namespace NebulaDefender
{
    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;
        [SerializeField] private LevelDefinition[] levels;
        [SerializeField] private PlayerController player;
        [SerializeField] private PilotInput input;
        [SerializeField] private ArenaBounds arena;
        [SerializeField] private ProjectilePool projectiles;
        [SerializeField] private CombatDirector director;
        [SerializeField] private CombatFeedback feedback;
        [SerializeField] private bool quickStart = true;
        [SerializeField] private AudioService audioService;
        [SerializeField] private SpaceBackdrop backdrop;
        private int levelIndex;
        private string runId;
        public event Action<SessionState> StateChanged;
        public static event Action EditorQuitRequested;
        public bool Sandbox => quickStart;
        public bool CampaignMode { get; private set; }
        public bool GuestRun { get; private set; }
        public JumpGate Gate { get; private set; }
        public bool ExtractionReady => CampaignMode && Gate != null && Gate.Ready;
        public ProfileRepository Profiles { get; private set; }
        public GameSettings Settings { get; } = new GameSettings();
        public AudioService Audio => audioService;
        public int UnlockedSector => quickStart || GuestRun ? 3 : Profiles?.Selected?.maxSector ?? 1;
        public int CurrentSectorIndex => levelIndex;
        public LevelDefinition[] Levels => levels;
        public SessionState State { get; private set; }
        public bool IsPlaying => State == SessionState.Playing;
        public GameBalance Balance => balance;
        public PlayerController Player => player;
        public PlayerHealth Health { get; private set; }
        public WeaponController Weapon { get; private set; }
        public PilotInput Input => input;
        public ArenaBounds Arena => arena;
        public ProjectilePool Projectiles => projectiles;
        public CombatDirector Director => director;
        public CombatFeedback Feedback => feedback;
        public MissionProgress Mission { get; private set; }
        public RunStatistics Stats { get; private set; } = new RunStatistics();
        public LevelDefinition Level => levels[levelIndex];
        public float TurnSensitivity => Settings.Turn;
        public bool DamagedThisSector { get; private set; }
        public void Configure(GameBalance b, LevelDefinition[] sectors, PlayerController p, PilotInput controls, ArenaBounds bounds, ProjectilePool pool, CombatDirector combat, CombatFeedback fx, bool quick)
        { balance = b; levels = sectors; player = p; input = controls; arena = bounds; projectiles = pool; director = combat; feedback = fx; quickStart = quick; }
        public void ConfigurePresentation(AudioService audio, SpaceBackdrop space) { audioService = audio; backdrop = space; }
        private void Start()
        {
            Settings.Load();
            CampaignMode = !quickStart;
            Gate = new GameObject("Jump gate").AddComponent<JumpGate>();
            Gate.transform.SetParent(transform,false); Gate.Initialize(this);
            if (!quickStart) Profiles = new ProfileRepository(Application.persistentDataPath);
            if (audioService != null) audioService.Initialize(this);
            feedback.SetSession(this);
            feedback.Initialize(); projectiles.Initialize(arena, balance.maxProjectiles);
            director.Initialize(this); player.Initialize(this, input, arena);
            Health = player.GetComponent<PlayerHealth>(); Weapon = player.GetComponent<WeaponController>();
            Health.Initialize(this, balance); Weapon.Initialize(this);
            Camera.main.GetComponent<CameraRig>().Bind(player, this);
            ApplySettings();
            if (quickStart) StartRun(0); else ReturnToMenu();
        }
        private void Update()
        {
            if (input.ViewPressed && (State == SessionState.Playing || State == SessionState.Paused)) Camera.main.GetComponent<CameraRig>().ToggleView();
            if (input.PausePressed && (State == SessionState.Playing || State == SessionState.Paused)) Pause(State != SessionState.Paused);
            if (IsPlaying) Stats.seconds += Time.deltaTime;
        }
        public void StartRun(int index)
        {
            if (!quickStart && !GuestRun && (Profiles.Selected == null || index >= UnlockedSector)) return;
            CommitRun();
            runId = !quickStart && !GuestRun ? Profiles.StartRun() : null;
            Stats = new RunStatistics(); Health.Initialize(this, balance); Weapon.Initialize(this);
            BeginSector(index);
        }
        public void LaunchCampaign(int index = 0)
        {
            CommitRun(); runId = null;
            CampaignMode = true; GuestRun = Profiles?.Selected == null;
            StartRun(index);
        }
        public void BeginSector(int index)
        {
            levelIndex = Mathf.Clamp(index, 0, levels.Length - 1); Stats.maxSector = Mathf.Max(Stats.maxSector, levelIndex + 1);
            Mission = new MissionProgress(Level.asteroidGoal, Level.droneGoal, Level.boss); DamagedThisSector = false;
            Gate.Close();
            Time.timeScale = 1; player.gameObject.SetActive(true); player.ResetPosition();
            projectiles.Clear(); SetState(SessionState.Playing); director.Begin(Level);
            Camera.main.backgroundColor = Level.spaceColor;
            if (backdrop != null) backdrop.ShowSector(Level.sector);
            if (audioService != null) audioService.PlayMusic(Level.sector);
        }
        public void AwardRock(int tier) { if (!IsPlaying || ExtractionReady) return; Stats.AddScore(balance.AsteroidPoints(tier)); Stats.asteroids++; Mission.RockDestroyed(); CheckComplete(); }
        public void AwardDrone() { if (!IsPlaying || ExtractionReady) return; Stats.AddScore(balance.dronePoints); Stats.enemies++; Mission.DroneDestroyed(); CheckComplete(); }
        public void AwardBoss() { if (!IsPlaying || ExtractionReady) return; Stats.AddScore(balance.bossPoints); Stats.enemies++; Mission.DefeatBoss(); CheckComplete(); }
        public void MarkDamage() => DamagedThisSector = true;
        public void CheckComplete()
        {
            if (!IsPlaying || !Mission.Complete) return;
            if (CampaignMode)
            {
                if (!Gate.Ready)
                {
                    director.StopForExtraction(); projectiles.Clear();
                    var heading=Camera.main.transform.forward; heading.y=0; heading.Normalize();
                    Gate.Open(player.Body.position+heading*14,heading);
                    if(audioService!=null)audioService.Cue(AudioCue.Complete);
                }
                return;
            }
            CompleteSector();
        }
        public bool TryExtract()
        {
            if (!IsPlaying || !ExtractionReady || !Mission.Complete || (player.Body.position-Gate.transform.position).sqrMagnitude>4) return false;
            Gate.Close(); CompleteSector(); return true;
        }
        private void CompleteSector()
        {
            Stats.AddScore(balance.sectorBonus + (DamagedThisSector ? 0 : balance.flawlessBonus));
            SetState(levelIndex == levels.Length - 1 ? SessionState.Victory : SessionState.SectorComplete);
            Stats.victory = State == SessionState.Victory;
            director.Clear(); projectiles.Clear();
            CommitRun(Mathf.Min(3, levelIndex + 2));
            if (audioService != null) audioService.Cue(State == SessionState.Victory ? AudioCue.Victory : AudioCue.Complete);
        }
        public void Lose()
        {
            if (!IsPlaying) return; Gate.Close(); SetState(SessionState.Defeat); player.gameObject.SetActive(false); CommitRun();
            if (audioService != null) audioService.Cue(AudioCue.Defeat);
        }
        public void NextSector() { if (State == SessionState.SectorComplete) { Health.Repair(25); BeginSector(levelIndex + 1); } }
        public void Restart() { if (State != SessionState.Menu) StartRun(levelIndex); }
        public void ReturnToMenu()
        {
            CommitRun(); runId = null; director.Clear(); projectiles.Clear(); player.gameObject.SetActive(false);
            Gate.Close(); GuestRun=false; CampaignMode=!quickStart;
            SetState(SessionState.Menu);
            if (backdrop != null) backdrop.ShowSector(0);
            if (audioService != null) audioService.PlayMusic(0);
        }
        private void CommitRun(int unlocked = 0)
        {
            if (runId != null && Profiles != null) Profiles.Record(runId, Stats, unlocked > 0 ? unlocked : Stats.maxSector);
        }
        public void ApplySettings()
        {
            var rig = Camera.main.GetComponent<CameraRig>(); if (rig != null) rig.ShakeEnabled = Settings.CameraShake;
            if (audioService != null) audioService.ApplyVolumes();
        }
        public void Quit() { CommitRun(); if (Application.isEditor) EditorQuitRequested?.Invoke(); else Application.Quit(); }
        public void Pause(bool value) { if (State == SessionState.Playing || State == SessionState.Paused) SetState(value ? SessionState.Paused : SessionState.Playing); }
        private void SetState(SessionState state)
        {
            State = state; Time.timeScale = state == SessionState.Playing || state == SessionState.Menu ? 1 : 0;
            input.SetFlight(state == SessionState.Playing);
            if (audioService != null) audioService.PauseCombat(state != SessionState.Playing);
            if (backdrop != null) backdrop.MenuVisible(state == SessionState.Menu);
            StateChanged?.Invoke(state);
        }
        private void OnApplicationFocus(bool focus) { if (!Application.isBatchMode && !focus && IsPlaying) Pause(true); }
        private void OnApplicationQuit() => CommitRun();
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
