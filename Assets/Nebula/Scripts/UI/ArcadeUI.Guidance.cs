using System;
using TMPro;
using UnityEngine;

namespace NebulaDefender
{
    public sealed partial class ArcadeUI
    {
        private TMP_Text firstGoal, secondGoal, bonus, guideTitle, guideText, guideDetail, scoreGain, toastTitle, toastBody;
        private RectTransform firstGoalFill, secondGoalFill, secondGoalTrack, shieldFill;
        private GameObject missionToast;
        private MissionProgress observedMission;
        private int lastScore, lastRocks, lastDrones, lastLives, lastHealth;
        private bool bossAnnounced, doubleSeen, triedThrust, triedTurn, triedFire, triedShield, helpVisible;
        private float toastUntil, scoreUntil;
        private Action returnFromHelp;
        private Camera gameplayCamera;
        private CameraRig flightCamera;
        private UnityEngine.UI.Button cameraView;
        private RectTransform objectiveMarker;
        private TMP_Text objectiveArrow, objectiveCaption;

        public void Briefing(int index)
        {
            index = Mathf.Clamp(index, 0, session.Levels.Length - 1);
            var level = session.Levels[index];
            var card = Modal($"SECTOR {index + 1:00} / PREPARACIÓN", level.displayName, 1230, 910);
            Label(card, "TU MISIÓN", 36, 154, 1110, 36, 20, cyan);
            Label(card, level.MissionSteps, 36, 208, 1140, 140, 36);
            Box("Briefing separator", card, 36, 372, 1158, 2, panel);
            Label(card, "PLAN DE VUELO", 36, 405, 1100, 35, 20, cyan);
            Label(card, level.TacticalAdvice, 36, 455, 1158, 100, 28, muted);
            Label(card, $"SOBREVIVE: {session.Balance.startingLives} vidas · {session.Balance.healthPerLife} de salud por vida.\nBONO: +{session.Balance.flawlessBonus} puntos si completas el sector sin recibir daño.", 36, 581, 1158, 90, 25);
            Label(card, "Los bordes conectan con el lado opuesto. Al cumplir la misión, las amenazas restantes se retiran.", 36, 688, 1158, 63, 23, muted);
            Button(card, "COMENZAR MISIÓN", 36, 800, 530, () => session.StartRun(index), true, true);
            Button(card, "CONTROLES", 590, 800, 300, () => ShowHelp(() => Briefing(index)));
            Button(card, "VOLVER", 914, 800, 280, Sectors);
            FocusFirst();
        }

        private void ShowHelp(Action onReturn)
        {
            var card = Modal("GUÍA DE PILOTAJE / F1", "CÓMO JUGAR", 1260, 950);
            returnFromHelp = onReturn; helpVisible = true;
            Label(card, "CONTROLES", 36, 157, 540, 35, 20, cyan);
            string[] keys = { "W / ARRIBA", "S / ABAJO", "A D / FLECHAS", "ESPACIO", "SHIFT", "ESC", "F1", "C" };
            string[] descriptions = { "Acelerar hacia la proa", "Frenar / propulsión inversa", "Girar para apuntar", "Disparar (mantén pulsado)", "Activar el escudo", "Pausa / continuar", "Abrir / cerrar esta guía", "Vista 3D / vista táctica" };
            for (int i = 0; i < keys.Length; i++)
            {
                float y = 211 + i * 60;
                Box("Key row", card, 36, y - 5, 552, 55, panel);
                Label(card, keys[i], 49, y + 4, 205, 43, 22, cyan);
                Label(card, descriptions[i], 259, y + 4, 320, 43, 23);
            }
            Label(card, "APUNTA CON LA PROA", 634, 157, 590, 35, 20, cyan);
            Label(card, "La cámara sigue a tu nave desde atrás. Gira con A/D, usa impulsos cortos y frena con S. Pulsa C para ver toda la zona desde arriba.", 634, 217, 590, 150, 28);
            Label(card, "DESTRUYE Y SOBREVIVE", 634, 386, 590, 35, 20, cyan);
            Label(card, "Cada asteroide o fragmento destruido suma uno al objetivo. Los drones deben eliminarse por separado. El jefe aparece tras destruir sus defensas.", 634, 440, 590, 166, 27, muted);
            Label(card, $"ESCUDO: {session.Balance.shieldDuration:0.0} s de protección / {session.Balance.shieldCooldown:0} s de recarga.\nRECOGIDAS: reparación +30 y láser doble temporal.", 36, 732, 1188, 81, 25, muted);
            Button(card, session.State == SessionState.Paused ? "VOLVER AL VUELO / PAUSA" : "VOLVER", 36, 847, 1188, CloseHelp, true, true);
            FocusFirst();
        }

