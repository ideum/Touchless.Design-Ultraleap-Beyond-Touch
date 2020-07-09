using System;
using Ideum;
using Ideum.Data;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class App : MonoBehaviour {

  private bool _connected;
  private float _queryInterval = 0.25f;
  private float _timer;

  public UIController UIController;
  public RectTransform Scalar;

  void Awake() {
    int screen = 0;
    try {
      var path = Path.Combine(Application.streamingAssetsPath, "settings.json");
      if (File.Exists(path)) {
        var json = File.ReadAllText(path);
        var obj = JObject.Parse(json);
        if (int.TryParse(obj["ScreenNumber"].ToString(), out screen)) {
          Log.Info("Screen set to " + screen);
        }
      }
    }
    catch (Exception e) {
      Log.Error(e);
    }

#if !UNITY_EDITOR
        if(Display.displays.Length > 1)
        {
            PlayerPrefs.SetInt("UnitySelectMonitor", screen);

            var display = Display.displays[screen];
            int w = display.systemWidth;
            int h = display.systemHeight;
            Screen.SetResolution(w, h, true);
        }
        else
        {
            Application.Quit();
        } 
#endif
    TouchlessDesign.Initialize(AppSettings.Get().DataDirectory.GetPath());
    TouchlessDesign.Connected += OnConnected;
    TouchlessDesign.Disconnected += OnDisconnected;
  }

  void OnApplicationQuit() {
    TouchlessDesign.DeInitialize();
  }

  private void OnDisconnected() {
    Log.Info("Disconnected. Suspending queries");
    _connected = false;
  }

  private void OnConnected() {
    Log.Info("Connected. Starting to query...");
    _connected = true;
    TouchlessDesign.QueryAddOn(HandleAddOnQuery);
  }

  private void HandleAddOnQuery(bool hasScreen, bool hasLEDs, int width_px, int height_px, int width_mm, int height_mm) {
    Log.Info($"{hasScreen}, {hasLEDs}, {width_px}, {height_px}, {width_mm}, {height_mm}");
    if (!hasScreen) return;
    float scaledX = Scalar.localScale.x * (width_mm / height_mm) / (width_px / height_px);
    Scalar.localScale = new Vector2(scaledX, Scalar.localScale.y);
  }

  private void Update() {
    if (_connected) {
      _timer += Time.deltaTime;
      if (_timer > _queryInterval) {
        TouchlessDesign.QueryClickAndHoverState(HandleQueryResponse);
        TouchlessDesign.QueryNoTouchState(HandleNoTouchState);
        _timer = 0f;
      }
    }
  }

  private void HandleNoTouchState(bool noTouch) {
    UIController.NoTouchWarning(noTouch);
  }

  private void HandleQueryResponse(bool clickState, HoverStates hoverState) {
    UIController.DoStateChange(hoverState, clickState);
  }
}
