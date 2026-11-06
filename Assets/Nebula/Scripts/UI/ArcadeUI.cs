using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace NebulaDefender
{
    /// <summary>uGUI presentation. Gameplay, profiles and persistence remain owned by their respective systems.</summary>
    public sealed partial class ArcadeUI : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private TMP_FontAsset font;
        private RectTransform page, hud;
        private TMP_Text telemetry, objective, score, timer, shield, bossLabel, notification;
        private RectTransform healthFill, bossFill;
        private GameObject bossPanel;
        private float nextHud;
        private int pilotPage;
        private bool settingsFromPause;
        private readonly Color ink = new Color(0.025f,0.045f,0.08f,0.96f);
        private readonly Color panel = new Color(0.045f,0.09f,0.14f,0.97f);
        private readonly Color text = new Color(0.83f,0.91f,0.96f);
        private readonly Color muted = new Color(0.62f,0.75f,0.83f);
        private readonly Color cyan = new Color(0.18f,0.9f,1);
        private Button first;
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        public void Configure(GameSession owner, TMP_FontAsset typeface) { session = owner; font = typeface; }
        private IEnumerator Start()
        {
            GetComponent<CanvasScaler>().screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            yield return null;
            var module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (module.actionsAsset == null) module.AssignDefaultActions();
            module.submit.action.AddBinding("<Keyboard>/enter");
            foreach (var resolution in Screen.resolutions)
            {
                var size = new Vector2Int(resolution.width,resolution.height);
                if (size.x >= 960 && size.y >= 540 && !resolutions.Contains(size)) resolutions.Add(size);
            }
            if (resolutions.Count == 0) resolutions.AddRange(new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)});
            BuildHUD(); session.StateChanged += StateChanged; StateChanged(session.State);
        }
        private void OnDestroy() { Cursor.visible=true;if (session != null) session.StateChanged -= StateChanged; }
        private RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go = new GameObject(name,typeof(RectTransform));var r = go.GetComponent<RectTransform>();
            r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        private RectTransform Box(string name,Transform parent,float x,float y,float w,float h,Color color)
        { var r=Rect(name,parent,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return r; }
        private TMP_Text Label(Transform parent,string value,float x,float y,float w,float h,int size,Color? color=null)
        {
            var r=Rect("Text / "+value,parent,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font=font;t.fontSize=size;t.color=color??text;t.text=value;t.richText=false;t.raycastTarget=false;
            t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        private Button Button(Transform parent,string value,float x,float y,float w,Action action,bool enabled=true,bool primary=false)
        {
            var r=Box("Button / "+value,parent,x,y,w,60,primary?new Color(0.06f,0.27f,0.35f):panel);
            r.GetComponent<Image>().raycastTarget=true;
            var b=r.gameObject.AddComponent<Button>();var colors=b.colors;
            colors.normalColor=Color.white;colors.highlightedColor=new Color(0.45f,0.9f,1);colors.selectedColor=new Color(0.4f,0.8f,1);colors.pressedColor=new Color(0.2f,0.6f,0.7f);colors.disabledColor=new Color(0.4f,0.45f,0.5f);b.colors=colors;
            b.interactable=enabled;
            Box("Accent",r,0,0,3,60,enabled?cyan:muted);
            var label=Label(r,value,24,14,w-42,35,23,enabled?text:muted);label.alignment=TextAlignmentOptions.MidlineLeft;
            b.onClick.AddListener(()=>{ if(session.Audio!=null)session.Audio.Cue(AudioCue.Confirm);action(); });
            r.gameObject.AddComponent<ButtonFeedback>().Initialize(session.Audio);
            if(first==null&&enabled)first=b;return b;
        }
        private void ClearPage()
        {
            helpVisible=false;
            EventSystem.current.SetSelectedGameObject(null); first=null;
            if(page!=null){page.gameObject.SetActive(false);Destroy(page.gameObject);}
            page=Rect("Active screen",transform,0,0,1920,1080);
        }
        private void FocusFirst() { if(first!=null)EventSystem.current.SetSelectedGameObject(first.gameObject); }
        private RectTransform Modal(string kicker,string title,float width=860,float height=790)
        {
            ClearPage();Box("Dim",page,0,0,1920,1080,new Color(0.005f,0.012f,0.025f,0.72f));
            var card=Box("Screen / "+title,page,(1920-width)/2,(1080-height)/2,width,height,ink);
            Box("Top rule",card,0,0,width,3,cyan);
            Label(card,kicker,36,27,width-72,30,16,cyan);Label(card,title,36,68,width-72,65,42);
            return card;
        }
        private void StateChanged(SessionState state)
        {
            if(state!=SessionState.Playing)Cursor.visible=true;
            if(hud==null)return;
            hud.gameObject.SetActive(state==SessionState.Playing||state==SessionState.Paused);
            if(state==SessionState.Menu)MainMenu();
            else if(state==SessionState.Paused)PauseMenu();
            else if(state==SessionState.Playing){ClearPage();}
            else Results();
        }
        public void MainMenu()
        {
            if(session.Sandbox){SandboxMenu();return;}
            ClearPage();
            Box("Navigation backdrop",page,0,0,815,1080,new Color(0.012f,0.026f,0.05f,0.92f));
            Box("Left accent",page,62,84,4,46,cyan);
            Label(page,"UPB  /  SCIENCE & TECHNOLOGY 2026",88,90,650,30,18,cyan);
            Label(page,"NEBULA\nDEFENDER",82,164,705,218,92);
            Label(page,"G A L A C T I C   A S S A U L T",89,390,660,36,23,cyan);
            Label(page,"Tres sectores. Una última defensa.",90,451,620,70,25,muted);
            Button(page,"JUGAR",90,532,600,()=>{if(session.Profiles?.Selected==null)Pilots();else Sectors();},true,true);
            Button(page,"CÓMO JUGAR",90,602,600,()=>ShowHelp(MainMenu));
            Button(page,"PILOTOS",90,672,600,Pilots);
            Button(page,"RÉCORDS",90,742,600,Ranking);
            Button(page,"OPCIONES",90,812,600,()=>Settings(false));
#if UNITY_WEBGL && !UNITY_EDITOR
            Button(page,"VUELO LIBRE",90,882,600,()=>SceneManager.LoadScene("CoreSandbox"));
#else
            Button(page,"SALIR",90,882,600,session.Quit);
#endif
            Label(page,session.Profiles?.Selected!=null?"PILOTO ACTIVO  /  "+session.Profiles.Selected.nickname:"SIN PILOTO  /  REGISTRO LOCAL",90,962,680,34,19,cyan);
            Label(page,"UNIVERSIDAD POLITÉCNICA DE BACALAR\nMtro. Esdras Chuc C. · Arcade 3D independiente / Offline",90,1000,690,58,16,muted);
            Label(page,"ND–01\nAURORA INTERCEPTOR",1280,770,520,80,25,cyan);
            Label(page,"MOUSE + CLIC  /  TÚ PILOTAS",1140,1000,700,36,20,muted);
            Notice(page,850,915,1000);FocusFirst();
        }
        private void SandboxMenu()
        {
            var card=Modal("VUELO LIBRE","VUELO DE PRUEBA",900,650);
            Label(card,"Apunta con el mouse. Destruye y sobrevive.",36,166,820,100,26,muted);
            Button(card,"INICIAR VUELO",36,320,828,()=>session.StartRun(0),true,true);
            Button(card,"ABRIR CAMPAÑA",36,405,828,()=>{Time.timeScale=1;SceneManager.LoadScene("Campaign");});
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            Button(card,"SALIR",36,490,828,session.Quit);
#endif
            FocusFirst();
        }
        private void Notice(Transform parent,float x,float y,float width)
        {
            string notice=session.Profiles?.Notice;
            if(!string.IsNullOrEmpty(notice))Label(parent,notice,x,y,width,42,17,new Color(1,0.65f,0.3f));
        }
        public void Pilots()
        {
            if(session.Profiles==null){MainMenu();return;}
            var card=Modal("PILOTOS / SOLO EN ESTE EQUIPO","REGISTRO DE PILOTO",1060,860);
            Label(card,"Apodo (2–24 caracteres). No se necesita una cuenta online.",36,135,980,40,22,muted);
            var inputRoot=Box("Pilot nickname",card,36,194,640,62,panel);
            inputRoot.GetComponent<Image>().raycastTarget=true;
            var field=inputRoot.gameObject.AddComponent<TMP_InputField>();
            var viewport=Rect("Viewport",inputRoot,16,6,608,50);viewport.gameObject.AddComponent<RectMask2D>();
            var inputText=Label(viewport,"",0,0,608,50,25);inputText.alignment=TextAlignmentOptions.MidlineLeft;inputText.textWrappingMode=TextWrappingModes.NoWrap;
            var placeholder=Label(viewport,"Escribe tu apodo",0,0,608,50,23,muted);placeholder.alignment=TextAlignmentOptions.MidlineLeft;
            field.textViewport=viewport;field.textComponent=(TextMeshProUGUI)inputText;field.placeholder=placeholder;field.characterLimit=24;field.lineType=TMP_InputField.LineType.SingleLine;
            notification=Label(card,"",36,268,980,40,20,new Color(1,0.65f,0.3f));
            void Register(){try{session.Profiles.Register(field.text);Sectors();}catch(ArgumentException e){notification.text=e.Message;}}
            Button(card,"REGISTRAR",700,194,324,Register,true,true);
            field.onSubmit.AddListener(_=>Register());
            Label(card,"PILOTOS REGISTRADOS",36,326,980,34,18,cyan);
            var pilots=session.Profiles.Data.pilots;
            int pageCount=Mathf.Max(1,(pilots.Count+4)/5);pilotPage=Mathf.Clamp(pilotPage,0,pageCount-1);
            for(int i=0;i<5;i++)
            {
                int index=pilotPage*5+i;if(index>=pilots.Count)break;var p=pilots[index];
                Button(card,$"{p.nickname}     /     {p.bestScore:000000} pts     /     SECTOR {p.maxSector}",36,370+i*68,988,()=>{session.Profiles.Select(p.id);Sectors();});
            }
            if(pilots.Count==0)Label(card,"Tu primer vuelo comienza aquí. Registra un apodo para jugar.",36,397,970,85,26,muted);
            Button(card,"VOLVER",36,760,270,MainMenu);
            if(pageCount>1){Button(card,"ANTERIOR",330,760,230,()=>{pilotPage=(pilotPage+pageCount-1)%pageCount;Pilots();});Button(card,"SIGUIENTE",584,760,240,()=>{pilotPage=(pilotPage+1)%pageCount;Pilots();});}
            Notice(card,36,716,980);FocusFirst();
        }
        public void Sectors()
        {
            if(session.Profiles?.Selected==null){Pilots();return;}
            var card=Modal("CAMPAÑA / "+session.Profiles.Selected.nickname,"SELECCIONA EL SECTOR",1300,780);
            Label(card,"Elige tu misión.",36,142,1220,38,22,muted);
            for(int i=0;i<session.Levels.Length;i++)
            {
                int sector=i;var data=session.Levels[i];bool unlocked=i<session.UnlockedSector;
                var slot=Box("Sector card",card,36+i*418,220,392,386,panel);
                Label(slot,$"0{i+1} / "+(unlocked?"DISPONIBLE":"BLOQUEADO"),24,25,344,35,18,unlocked?cyan:muted);
                Label(slot,data.displayName,24,85,344,100,35);
                Label(slot,data.Objective,24,196,344,90,23,muted);
                Button(slot,unlocked?"VER MISIÓN":"COMPLETA EL ANTERIOR",24,303,344,()=>Briefing(sector),unlocked,true);
            }
            Label(card,"Los fragmentos también cuentan.",36,645,1220,50,22,muted);
            Button(card,"MENÚ PRINCIPAL",36,700,360,MainMenu);Notice(card,430,705,800);FocusFirst();
        }
        public void Ranking()
        {
            var card=Modal("RANKING LOCAL / MEJOR PARTIDA POR PILOTO","SALÓN DE LA FAMA",1420,880);
            Label(card,"POS.     PILOTO",36,158,560,36,20,cyan);
            Label(card,"PUNTOS       SECTOR        SUPERVIVENCIA",660,158,720,36,20,cyan);
            var records=session.Profiles?.GetTop(10);
            if(records==null||records.Count==0)Label(card,"Aún no hay partidas registradas. La próxima marca puede ser tuya.",36,272,1310,110,33,muted);
            else for(int i=0;i<records.Count;i++)
            {
                var r=records[i];float y=212+i*49;
                if(i%2==0)Box("Ranking row",card,25,y-3,1370,46,panel);
                Label(card,$"{i+1:00}       {r.nickname}",36,y,590,42,23,i==0?cyan:text);
                Label(card,$"{r.score:000000}           {r.sector}              {TimeLabel(r.survivalSeconds)}"+(r.victory?"   VICTORIA":""),660,y,720,42,23);
            }
            Label(card,"Esta clasificación pertenece a esta computadora. No se comparte por Internet.",36,745,1320,38,21,muted);
            Button(card,"VOLVER",36,795,340,MainMenu);Notice(card,420,805,930);FocusFirst();
        }
        private Slider Volume(Transform parent,string name,float y,float value)
        {
            Label(parent,name,36,y,330,40,23);
            var root=Rect(name,parent,400,y+4,470,34);var slider=root.gameObject.AddComponent<Slider>();
            var sliderHit=root.gameObject.AddComponent<Image>();sliderHit.color=Color.clear;sliderHit.raycastTarget=true;
            Box("Track",root,0,13,470,8,panel);
            var fillArea=Rect("Fill Area",root,0,0,470,34);
            var fill=Box("Fill",fillArea,0,13,470,8,cyan);
            fill.anchorMin=new Vector2(0,0.5f);fill.anchorMax=new Vector2(1,0.5f);fill.pivot=new Vector2(0.5f,0.5f);fill.anchoredPosition=Vector2.zero;fill.sizeDelta=new Vector2(0,8);
            slider.fillRect=fill;slider.direction=Slider.Direction.LeftToRight;
            var handleArea=Rect("Handle Area",root,0,0,470,34);
            var handle=Box("Handle",handleArea,0,4,20,26,text);handle.anchorMin=handle.anchorMax=new Vector2(0,0.5f);handle.pivot=new Vector2(0.5f,0.5f);handle.anchoredPosition=Vector2.zero;
            handle.GetComponent<Image>().raycastTarget=true;
            slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.value=value;
            var amount=Label(parent,Mathf.RoundToInt(value*100)+"%",900,y,110,40,23,cyan);
            slider.onValueChanged.AddListener(v=>amount.text=Mathf.RoundToInt(v*100)+"%");return slider;
        }
        public void Settings(bool fromPause)
        {
            settingsFromPause=fromPause;
            var card=Modal("PREFERENCIAS / GUARDADO LOCAL","CONFIGURACIÓN",1100,960);
            var s=session.Settings;
            var music=Volume(card,"MÚSICA",160,s.Music);var effects=Volume(card,"EFECTOS",230,s.Effects);
            var sensitivity=Volume(card,"VELOCIDAD DE GIRO",300,(s.Turn-0.6f)/0.8f);
            bool full=Screen.fullScreen;bool shake=s.CameraShake;int quality=QualitySettings.GetQualityLevel();
            int resolutionIndex=resolutions.IndexOf(new Vector2Int(Screen.width,Screen.height));if(resolutionIndex<0)resolutionIndex=Mathf.Max(0,resolutions.Count-2);
            Button shakeButton=null,qualityButton=null;
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            Button fullscreenButton=null,resolutionButton=null;
#endif
            void SetButtonText(Button b,string value)=>b.GetComponentInChildren<TMP_Text>().text=value;
#if UNITY_WEBGL && !UNITY_EDITOR
            Label(card,"Usa PANTALLA COMPLETA arriba del juego.\nLa imagen se adapta al tamaño de tu navegador.",36,395,1028,130,25,muted);
#else
            fullscreenButton=Button(card,"PANTALLA: "+(full?"COMPLETA":"VENTANA"),36,395,1028,()=>{full=!full;SetButtonText(fullscreenButton,"PANTALLA: "+(full?"COMPLETA":"VENTANA"));});
            resolutionButton=Button(card,$"RESOLUCIÓN: {resolutions[resolutionIndex].x} × {resolutions[resolutionIndex].y}",36,470,1028,()=>{resolutionIndex=(resolutionIndex+1)%resolutions.Count;SetButtonText(resolutionButton,$"RESOLUCIÓN: {resolutions[resolutionIndex].x} × {resolutions[resolutionIndex].y}");});
#endif
            qualityButton=Button(card,"CALIDAD: "+QualitySettings.names[quality],36,545,1028,()=>{quality=(quality+1)%QualitySettings.names.Length;SetButtonText(qualityButton,"CALIDAD: "+QualitySettings.names[quality]);});
            shakeButton=Button(card,"VIBRACIÓN DE CÁMARA: "+(shake?"ACTIVA":"DESACTIVADA"),36,620,1028,()=>{shake=!shake;SetButtonText(shakeButton,"VIBRACIÓN DE CÁMARA: "+(shake?"ACTIVA":"DESACTIVADA"));});
            Label(card,"Pulsa las opciones para alternarlas. Los cambios se aplican al guardar.",36,716,1010,65,22,muted);
            Button(card,"GUARDAR Y VOLVER",36,825,630,()=>{
                var r=resolutions[resolutionIndex];s.Apply(music.value,effects.value,0.6f+sensitivity.value*0.8f,shake,full,r.x,r.y,quality);session.ApplySettings();
                if(settingsFromPause)PauseMenu();else MainMenu();
            },true,true);
            Button(card,"CANCELAR",700,825,364,()=>{if(settingsFromPause)PauseMenu();else MainMenu();});FocusFirst();
        }
        private void PauseMenu()
        {
            var card=Modal("VUELO EN SUSPENSIÓN","PAUSA",860,780);
            Button(card,"CONTINUAR",36,247,788,()=>session.Pause(false),true,true);
            Button(card,"AYUDA",36,327,788,()=>ShowHelp(PauseMenu));
            Button(card,"REINICIAR SECTOR",36,407,788,session.Restart);
            Button(card,"CONFIGURACIÓN",36,487,788,()=>Settings(true));
            Button(card,"MENÚ PRINCIPAL",36,567,788,session.ReturnToMenu);
            Label(card,"ESC para continuar",36,690,788,40,22,muted);FocusFirst();
        }
        private void Results()
        {
            bool complete=session.State==SessionState.SectorComplete;bool victory=session.State==SessionState.Victory;
            var card=Modal(victory?"LOS TRES SECTORES ESTÁN A SALVO":complete?"OBJETIVO COMPLETADO":"SIN VIDAS DISPONIBLES",victory?"VICTORIA GALÁCTICA":complete?"SECTOR ASEGURADO":"MISIÓN TERMINADA",980,790);
            Label(card,$"{session.Stats.score:000000}",36,182,900,105,82,cyan);
            Label(card,"PUNTUACIÓN TOTAL",40,292,860,38,18,muted);
            Label(card,$"SECTOR {session.Stats.maxSector}     /     {TimeLabel(session.Stats.seconds)} DE VUELO\n{session.Stats.asteroids} ASTEROIDES     /     {session.Stats.enemies} ENEMIGOS",36,368,900,95,26);
            if(complete)Button(card,"SIGUIENTE SECTOR",36,528,908,session.NextSector,true,true);
            else Button(card,"VOLVER A INTENTAR",36,528,908,()=>session.StartRun(0),true,true);
            Button(card,"MENÚ PRINCIPAL",36,611,908,session.ReturnToMenu);Notice(card,36,710,900);FocusFirst();
        }
        private static string TimeLabel(float seconds) => $"{(int)seconds/60:00}:{(int)seconds%60:00}";
    }
}