        private void CloseHelp() { var back = returnFromHelp; helpVisible = false; back?.Invoke(); }
        private void FlightHelp() { session.Pause(true); ShowHelp(() => session.Pause(false)); }

        private void BuildHUD()
        {
            hud = Rect("Flight HUD", transform, 0, 0, 1920, 1080);
            gameplayCamera = Camera.main; flightCamera = gameplayCamera.GetComponent<CameraRig>();
            var health = Box("Ship integrity", hud, 24, 22, 450, 142, ink);
            Box("Accent", health, 0, 0, 4, 142, cyan);
            telemetry = Label(health, "", 22, 15, 410, 75, 27);
            Box("Health track", health, 22, 111, 400, 10, panel);
            healthFill = Box("Health", health, 22, 111, 400, 10, cyan);
            var points = Box("Score", hud, 490, 22, 300, 142, ink);
            Label(points, "PUNTUACIÓN", 20, 12, 260, 30, 18, muted);
            score = Label(points, "", 20, 47, 270, 50, 37, cyan);
            scoreGain = Label(points, "", 20, 102, 270, 30, 23, new Color(0.45f, 1, 0.75f));
            Button(hud, "PAUSA", 810, 22, 163, () => session.Pause(true));
            Button(hud, "AYUDA F1", 990, 22, 163, FlightHelp);
            cameraView = Button(hud, "VISTA 3D [C]", 810, 100, 343, () => flightCamera.ToggleView());
            var goals = Box("Mission objectives", hud, 1175, 22, 721, 224, ink);
            Box("Accent", goals, 0, 0, 4, 224, cyan);
            objective = Label(goals, "", 22, 14, 677, 32, 23, cyan);
            firstGoal = Label(goals, "", 22, 62, 677, 38, 27);
            Box("First goal track", goals, 22, 106, 677, 6, panel);
            firstGoalFill = Box("First goal progress", goals, 22, 106, 677, 6, cyan);
            secondGoal = Label(goals, "", 22, 126, 677, 38, 25);
            secondGoalTrack = Box("Second goal track", goals, 22, 169, 677, 6, panel);
            secondGoalFill = Box("Second goal progress", secondGoalTrack, 0, 0, 677, 6, cyan);
            bonus = Label(goals, "", 22, 191, 677, 28, 19);

            bossPanel = Box("Nemesis telemetry", hud, 535, 320, 640, 83, ink).gameObject;
            bossLabel = Label(bossPanel.transform, "", 20, 12, 600, 36, 25, new Color(1, 0.55f, 0.3f));
            Box("Boss track", bossPanel.transform, 20, 65, 600, 7, panel);
            bossFill = Box("Boss health", bossPanel.transform, 20, 65, 600, 7, new Color(1, 0.32f, 0.15f));
            missionToast = Box("Mission announcement", hud, 535, 182, 618, 120, ink).gameObject;
            toastTitle = Label(missionToast.transform, "", 20, 13, 578, 30, 20, cyan);
            toastBody = Label(missionToast.transform, "", 20, 49, 578, 65, 27);
            objectiveMarker = Box("Offscreen objective", hud, 40, 400, 210, 66, ink);
            objectiveArrow = Label(objectiveMarker, ">", 10, 8, 40, 48, 39, cyan);
            objectiveArrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            objectiveCaption = Label(objectiveMarker, "", 62, 10, 140, 50, 20, cyan);

            var guidance = Box("Pilot guidance", hud, 24, 831, 1128, 170, ink);
            guideTitle = Label(guidance, "", 22, 13, 1084, 30, 19, cyan);
            guideText = Label(guidance, "", 22, 50, 1084, 70, 29);
            guideDetail = Label(guidance, "", 22, 133, 1084, 31, 21, muted);
            var defense = Box("Shield status", hud, 1175, 831, 721, 170, ink);
            Label(defense, "DEFENSA / SHIFT", 22, 13, 677, 32, 20, cyan);
            shield = Label(defense, "", 22, 57, 677, 43, 29);
            Box("Shield track", defense, 22, 119, 677, 8, panel);
            shieldFill = Box("Shield charge", defense, 22, 119, 677, 8, cyan);
            var bottom = Box("Flight reference", hud, 24, 1006, 1872, 50, ink);
            Label(bottom, "W/S PROPULSIÓN   A/D GIRO   ESPACIO LÁSER   SHIFT ESCUDO   C VISTA   F1 AYUDA", 18, 12, 1270, 35, 19, muted);
            timer = Label(bottom, "", 1300, 12, 550, 35, 21); timer.alignment = TextAlignmentOptions.MidlineRight;
        }

