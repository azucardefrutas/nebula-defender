using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NebulaDefender.Tests
{
    public sealed class CoreCombatTests
    {
        private GameSession session;
        private Keyboard keyboard;
        private Mouse mouse;
        private InputSettings.BackgroundBehavior originalBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
#endif
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CoreSandbox");
            yield return null;
            session=Object.FindFirstObjectByType<GameSession>();
            keyboard=InputSystem.AddDevice<Keyboard>();
            originalBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            originalEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.EnableDevice(keyboard);
            yield return null;
            Assert.That(session.IsPlaying,Is.True);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
            if(mouse!=null)InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior=originalBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=originalEditorInput;
#endif
            Time.timeScale=1;yield return null;
        }
        [UnityTest] public IEnumerator KeyboardThrustAndTurnMoveRigidbodyOnXZPlane()
        {
            var before=session.Player.Body.position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.D));
            InputSystem.Update();
            Assert.That(session.Input.FlightEnabled,Is.True);
            Assert.That(keyboard.wKey.ReadValue(),Is.EqualTo(1),"The simulated keyboard must receive the queued event.");
            var diagnostics=new System.Text.StringBuilder();
            foreach(var action in InputSystem.ListEnabledActions())if(action.name=="Throttle")
            { diagnostics.Append($"Action {action} = {action.ReadValue<float>()}; ");foreach(var control in action.controls)diagnostics.Append($"{control.path}={control.ReadValueAsObject()} "); }
            Assert.That(session.Input.Throttle,Is.EqualTo(1),"The real Flight action must read the simulated keyboard. "+diagnostics);
            yield return new WaitForSeconds(0.6f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            Assert.That(Vector3.Distance(before,session.Player.Body.position),Is.GreaterThan(1));
            Assert.That(Mathf.Abs(session.Player.Body.position.y),Is.LessThan(0.01f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(0,session.Player.Body.rotation.eulerAngles.y)),Is.GreaterThan(20));
            Assert.That(session.Player.Body.linearVelocity.magnitude,Is.LessThanOrEqualTo(session.Balance.maxSpeed+0.6f));
        }
        [UnityTest] public IEnumerator MouseAimsThrustsAndBrakesWithoutKeyboard()
        {
            session.Director.Clear(); yield return null;
            mouse=InputSystem.AddDevice<Mouse>(); InputSystem.EnableDevice(mouse);
            InputSystem.QueueStateEvent(mouse,new MouseState()); yield return null;
            var aim=Camera.main.WorldToScreenPoint(new Vector3(7,0,10));
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(aim.x,aim.y) }.WithButton(MouseButton.Right));
            yield return new WaitForSeconds(.55f);
            Assert.That(session.Input.MouseSteering,Is.True);
            Assert.That(session.Player.PointerAimValid,Is.True);
            Assert.That(session.Player.Body.position.magnitude,Is.GreaterThan(1));
            Assert.That(session.Player.Body.position.y,Is.EqualTo(0).Within(.01f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(0,session.Player.Body.rotation.eulerAngles.y)),Is.GreaterThan(12));
            float speed=session.Player.Body.linearVelocity.magnitude;
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(aim.x,aim.y) });
            yield return new WaitForSeconds(.4f);
            Assert.That(session.Player.Body.linearVelocity.magnitude,Is.LessThan(speed*.7f));
            session.Pause(true);
            var p=session.Player.Body.position; float time=session.Stats.seconds;
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(aim.x,aim.y) }.WithButton(MouseButton.Right).WithButton(MouseButton.Left));
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(session.Player.Body.position,Is.EqualTo(p));
            Assert.That(session.Stats.seconds,Is.EqualTo(time));
            Assert.That(session.Input.Fire,Is.False);
        }
        [UnityTest] public IEnumerator MouseAimAndLeftClickDestroyRockThroughRealProjectile()
        {
            session.Director.Clear(); yield return null;
            var rock=session.Director.SpawnRock(0,new Vector3(4,0,7),Vector3.zero);
            mouse=InputSystem.AddDevice<Mouse>(); InputSystem.EnableDevice(mouse);
            InputSystem.QueueStateEvent(mouse,new MouseState()); yield return null;
            var aim=Camera.main.WorldToScreenPoint(rock.transform.position);
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(aim.x,aim.y) });
            yield return new WaitForSeconds(.7f);
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(aim.x,aim.y) }.WithButton(MouseButton.Left));
            yield return new WaitForSeconds(.6f);
            Assert.That(rock==null,Is.True,"Mouse aiming and real laser contact must destroy the target.");
            Assert.That(session.Stats.score,Is.GreaterThanOrEqualTo(session.Balance.smallPoints));
        }
        [UnityTest] public IEnumerator MouseClickOnPauseCannotFireAndHeldButtonDoesNotLeakIntoFlight()
        {
            session.Director.Clear(); yield return null;
            mouse=InputSystem.AddDevice<Mouse>(); InputSystem.EnableDevice(mouse);
            InputSystem.QueueStateEvent(mouse,new MouseState()); yield return null;
            UnityEngine.UI.Button pauseButton=null;
            foreach(var button in Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None))
                if(button.name=="Button / II")pauseButton=button;
            Assert.That(pauseButton,Is.Not.Null);
            var rect=(RectTransform)pauseButton.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState { position=point });
            yield return null; yield return null;
            var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position=point },hits);
            Assert.That(hits.Count,Is.GreaterThan(0),$"UI raycast must hit pause: position={point}, screen={Screen.width}x{Screen.height}, rect={rect.rect}");
            Assert.That(hits[0].gameObject,Is.EqualTo(pauseButton.gameObject));
            InputSystem.QueueStateEvent(mouse,new MouseState { position=point }.WithButton(MouseButton.Left));
            InputSystem.Update();
            Assert.That(session.Input.Fire,Is.False,"UI must block the click before the next EventSystem update.");
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(session.Input.Fire,Is.False);
            InputSystem.QueueStateEvent(mouse,new MouseState { position=point });
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            InputSystem.QueueStateEvent(mouse,new MouseState { position=new Vector2(Screen.width*.5f,Screen.height*.5f) }.WithButton(MouseButton.Left));
            session.Pause(false);
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(session.Input.Fire,Is.False,"A click held while returning from a menu must first be released.");
            Assert.That(session.Projectiles.ActiveCount,Is.Zero);
        }
        [UnityTest] public IEnumerator RealLaserDestroysRockAndAwardsOnce()
        {
            session.Director.Clear();yield return null;
            var target=session.Director.SpawnRock(0,new Vector3(0,0,6),Vector3.zero);
            Assert.That(target,Is.Not.Null);
            int before=session.Stats.score;
            Assert.That(session.Weapon.TryFire(),Is.True);
            yield return new WaitForSeconds(0.35f);
            Assert.That(target==null,Is.True,"A real swept/trigger contact should destroy the small rock.");
            Assert.That(session.Stats.score,Is.EqualTo(before+session.Balance.smallPoints));
            Assert.That(session.Mission.Asteroids,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator LargeRockSplitsIntoTwoMediumRocksAndCannotAwardTwice()
        {
            session.Director.Clear();yield return null;
            var large=session.Director.SpawnRock(2,new Vector3(8,0,8),Vector3.right*2);
            large.Hit(999,large.transform.position);large.Hit(999,large.transform.position);
            yield return null;
            var all=Object.FindObjectsByType<AsteroidController>(FindObjectsSortMode.None);
            int medium=0;foreach(var rock in all)if(rock.Tier==1)medium++;
            Assert.That(medium,Is.EqualTo(2));Assert.That(session.Mission.Asteroids,Is.EqualTo(1));
            Assert.That(session.Stats.score,Is.EqualTo(session.Balance.largePoints));
        }
        [UnityTest] public IEnumerator PauseFreezesPhysicsTimersAndFireThenResumes()
        {
            session.Player.Body.linearVelocity=Vector3.forward*5;
            yield return new WaitForFixedUpdate();
            session.Pause(true);var p=session.Player.Body.position;float seconds=session.Stats.seconds;
            Assert.That(session.Weapon.TryFire(),Is.False);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(session.Player.Body.position,Is.EqualTo(p));Assert.That(session.Stats.seconds,Is.EqualTo(seconds));
            session.Pause(false);yield return new WaitForSeconds(0.15f);
            Assert.That(session.Stats.seconds,Is.GreaterThan(seconds));Assert.That(session.Player.Body.position.z,Is.GreaterThan(p.z));
        }
        [UnityTest] public IEnumerator ShieldCannotBeReactivatedDuringCooldown()
        {
            Assert.That(session.Health.ActivateShield(),Is.True);Assert.That(session.Health.ActivateShield(),Is.False);
            session.Health.Hit(100,Vector3.zero);Assert.That(session.Health.Vitals.Health,Is.EqualTo(100));
            yield return null;
        }
        [UnityTest] public IEnumerator ProjectilesExpireAndReturnToBoundedPool()
        {
            session.Director.Clear();yield return null;
            session.Projectiles.Fire(Team.Player,new Vector3(20,0,0),Vector3.forward,1,25,0.08f);
            Assert.That(session.Projectiles.ActiveCount,Is.EqualTo(1));
            yield return new WaitForSeconds(0.2f);Assert.That(session.Projectiles.ActiveCount,Is.Zero);
        }
        [UnityTest] public IEnumerator SpaceAndShiftActionsTriggerWeaponAndShield()
        {
            session.Director.Clear();yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space,Key.LeftShift));
            yield return new WaitForSeconds(0.08f);
            Assert.That(session.Projectiles.ActiveCount,Is.GreaterThan(0));Assert.That(session.Health.ShieldActive,Is.True);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        }
        [UnityTest] public IEnumerator EscapeActionPausesAndResumesWithoutDuplicateToggle()
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return new WaitForSecondsRealtime(0.08f);Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return new WaitForSecondsRealtime(0.08f);Assert.That(session.State,Is.EqualTo(SessionState.Playing));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        }
        [UnityTest] public IEnumerator EnterActivatesTheSelectedMenuAction()
        {
            session.ReturnToMenu();yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That(session.State,Is.EqualTo(SessionState.Playing));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        }
        [UnityTest] public IEnumerator F1HelpFreezesCombatAndReturnsToFlight()
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F1));
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            Assert.That(session.Weapon.TryFire(),Is.False);
            float seconds=session.Stats.seconds;
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(session.Stats.seconds,Is.EqualTo(seconds));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F1));
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(session.State,Is.EqualTo(SessionState.Playing));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        }
        [UnityTest] public IEnumerator Close3DCameraKeepsShipVisibleAndCChangesToTacticalView()
        {
            var camera=Camera.main;var rig=camera.GetComponent<CameraRig>();
            Assert.That(rig.TacticalView,Is.False);
            Assert.That(camera.transform.position.y,Is.LessThan(20));
            var visible=camera.WorldToViewportPoint(session.Player.Body.position);
            Assert.That(visible.z,Is.GreaterThan(0));
            Assert.That(visible.x,Is.InRange(0.2f,0.8f));Assert.That(visible.y,Is.InRange(0.15f,0.85f));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.C));
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(rig.TacticalView,Is.True);Assert.That(camera.transform.position.y,Is.GreaterThan(20));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.C));
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(rig.TacticalView,Is.False);Assert.That(camera.transform.position.y,Is.LessThan(20));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        }
    }
}
