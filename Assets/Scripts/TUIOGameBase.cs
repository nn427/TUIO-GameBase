using TUIOsharp.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public interface iTUIOCursorEvent {
    void OnCursorAdd(TuioCursor entity, float x, float y);
    void OnCursorUpdate(TuioCursor entity, float x, float y);
    void OnCursorRemove(TuioCursor entity, float x, float y);
}

[RequireComponent(typeof(GraphicRaycaster))]
public class TUIOGameBase : MonoBehaviour, iTUIOCursorEvent {

    protected GraphicRaycaster graphicRaycaster;
    protected EventSystem eventSystem;
    protected TUIOHandler tuioHandler;

    protected virtual void Start() {
        UnityEngine.Input.multiTouchEnabled = true;
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        graphicRaycaster = GetComponent<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
        tuioHandler = FindObjectOfType<TUIOHandler>();

        if (tuioHandler != null) {
            tuioHandler.OnCursorAdded += OnCursorAdd;
            tuioHandler.OnCursorUpdated += OnCursorUpdate;
            tuioHandler.OnCursorRemoved += OnCursorRemove;
        }
    }

    public void OnCursorAdd(TuioCursor entity, float x, float y) {
        onCursorAdd(entity, x, y);
    }

    public void OnCursorUpdate(TuioCursor entity, float x, float y) {
        onCursorUpdate(entity, x, y);
    }

    public void OnCursorRemove(TuioCursor entity, float x, float y) {
        onCursorRemove(entity, x, y);
    }
    
    protected virtual void onCursorAdd(TuioCursor entity, float x, float y) { 
    }

    protected virtual void onCursorUpdate(TuioCursor entity, float x, float y) {
    }

    protected virtual void onCursorRemove(TuioCursor entity, float x, float y) {
    }

}