        private void Announce(string title, string body)
        { toastTitle.text = title; toastBody.text = body; toastUntil = Time.time + 4.5f; }

        private void ObserveMission()
        {
            if (observedMission == session.Mission) return;
            observedMission = session.Mission;
            lastScore = session.Stats.score; lastRocks = lastDrones = 0;
            lastLives = session.Health.Vitals.Lives; lastHealth = session.Health.Vitals.Health;
            bossAnnounced = doubleSeen = false; scoreUntil = 0;
            Announce(session.Sandbox ? "VUELO DE PRÁCTICA" : $"SECTOR {session.Level.sector:00} / MISIÓN", session.Level.Objective);
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
            if (session.IsPlaying)
            {
                triedThrust |= session.Input.Throttle > 0.1f;
                triedTurn |= Mathf.Abs(session.Input.Turn) > 0.1f;
                triedFire |= session.Input.Fire;
                triedShield |= session.Health.ShieldActive;
            }
            if (Time.unscaledTime < nextHud) return;
            nextHud = Time.unscaledTime + 0.1f;
            ObserveMission();
            var v = session.Health.Vitals;
            telemetry.text = $"CASCO {v.Health:000} / {v.MaxHealth}\nVIDAS {v.Lives}";
            healthFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400 * (float)v.Health / v.MaxHealth);
            healthFill.GetComponent<UnityEngine.UI.Image>().color = v.Health <= 30 ? new Color(1, 0.35f, 0.25f) : cyan;
            if (session.Stats.score > lastScore) { scoreGain.text = $"+{session.Stats.score - lastScore} PUNTOS"; scoreUntil = Time.time + 2; }
            scoreGain.gameObject.SetActive(Time.time < scoreUntil);
            score.text = $"{session.Stats.score:000000}";
            UpdateGoals(); UpdateGuidance();
            UpdateObjectiveMarker();
            cameraView.GetComponentInChildren<TMP_Text>().text = flightCamera.TacticalView ? "VISTA TÁCTICA [C]" : "VISTA 3D [C]";
            if (v.Lives < lastLives) Announce("NAVE RECONSTRUIDA", $"Te quedan {v.Lives} vidas. Tienes protección temporal.");
            else if (v.Health > lastHealth) Announce("REPARACIÓN RECOGIDA", $"+{v.Health - lastHealth} de salud. Continúa la misión.");
            if (session.Weapon.DoubleShot && !doubleSeen) Announce("MEJORA RECOGIDA", "Láser doble durante 14 segundos. Mantén Espacio.");
            doubleSeen = session.Weapon.DoubleShot;
            lastHealth = v.Health; lastLives = v.Lives; lastScore = session.Stats.score;
            missionToast.SetActive(session.IsPlaying && Time.time < toastUntil);
            var boss = session.Director.Boss; bossPanel.SetActive(boss != null);
            if (boss != null) { bossLabel.text = $"NEMESIS PRIME · FASE {boss.Phase} · {boss.HealthFraction:P0}"; bossFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600 * boss.HealthFraction); }
            timer.text = $"{TimeLabel(session.Stats.seconds)}   /   EN PANTALLA {session.Director.RockCount + session.Director.DroneCount + (boss != null ? 1 : 0)}";
        }

        private void UpdateGoals()
        {
            var m = session.Mission; var level = session.Level; var boss = session.Director.Boss;
            objective.text = $"SECTOR {level.sector:00} / {level.displayName}";
            int count = level.boss ? m.Drones : m.Asteroids;
            int goal = level.boss ? m.DroneGoal : m.AsteroidGoal;
            firstGoal.text = (level.boss ? "DEFENSAS" : "ASTEROIDES") + $"  {Mathf.Min(count, goal)}/{goal}" + (count >= goal ? " · COMPLETADO" : $" · FALTAN {goal - count}");
            firstGoalFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 677 * Mathf.Clamp01((float)count / Mathf.Max(1, goal)));
            firstGoal.color = count >= goal ? new Color(0.45f, 1, 0.75f) : text;
            secondGoalTrack.gameObject.SetActive(level.boss || m.DroneGoal > 0);
            if (level.boss)
            {
                secondGoal.text = boss != null ? $"NEMESIS PRIME · DAÑO {1 - boss.HealthFraction:P0}" : count < goal ? "JEFE · TRAS LAS DEFENSAS" : "JEFE · APROXIMÁNDOSE";
                secondGoalFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boss != null ? 677 * (1 - boss.HealthFraction) : 0);
                if (boss != null && !bossAnnounced) { bossAnnounced = true; Announce("OBJETIVO FINAL / NEMESIS PRIME", "Destruye la nave insignia. Esquiva sus salvas."); }
            }
            else if (m.DroneGoal > 0)
            {
                secondGoal.text = $"DRONES  {Mathf.Min(m.Drones, m.DroneGoal)}/{m.DroneGoal}" + (m.Drones >= m.DroneGoal ? " · COMPLETADO" : $" · FALTAN {m.DroneGoal - m.Drones}");
                secondGoalFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 677 * Mathf.Clamp01((float)m.Drones / m.DroneGoal));
                if (m.Asteroids >= m.AsteroidGoal && lastRocks < m.AsteroidGoal && m.Drones < m.DroneGoal)
                    Announce("ASTEROIDES ASEGURADOS", $"Elimina los {m.DroneGoal - m.Drones} drones restantes.");
                if (m.Drones >= m.DroneGoal && lastDrones < m.DroneGoal && m.Asteroids < m.AsteroidGoal)
                    Announce("DRONES NEUTRALIZADOS", $"Destruye los {m.AsteroidGoal - m.Asteroids} asteroides restantes.");
            }
            else secondGoal.text = "CADA FRAGMENTO TAMBIÉN CUENTA";
            bonus.text = session.DamagedThisSector ? "BONO SIN DAÑO: PERDIDO · LA MISIÓN CONTINÚA" : $"BONO SIN DAÑO: +{session.Balance.flawlessBonus} PTS DISPONIBLE";
            bonus.color = session.DamagedThisSector ? muted : new Color(0.45f, 1, 0.75f);
            lastRocks = m.Asteroids; lastDrones = m.Drones;
        }

        private void UpdateGuidance()
        {
            int learned = (triedThrust ? 1 : 0) + (triedTurn ? 1 : 0) + (triedFire ? 1 : 0) + (triedShield ? 1 : 0);
            guideTitle.text = learned < 4 ? $"APRENDE EL VUELO / {learned} DE 4 CONTROLES PROBADOS" : "CONSEJO DE VUELO / F1 PARA CONSULTAR LA GUÍA";
            guideText.text = !triedThrust ? "Mantén W o ARRIBA para acelerar." : !triedTurn ? "Pulsa A/D o las flechas para girar y apuntar." : !triedFire ? "Mantén ESPACIO para disparar." : !triedShield ? "Pulsa SHIFT para probar el escudo." : session.Health.Vitals.Health <= 30 ? "Salud baja: aléjate y busca una reparación." : session.Level.TacticalAdvice;
            guideText.fontSize = learned < 4 ? 29 : 25;
            guideDetail.text = learned < 4 ? "El cañón dispara hacia la proa. C cambia la vista. F1 abre la ayuda." : "Sigue el indicador cian para encontrar objetivos. C muestra la vista táctica.";
            var h = session.Health;
            shield.text = h.ShieldActive ? $"ACTIVO · {h.ShieldActiveRemaining:0.0} s" : h.ShieldRemaining > 0 ? $"RECARGA · {h.ShieldRemaining:0.0} s" : h.SafetyRemaining > 0 ? $"PROTECCIÓN · {h.SafetyRemaining:0.0} s" : "LISTO · PULSA SHIFT";
            float charge = h.ShieldActive ? h.ShieldActiveRemaining / Mathf.Max(0.01f, session.Balance.shieldDuration) : 1 - h.ShieldRemaining / Mathf.Max(0.01f, session.Balance.shieldCooldown);
            shieldFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 677 * Mathf.Clamp01(charge));
        }
        private void UpdateObjectiveMarker()
        {
            var target = session.Director.NearestObjective();
            if (target == null || flightCamera.TacticalView) { objectiveMarker.gameObject.SetActive(false); return; }
            var v = gameplayCamera.WorldToViewportPoint(target.transform.position);
            bool outside = v.z < 0 || v.x < .08f || v.x > .92f || v.y < .22f || v.y > .78f;
            objectiveMarker.gameObject.SetActive(outside);
            if (!outside) return;
            var direction = new Vector2(v.x - .5f, v.y - .5f);
            if (v.z < 0) direction = -direction;
            if (direction.sqrMagnitude < .001f) direction = Vector2.down;
            direction.Normalize();
            objectiveMarker.anchoredPosition = new Vector2(Mathf.Clamp(960 + direction.x * 820 - 105, 24, 1686), -Mathf.Clamp(540 - direction.y * 300 - 33, 265, 756));
            objectiveArrow.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            objectiveCaption.text = (target is BossController ? "NÉMESIS" : target is EnemyController ? "DRON" : "ASTEROIDE") + "\nACÉRCATE";
        }
    }
}
