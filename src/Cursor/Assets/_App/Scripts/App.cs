using Ideum.Data;
using UnityEngine;

namespace Ideum {
  public class App : MonoBehaviour {

    public Cursor Cursor;

    private bool _connected;
    private float _queryInterval = 0.25f;
    private float _timer;

    void Start() {
      TouchlessDesign.Initialize(AppSettings.Get().DataDirectory.GetPath());
      TouchlessDesign.Connected += OnConnected;
      TouchlessDesign.Disconnected += OnDisconnected;
    }

    void OnApplicationQuit() {
      TouchlessDesign.DeInitialize();
    }

    private void OnDisconnected() {
      Log.Info("Disconnected. Suspending queries");
      Cursor.DoStateChange(HoverStates.None, false);
      _connected = false;
    }

    private void OnConnected() {
      Log.Info("Connected. Starting to query...");
      _connected = true;
    }

    private void Update() {
      if (_connected) {
        _timer += Time.deltaTime;
        if(_timer > _queryInterval) {
          TouchlessDesign.QueryClickAndHoverState(HandleQueryResponse);
          TouchlessDesign.QueryNoTouchState(HandleNoTouch);
          _timer = 0f;
        }
      }
    }

    private void HandleNoTouch(bool noTouch) {
      if (noTouch) {
        Cursor.ShowNoTouch();
      } 
    }

    private void HandleQueryResponse(bool clickState, HoverStates hoverState) {
      Cursor.DoStateChange(hoverState, clickState);
    }

  }
}