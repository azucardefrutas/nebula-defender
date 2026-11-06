using System;
using TMPro;
using UnityEngine;

namespace NebulaDefender
{
    public sealed partial class ArcadeUI
    {
        private TMP_Text scoreGain, toastTitle, toastBody, targetCaption;
        private RectTransform goalFill, shieldFill, reticle, targetMarker;
        private GameObject missionToast, hintPanel;
        private MissionProgress observedMission;
        private int lastScore, lastLives, lastHealth;
        private bool doubleSeen, helpVisible;
        private float toastUntil, scoreUntil, hintUntil;
        private Action returnFromHelp;
        private Camera gameplayCamera;
        private CameraRig flightCamera;
        private UnityEngine.UI.Button cameraView;

        public void Briefing(int index)
        {
            index = Mathf.Clamp(index, 0, session.Levels.Length - 1);
            var level = session.Levels[index];
            var card = Modal($"SECTOR {index + 1:00}", level.displayName, 1040, 590);
            Label(card, level.MissionSteps, 36, 178, 968, 135, 32);
            Label(card, "Mouse apunta · Clic dispara · Derecho avanza", 36, 338, 968, 44, 26, cyan);
            Button(card, "DESPEGAR", 36, 470, 530, () => session.StartRun(index), true, true);
            Button(card, "AYUDA", 586, 470, 190, () => ShowHelp(() => Briefing(index)));
            Button(card, "VOLVER", 796, 470, 208, Sectors);
            FocusFirst();
        }
        private void ShowHelp(Action onReturn)
        {
            var card = Modal("F1 / AYUDA", "TÚ PILOTAS", 1020, 770);
            returnFromHelp = onReturn; helpVisible = true;
            string[] keys = { "MOUSE", "CLIC IZQUIERDO", "CLIC DERECHO", "CENTRAL / SHIFT" };
            string[] descriptions = { "Apunta", "Dispara", "Mantén para avanzar", "Pulsa para el escudo" };
            for (int i = 0; i < keys.Length; i++)
            {
                float y = 185 + i * 78;
                Box("Control row", card, 36, y, 948, 65, panel);
                Label(card, keys[i], 54, y + 15, 390, 40, 26, cyan);
                Label(card, descriptions[i], 454, y + 15, 510, 40, 27);
            }
            Label(card, "También: WASD + Espacio\nC cambia cámara · ESC pausa", 36, 525, 948, 100, 27, muted);
            Button(card, "LISTO", 36, 664, 948, CloseHelp, true, true);
            FocusFirst();
        }
        private void CloseHelp() { var back = returnFromHelp; helpVisible = false; back?.Invoke(); }
        private void FlightHelp() { session.Pause(true); ShowHelp(() => session.Pause(false)); }

