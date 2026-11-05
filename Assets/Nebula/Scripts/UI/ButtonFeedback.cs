using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaDefender
{
    public sealed class ButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private AudioService audioService;
        private bool highlighted;
        public void Initialize(AudioService audio) => audioService = audio;
        public void OnSelect(BaseEventData data) { highlighted = true; if(audioService!=null)audioService.Cue(AudioCue.Select); }
        public void OnDeselect(BaseEventData data) => highlighted = false;
        public void OnPointerEnter(PointerEventData data) { highlighted = true; if(audioService!=null)audioService.Cue(AudioCue.Select); }
        public void OnPointerExit(PointerEventData data) => highlighted = EventSystem.current.currentSelectedGameObject == gameObject;
        private void Update() => transform.localScale = Vector3.Lerp(transform.localScale,Vector3.one*(highlighted?1.012f:1),Time.unscaledDeltaTime*15);
    }
}
