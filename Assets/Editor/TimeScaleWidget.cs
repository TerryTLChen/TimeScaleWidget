using UnityEngine;

namespace UnityEditor {
    
    [InitializeOnLoad]
    public class TimeScaleWidget {

        const string WIN_POS_X_KEY = "TimeScaleWindow_PosX";
        const string WIN_POS_Y_KEY = "TimeScaleWindow_PosY";
        const float MAX_SCALE = 12f;

        const string BUTTON_NAME_BY_FOUR = "/4";
        const string BUTTON_NAME_BY_TWO = "/2";
        const string BUTTON_NAME_TIMES_TWO = "x2";
        const string BUTTON_NAME_TIMES_FOUR = "x4";
        const string BUTTON_NAME_RESET = "Reset";
        const string LABEL_TIME_SCALE = "Time Scale";

        static bool resetPosition = false;
        static bool isDragging = false;
        static bool positionChanged = false;

        static Rect windowRect = Rect.zero;
        static readonly Rect startWindowRect = new Rect(10f, 40f, 200f, 50f);

        static readonly float horizontalSpace = 5f;
        static readonly float windowTitleHeight = 15f; // roughly
        static readonly GUIStyle contentTextStyle = new GUIStyle();
        static readonly GUIContent emptyContent = new GUIContent("");

        static float timeScale = 1f;
        static bool focused = false;

        static Color textColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        static Color coldColor = new Color(textColor.r, textColor.g, 1f, textColor.a);
        static Color hotColor = new Color(1f, textColor.g, textColor.b, textColor.a);
        static Color defaultBackgroundColor;

        static TimeScaleWidget() {

            contentTextStyle.alignment = TextAnchor.MiddleRight;
            contentTextStyle.normal.textColor = textColor;
            contentTextStyle.fontStyle = FontStyle.Bold;

            defaultBackgroundColor = GUI.backgroundColor;

            SceneView.onSceneGUIDelegate += OnSceneGUI;

            windowRect.size = startWindowRect.size;
            windowRect.position = storedWindowPosition;
        }

        static Vector2 storedWindowPosition {
            get {
                Vector2 position = startWindowRect.position;
                if (EditorPrefs.HasKey(WIN_POS_X_KEY))
                    position.x = EditorPrefs.GetFloat(WIN_POS_X_KEY);
                if (EditorPrefs.HasKey(WIN_POS_Y_KEY))
                    position.y = EditorPrefs.GetFloat(WIN_POS_Y_KEY);
                return position;
            }
            set {
                EditorPrefs.SetFloat(WIN_POS_X_KEY, value.x);
                EditorPrefs.SetFloat(WIN_POS_Y_KEY, value.y);
            }
        }