        private void BuildHUD()
        {
            hud = Rect("Flight HUD", transform, 0, 0, 1920, 1080);
            gameplayCamera = Camera.main; flightCamera = gameplayCamera.GetComponent<CameraRig>();
            var health = Box("Ship integrity", hud, 28, 26, 350, 96, ink);
            telemetry = Label(health, "", 18, 12, 314, 42, 27);
            Box("Hull track", health, 18, 70, 314, 7, panel);
            healthFill = Box("Hull", health, 18, 70, 314, 7, cyan);
            var goal = Box("Objective", hud, 630, 26, 660, 96, ink);
            objective = Label(goal, "", 20, 12, 620, 37, 25, cyan);
            Box("Goal track", goal, 20, 70, 620, 5, panel);
            goalFill = Box("Goal progress", goal, 20, 70, 620, 5, cyan);
            var points = Box("Score", hud, 1462, 26, 230, 96, ink);
            score = Label(points, "", 18, 13, 200, 43, 34);
            scoreGain = Label(points, "", 18, 58, 200, 32, 22, cyan);
            Button(hud, "II", 1710, 26, 82, () => session.Pause(true));
            Button(hud, "?", 1810, 26, 82, FlightHelp);
            bossPanel = Box("Nemesis telemetry", hud, 630, 140, 660, 68, ink).gameObject;
            bossLabel = Label(bossPanel.transform, "", 20, 8, 620, 36, 24, new Color(1, .5f, .3f));
            Box("Boss track", bossPanel.transform, 20, 53, 620, 6, panel);
            bossFill = Box("Boss health", bossPanel.transform, 20, 53, 620, 6, new Color(1, .35f, .15f));
            missionToast = Rect("Announcement", hud, 630, 228, 660, 82).gameObject;
            toastTitle = Label(missionToast.transform, "", 0, 0, 660, 40, 30, cyan);
            toastTitle.alignment = TextAlignmentOptions.Center;
            toastBody = Label(missionToast.transform, "", 0, 44, 660, 36, 24);
            toastBody.alignment = TextAlignmentOptions.Center;
            var defense = Box("Shield", hud, 28, 980, 350, 72, ink);
            shield = Label(defense, "", 18, 8, 314, 36, 23, cyan);
            Box("Shield track", defense, 18, 52, 314, 5, panel);
            shieldFill = Box("Shield charge", defense, 18, 52, 314, 5, cyan);
            cameraView = Button(hud, "C / TÁCTICA", 1622, 992, 270, () => flightCamera.ToggleView());
            hintPanel = Box("Flight tip", hud, 520, 993, 880, 59, ink).gameObject;
            var hint = Label(hintPanel.transform, "Mouse apunta · Clic dispara · Derecho avanza", 16, 12, 848, 37, 24, muted);
            hint.alignment = TextAlignmentOptions.Center;
            timer = Label(hud, "", 28, 936, 350, 32, 20, muted);
            targetMarker = Box("Target direction", hud, 40, 400, 180, 54, ink);
            targetCaption = Label(targetMarker, "", 10, 8, 160, 38, 23, cyan);
            targetCaption.alignment = TextAlignmentOptions.Center;
            reticle = Rect("Aim reticle", hud, 960, 540, 40, 40);
            reticle.pivot = new Vector2(.5f, .5f);
            Box("Left", reticle, 0, 18, 10, 3, cyan); Box("Right", reticle, 30, 18, 10, 3, cyan);
            Box("Top", reticle, 18, 0, 3, 10, cyan); Box("Bottom", reticle, 18, 30, 3, 10, cyan);
            Box("Dot", reticle, 18, 18, 3, 3, text);
        }
        private void Announce(string title, string body = "")
        { toastTitle.text = title; toastBody.text = body; toastUntil = Time.time + 2.2f; }
        private void ObserveMission()
        {
            if (observedMission == session.Mission) return;
            observedMission = session.Mission; lastScore = session.Stats.score;
            lastLives = session.Health.Vitals.Lives; lastHealth = session.Health.Vitals.Health;
            doubleSeen = false; scoreUntil = 0; hintUntil = Time.time + 12;
            Announce(session.Sandbox ? "VUELO LIBRE" : session.Level.displayName);
        }
        private void Update()
        {
            if (hud == null) return;
            if (session.Input.HelpPressed)
            {
                if (helpVisible) CloseHelp();
                else if (session.IsPlaying) FlightHelp();
                else if (session.State == SessionState.Paused) ShowHelp(PauseMenu);
                else if (session.State == SessionState.Menu) ShowHelp(MainMenu);
            }
            if (!hud.gameObject.activeSelf || session.Mission == null) return;
            bool showAim = session.IsPlaying && session.Input.MouseSteering && session.Player.PointerAimValid;
            Cursor.visible = !showAim;
            reticle.gameObject.SetActive(showAim);
            if (showAim)
            {
                var p = session.Input.PointerPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(hud,p,null,out var local);
                reticle.anchoredPosition = local;
            }
            if (Time.unscaledTime < nextHud) return;
            nextHud = Time.unscaledTime + .1f; ObserveMission();
            var v = session.Health.Vitals;
            telemetry.text = $"{v.Health:000}   /   {v.Lives} VIDAS";
            healthFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 314f * v.Health / v.MaxHealth);
            healthFill.GetComponent<UnityEngine.UI.Image>().color = v.Health <= 30 ? new Color(1, .35f, .25f) : cyan;
            if (session.Stats.score > lastScore) { scoreGain.text = $"+{session.Stats.score - lastScore}"; scoreUntil = Time.time + 1.5f; }
            scoreGain.gameObject.SetActive(Time.time < scoreUntil); score.text = $"{session.Stats.score:000000}";
            var m = session.Mission; var b = session.Director.Boss;
            objective.text = session.Level.boss ? $"DEFENSAS  {Mathf.Min(m.Drones,m.DroneGoal)}/{m.DroneGoal}" : $"ASTEROIDES  {Mathf.Min(m.Asteroids,m.AsteroidGoal)}/{m.AsteroidGoal}" + (m.DroneGoal > 0 ? $"    DRONES  {Mathf.Min(m.Drones,m.DroneGoal)}/{m.DroneGoal}" : "");
            float progress = (float)(Mathf.Min(m.Asteroids,m.AsteroidGoal) + Mathf.Min(m.Drones,m.DroneGoal)) / Mathf.Max(1,m.AsteroidGoal + m.DroneGoal);
            goalFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 620 * progress);
            if (v.Lives < lastLives) Announce("OTRA OPORTUNIDAD", $"{v.Lives} vidas");
            else if (v.Health > lastHealth) Announce("+ REPARACIÓN");
            if (session.Weapon.DoubleShot && !doubleSeen) Announce("LÁSER DOBLE");
            doubleSeen = session.Weapon.DoubleShot; lastHealth = v.Health; lastLives = v.Lives; lastScore = session.Stats.score;
            missionToast.SetActive(session.IsPlaying && Time.time < toastUntil);
            bossPanel.SetActive(b != null);
            if (b != null) { bossLabel.text = $"NEMESIS PRIME   /   {b.HealthFraction:P0}"; bossFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 620 * b.HealthFraction); }
            var h = session.Health;
            shield.text = h.ShieldActive ? "ESCUDO ACTIVO" : h.ShieldRemaining > 0 ? $"ESCUDO  {h.ShieldRemaining:0.0}s" : "ESCUDO LISTO / SHIFT";
            shieldFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 314 * Mathf.Clamp01(1 - h.ShieldRemaining / Mathf.Max(.01f,session.Balance.shieldCooldown)));
            cameraView.GetComponentInChildren<TMP_Text>().text = flightCamera.TacticalView ? "C / VISTA 3D" : "C / TÁCTICA";
            hintPanel.SetActive(Time.time < hintUntil);
            timer.text = TimeLabel(session.Stats.seconds);
            UpdateTargetMarker();
        }
        private void UpdateTargetMarker()
        {
            var target = session.Director.NearestObjective();
            if (target == null || flightCamera.TacticalView || !session.IsPlaying) { targetMarker.gameObject.SetActive(false); return; }
            var v = gameplayCamera.WorldToViewportPoint(target.transform.position);
            bool outside = v.z < 0 || v.x < .05f || v.x > .95f || v.y < .12f || v.y > .85f;
            targetMarker.gameObject.SetActive(outside); if (!outside) return;
            var direction = new Vector2(v.x-.5f,v.y-.5f); if (v.z < 0) direction = -direction;
            if (direction.sqrMagnitude < .001f) direction = Vector2.down; direction.Normalize();
            targetMarker.anchoredPosition = new Vector2(Mathf.Clamp(960+direction.x*820-90,28,1712),-Mathf.Clamp(540-direction.y*400-27,148,908));
            string arrow = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? (direction.x > 0 ? ">" : "<") : (direction.y > 0 ? "^" : "v");
            targetCaption.text = arrow + (target is BossController ? "  NÉMESIS" : target is EnemyController ? "  DRON" : "  ROCA");
        }
    }
}
