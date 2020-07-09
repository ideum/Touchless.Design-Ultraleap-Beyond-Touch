using DG.Tweening;
using Ideum.Data;
using UnityEngine;

namespace Ideum {
  public class App : MonoBehaviour {

    public Cursor Cursor;
    public CanvasGroup WarningBackground;

    private bool _connected;
    private float _queryInterval = 0.25f;
    private float _timer;

    private Sequence _seq;
    bool _touchWarningActive = false;

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

      if (!noTouch && _touchWarningActive) {
        _touchWarningActive = false;
        _seq?.Kill();
        _seq = DOTween.Sequence();
        _seq.Append(WarningBackground.DOFade(0.0f, 0.5f));
      } else if (noTouch && !_touchWarningActive) {
        Debug.Log("NO TOUCH: " + noTouch);
        _touchWarningActive = true;
        _seq?.Kill();
        _seq = DOTween.Sequence();

        _seq.Append(WarningBackground.DOFade(1.0f, 0.5f));
      }
    }

    private void HandleQueryResponse(bool clickState, HoverStates hoverState) {
      Cursor.DoStateChange(hoverState, clickState);
    }

  }
}