        static void DoWindow(int windowID) {

            timeScale = Time.timeScale;

            Event e = Event.current;

            GUI.backgroundColor = Color.green;
            GUI.enabled = focused;

            if (GUI.Button(new Rect(180f, 4f, 20f, 6f), "")) {
                resetPosition = true;
                GUI.FocusControl(null);
            }

            GUI.enabled = focused;
            GUI.backgroundColor = defaultBackgroundColor;

            GUILayout.BeginHorizontal();

            if (timeScale < 1) {
                contentTextStyle.normal.textColor = coldColor;
            } else if (timeScale > 1) {
                contentTextStyle.normal.textColor = hotColor;
            }

            GUI.SetNextControlName(LABEL_TIME_SCALE);
            timeScale = Mathf.Clamp(EditorGUILayout.FloatField(LABEL_TIME_SCALE, timeScale, contentTextStyle, GUILayout.MaxWidth(startWindowRect.size.x)), 0f, MAX_SCALE);
            
            contentTextStyle.normal.textColor = textColor;
            GUILayout.EndHorizontal();
            GUILayout.Space(horizontalSpace);

            GUI.enabled = focused;
            GUILayout.BeginHorizontal();
            GUI.enabled = timeScale > 0f && focused;
            GUI.SetNextControlName("decrease");
            if (GUILayout.Button(emptyContent, GUI.skin.horizontalScrollbarLeftButton)) {
                timeScale -= 0.01f;
                GUI.FocusControl("decrease");
            }
            GUI.enabled = focused;

            GUI.SetNextControlName("slider");
            EditorGUI.BeginChangeCheck();
            timeScale = GUILayout.HorizontalSlider(timeScale, 0f, MAX_SCALE, GUI.skin.horizontalScrollbar, GUI.skin.horizontalScrollbarThumb);
            if (EditorGUI.EndChangeCheck()) {
                if (timeScale > 1f)
                    timeScale = Mathf.RoundToInt(timeScale);
                else
                    timeScale = Mathf.RoundToInt(timeScale * 10f) / 10f;
            }

            GUI.enabled = timeScale < MAX_SCALE && focused;
            GUI.SetNextControlName("increase");
            if (GUILayout.Button(emptyContent, GUI.skin.horizontalScrollbarRightButton)) {
                timeScale += 0.01f;
                GUI.FocusControl("increase");
            }
            GUI.enabled = focused;

            GUILayout.EndHorizontal();
            GUILayout.Space(horizontalSpace);

            GUIStyle buttonStyle = GUI.skin.button;
            buttonStyle.fontSize = 9;

            GUILayout.BeginHorizontal();
            GUI.enabled = focused && (timeScale != 1f / 4f);
            GUI.backgroundColor = (GUI.enabled) ? coldColor : defaultBackgroundColor;
            GUI.SetNextControlName(BUTTON_NAME_BY_FOUR);
            if (GUILayout.Button(BUTTON_NAME_BY_FOUR, buttonStyle, GUILayout.MaxWidth(25f))) {
                timeScale = 1f / 4f;
                GUI.FocusControl(BUTTON_NAME_BY_FOUR);
            }
            GUI.enabled = focused && (timeScale != 1f / 2f);
            GUI.backgroundColor = (GUI.enabled) ? coldColor : defaultBackgroundColor;
            GUI.SetNextControlName(BUTTON_NAME_BY_TWO);
            if (GUILayout.Button(BUTTON_NAME_BY_TWO, buttonStyle, GUILayout.MaxWidth(25f))) {
                timeScale = 1f / 2f;
                GUI.FocusControl(BUTTON_NAME_BY_TWO);
            }

            if (timeScale == 1f) GUI.backgroundColor = defaultBackgroundColor;
            else if (timeScale < 1f) GUI.backgroundColor = coldColor;
            else if (timeScale > 1f) GUI.backgroundColor = hotColor;

            GUI.enabled = focused && (timeScale != 1f);

            GUI.SetNextControlName(BUTTON_NAME_RESET);
            if (GUILayout.Button(BUTTON_NAME_RESET, buttonStyle)) {
                timeScale = 1f;
                GUI.FocusControl(BUTTON_NAME_RESET);
            }

            GUI.enabled = focused && (timeScale != 1f * 2f);
            GUI.backgroundColor = (GUI.enabled) ? hotColor : defaultBackgroundColor;
            GUI.SetNextControlName(BUTTON_NAME_TIMES_TWO);
            if (GUILayout.Button(BUTTON_NAME_TIMES_TWO, buttonStyle, GUILayout.MaxWidth(25f))) {
                timeScale = 1f * 2f;
                GUI.FocusControl(BUTTON_NAME_TIMES_TWO);
            }
            GUI.enabled = focused && (timeScale != 1f * 4f);
            GUI.backgroundColor = (GUI.enabled) ? hotColor : defaultBackgroundColor;
            GUI.SetNextControlName(BUTTON_NAME_TIMES_FOUR);
            if (GUILayout.Button(BUTTON_NAME_TIMES_FOUR, buttonStyle, GUILayout.MaxWidth(25f))) {
                timeScale = 1f * 4f;
                GUI.FocusControl(BUTTON_NAME_TIMES_FOUR);
            }
            GUILayout.EndHorizontal();

            GUI.enabled = focused;
            GUI.backgroundColor = defaultBackgroundColor;

            if (GUI.changed) {
                Time.timeScale = Mathf.Min(Mathf.RoundToInt(timeScale * 100f) / 100f, MAX_SCALE);
            }
            
            // Make the window drag
            Vector2 dragSize = new Vector2(startWindowRect.size.x, windowTitleHeight);
            if (e.type == EventType.MouseDrag) {
                isDragging = true;
            }
            // Mouse is released when dragging either inside (mouse up event) the window area or outside (ignore event)
            if (isDragging && (e.type == EventType.MouseUp || e.type == EventType.Ignore)) {
                isDragging = false;
                positionChanged = true;
                e.Use();
            }
            GUI.DragWindow(new Rect(Vector2.zero, dragSize)); // this will eat the mouseDrag event

            // Preventing click through by use all the mouse event
            if (e.isMouse) {
                e.Use();
            }
        }

        static Vector2 GetDefaultPosition(SceneView sceneView) {
            return new Vector2(startWindowRect.position.x, sceneView.position.size.y - startWindowRect.position.y - startWindowRect.size.y);
        }

        static Vector2 ClampToViewport(Rect sceneViewRect, Vector2 position) {

            float x = Mathf.Clamp(position.x, 0f, sceneViewRect.size.x - startWindowRect.size.x - sceneViewRect.position.x);
            float y = Mathf.Clamp(position.y, sceneViewRect.position.y + windowTitleHeight, sceneViewRect.size.y - startWindowRect.size.y - windowTitleHeight);

            return new Vector2(x, y);
        }

        static void OnSceneGUI(SceneView sceneView) {

            focused = (EditorWindow.focusedWindow == sceneView);
            if (focused)
            {
                Rect nextWindowRect = GUILayout.Window(1001, windowRect, DoWindow, emptyContent);

                if (resetPosition)
                {
                    windowRect.position = GetDefaultPosition(sceneView);
                    resetPosition = false;
                    positionChanged = true;
                }
                else
                {
                    windowRect.position = ClampToViewport(sceneView.camera.pixelRect, nextWindowRect.position);
                }

                if (positionChanged)
                {
                    storedWindowPosition = windowRect.position;
                    positionChanged = false;
                }
            }
        }
    }
}
