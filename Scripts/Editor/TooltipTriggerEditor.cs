using UnityEditor;
using UnityEngine;

namespace Tooltip.Editor
{
    [CustomEditor(typeof(TooltipTrigger))]
    public class TooltipTriggerEditor : UnityEditor.Editor
    {
        #region Serialized Properties

        private SerializedProperty propHeader;
        private SerializedProperty propContent;
        private SerializedProperty propHeaderSize;
        private SerializedProperty propContentSize;
        private SerializedProperty propPanelColor;
        private SerializedProperty propHeaderColor;
        private SerializedProperty propContentColor;
        private SerializedProperty propBorderColor;
        private SerializedProperty propBorderWidth;
        private SerializedProperty propBorderRadius;
        private SerializedProperty propHeaderFont;
        private SerializedProperty propContentFont;
        private SerializedProperty propFadeCurve;
        private SerializedProperty propFadeIn;
        private SerializedProperty propFadeOut;
        private SerializedProperty propPopupDelay;

        private SerializedProperty hideInspector;

        #endregion

        #region Editor Data
        
        //For Editor Serialization
        private const string PROP_KEY = "TOOLTIP_TRIGGER";

        //Editor only bools
        private bool _displayProperties,
            _displayCanvas,
            _displayText,
            _displayAnimation,
            _fadeInPreview,
            _fadeOutPreview,
            _fadeBothPreview;
        
        //For Text Size
        private const int MIN_SIZE = 4;
        private const int MAX_SIZE = 48;
        
        //For fade animations
        private float _fadeInTimer, _fadeOutTimer;

        #endregion

        private void OnEnable()
        {
            LoadSerializedEditorProperties();

            InitSerializedProperties();
            
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SaveSerializedEditorProperties();
            
            //Unsubscribing in Editor scripts causes A LOT of problems
            //EditorApplication.update -= OnEditorUpdate;
        }

        public override void OnInspectorGUI()
        {
            //necessary to call before changing any serialized values in a custom inspector
            serializedObject.Update();

            GUI.color = GetGUIColor();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Header:", EditorStyles.boldLabel);
            propHeader.stringValue = EditorGUILayout.TextField(propHeader.stringValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propContent, GUILayout.MinHeight(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string displayString = _displayProperties ? "Show Preview Only" : "Edit Properties";

            if (GUILayout.Button(displayString))
            {
                _displayProperties = !_displayProperties;
            }
            
            if (_displayProperties)
            {
                //Canvas Properties
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _displayCanvas = EditorGUILayout.BeginToggleGroup("Canvas",_displayCanvas);
                if (_displayCanvas)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    //Color Property Fields
                    propPanelColor.colorValue =
                        EditorGUILayout.ColorField("Background Color", propPanelColor.colorValue);
                    EditorGUILayout.PropertyField(propBorderColor);

                    //Border settings
                    propBorderWidth.intValue = EditorGUILayout.IntSlider("Border Width",propBorderWidth.intValue, 0, 6);
                    propBorderRadius.intValue = EditorGUILayout.IntSlider("Border Radius",propBorderRadius.intValue, 0, 30);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();

                //Text Properties
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _displayText = EditorGUILayout.BeginToggleGroup("Text",_displayText);
                if (_displayText)
                {
                    //Header Properties
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("Header:", HeaderLabel());
                    propHeaderSize.intValue = EditorGUILayout.IntSlider(propHeaderSize.intValue,MIN_SIZE,MAX_SIZE);
                    propHeaderColor.colorValue = EditorGUILayout.ColorField("Color", propHeaderColor.colorValue);
                    propHeaderFont.intValue = EditorGUILayout.Popup("Font:", propHeaderFont.intValue, Tooltip.FONTS);

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(5);

                    //Content Properties
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("Content:", HeaderLabel());
                    propContentSize.intValue = EditorGUILayout.IntSlider(propContentSize.intValue,MIN_SIZE,MAX_SIZE);
                    propContentColor.colorValue = EditorGUILayout.ColorField("Color", propContentColor.colorValue);
                    propContentFont.intValue = EditorGUILayout.Popup("Font:", propContentFont.intValue, Tooltip.FONTS);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
                
                //Animation Properties
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _displayAnimation = EditorGUILayout.BeginToggleGroup("Animations",_displayAnimation);
                if (_displayAnimation)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    propPopupDelay.floatValue =
                        EditorGUILayout.Slider(new GUIContent("Popup Delay","Time without moving the mouse before this tooltip will appear (in seconds)"), propPopupDelay.floatValue, 0f, 4f);
                    propFadeIn.boolValue = EditorGUILayout.Toggle("Fade In",propFadeIn.boolValue);
                    propFadeOut.boolValue = EditorGUILayout.Toggle("Fade Out",propFadeOut.boolValue);
                    if (propFadeIn.boolValue || propFadeOut.boolValue)
                    {
                        GUILayout.Label(new GUIContent("Alpha Fade Animation:", "A 1 second animation curve.\nIf 'Fade Out' is enabled, this animation will play in reverse when the tooltip disappears."));
                        propFadeCurve.animationCurveValue = EditorGUILayout.CurveField(propFadeCurve.animationCurveValue, Color.green, new Rect(0,0,1,1), GUILayout.MinHeight(40));    
                        EditorGUILayout.Space(5);
                    }
                    EditorGUILayout.EndVertical();

                    if (propFadeIn.boolValue || propFadeOut.boolValue)
                    {
                        GUILayout.BeginHorizontal();
                        const string fadeInString = "Preview Fade Animation";
                        if (GUILayout.Button(fadeInString))
                        {
                            _fadeInTimer = 0f;
                            _fadeOutTimer = 0f;
                            if (propFadeIn.boolValue && propFadeOut.boolValue)
                            {
                                _fadeBothPreview = true;
                                _fadeInPreview = true;
                                _fadeOutPreview = true;
                            }
                            else if (propFadeIn.boolValue && !propFadeOut.boolValue)
                            {
                                _fadeBothPreview = false;
                                _fadeInPreview = true;
                                _fadeOutPreview = false;
                            }
                            else if (propFadeOut.boolValue && !propFadeIn.boolValue)
                            {
                                _fadeBothPreview = false;
                                _fadeOutPreview = true;
                                _fadeInPreview = false;
                            }
                            else
                            {
                                _fadeBothPreview = false;
                                _fadeInPreview = true;
                                _fadeOutPreview = false;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }

            //PreviewGUI Properties
            const int margin = 20;
            float width = GUILayoutUtility.GetLastRect().width;
            float height = !string.IsNullOrEmpty(propHeader.stringValue)
                ? propHeaderSize.intValue + propContentSize.intValue + margin * 2
                : propContentSize.intValue + margin;
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical();
            
            OnPreviewGUI(GUILayoutUtility.GetRect(width, height), EditorStyles.whiteLabel);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.EndVertical();

            //ensures that the changed values are registered in Unity's Undo system
            //necessary to call after changing any serialized values in a custom inspector
            if (serializedObject.ApplyModifiedProperties())
                Repaint();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            //reset GUI.color to default for this OnPreviewGUI block
            GUI.color = Color.white;

            bool emptyHeader = !string.IsNullOrEmpty(propHeader.stringValue);
            
            //Text margin (in pixels)
            const int margin = 10;
            
            //Border settings
            int borderWidth = propBorderWidth.intValue;
            int borderRadius = propBorderRadius.intValue;
            Color borderColor =
                AnimateAlphaColor(propBorderColor.colorValue, propFadeCurve.animationCurveValue, _fadeInPreview,_fadeOutPreview);
            
            //Header Label settings
            Color headerColor =
                AnimateAlphaColor(propHeaderColor.colorValue, propFadeCurve.animationCurveValue, _fadeInPreview,_fadeOutPreview);
            GUIStyle headerStyle = HeaderStyle(headerColor);
            Rect headerRect = emptyHeader
                ? new Rect(r.x + margin * 2, r.y + margin, r.width - margin, r.height - margin)
                : Rect.zero;
            GUIContent headerText = emptyHeader
                ? new GUIContent("Header")
                : GUIContent.none;

            //Content Label settings
            Color contentColor =
                AnimateAlphaColor(propContentColor.colorValue, propFadeCurve.animationCurveValue, _fadeInPreview,_fadeOutPreview);
            GUIStyle contentStyle = ContentStyle(contentColor);
            Rect contentRect = emptyHeader
                ? new Rect(r.x + margin * 4,
                    r.y + headerStyle.CalcHeight(headerText, r.width - margin) + margin * 2, r.width - margin,
                    r.height - margin) : new Rect(r.x + margin * 2,
                    r.y + margin, r.width - margin,
                    r.height - margin);
            GUIContent contentText = new GUIContent("Content");

            //Background Texture settings
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, propPanelColor.colorValue);
            texture.Apply();
            
            //Color animation
            Color canvasColor =
                AnimateAlphaColor(propPanelColor.colorValue, propFadeCurve.animationCurveValue, _fadeInPreview,_fadeOutPreview);

            //Drawing everything
            GUI.DrawTexture(r, texture, ScaleMode.StretchToFill,true,0,canvasColor,r.width,borderRadius);
            if (borderWidth > 0)
            {
                //Border Texture settings
                Texture2D borderTexture = new Texture2D(1, 1);
                borderTexture.SetPixel(1, 1, borderColor);
                borderTexture.Apply();
                GUI.DrawTexture(r, borderTexture, ScaleMode.StretchToFill, true, 0, borderColor, borderWidth,
                    borderRadius);
            }
            GUI.Label(headerRect,headerText,headerStyle);
            GUI.Label(contentRect,contentText,contentStyle);

            GUI.color = GetGUIColor();
        }

        private void SavePreview(Rect r, GUIStyle background)
        {
            //reset GUI.color to default for this GUI block
            GUI.color = Color.white;

            //Border settings
            int borderWidth = propBorderWidth.intValue;
            int borderRadius = propBorderRadius.intValue;
            Color borderColor = propBorderColor.colorValue;
            
            //Background Texture settings
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, propPanelColor.colorValue);
            texture.Apply();
            
            //Drawing everything
            GUI.DrawTexture(r, texture, ScaleMode.StretchToFill,false,0,propPanelColor.colorValue,r.width,borderRadius);
            if (borderWidth > 0)
            {
                //Border Texture settings
                Texture2D borderTexture = new Texture2D(1, 1);
                borderTexture.SetPixel(1, 1, borderColor);
                borderTexture.Apply();
                GUI.DrawTexture(r, borderTexture, ScaleMode.StretchToFill, false, 0, borderColor, borderWidth,
                    borderRadius);
            }

            GUI.color = GetGUIColor();
        }

        private void OnEditorUpdate()
        {
            if (_fadeInPreview && !_fadeOutPreview && !_fadeBothPreview)
            {
                Repaint();
                if (_fadeInTimer < 2)
                {
                    _fadeInTimer += Time.deltaTime;
                }
                else
                {
                    _fadeInPreview = false;
                }
            }
            else if (_fadeOutPreview && !_fadeInPreview && !_fadeBothPreview)
            {
                Repaint();
                if (_fadeOutTimer < 2)
                {
                    _fadeOutTimer += Time.deltaTime;
                }
                else
                {
                    _fadeOutPreview = false;
                }
            }
            else if (_fadeBothPreview)
            {
                Repaint();
                if (_fadeInTimer < 2)
                {
                    _fadeInTimer += Time.deltaTime;
                }
                else
                {
                    _fadeInPreview = false;
                    if (_fadeOutTimer < 2)
                    {
                        _fadeOutTimer += Time.deltaTime;
                    }
                    else
                    {
                        _fadeOutPreview = false;
                        _fadeBothPreview = false;
                    }
                }
            }
        }

        private Color AnimateAlphaColor(Color color, AnimationCurve curve, bool fadeIn, bool fadeOut)
        {
            if (fadeIn)
            {
                return new Color(color.r, color.g,
                    color.b,
                    curve.Evaluate(-1 + _fadeInTimer) * color.a);
            }
            if (fadeOut)
            {
                return new Color(color.r, color.g,
                    color.b,
                    curve.Evaluate(2 - _fadeOutTimer) * color.a);
            }
            return color;
        }

        private GUIStyle HeaderStyle(Color color)
        {
            Font headerFont = LoadFont(Tooltip.FONTS[propHeaderFont.intValue]);

            return new GUIStyle
            {
                fontSize = propHeaderSize.intValue,
                fontStyle = FontStyle.Bold,
                normal = {textColor = color},
                font = headerFont,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
            };
        }

        private GUIStyle ContentStyle(Color color)
        {
            Font contentFont = LoadFont(Tooltip.FONTS[propContentFont.intValue]);
            return new GUIStyle
            {
                fontSize = propContentSize.intValue,
                normal = {textColor = color},
                font = contentFont,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
        }

        private static GUIStyle HeaderLabel()
        {
            return new GUIStyle
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = {textColor = Color.white}
            };
        }

        private static Font LoadFont(string font)
        {
            const string path = "Assets/Tooltip/Fonts/";
            const string ttf = ".ttf";
            return (Font)AssetDatabase.LoadAssetAtPath(path + font + ttf, typeof(Font));
            
            #region A Note on Custom Fonts

            /*
             An alternate way of getting fonts, if you want to load all of the fonts
             on someone's computer rather than only a predetermined list...
             try this:
             
             //Get all the fonts
             string[] fonts = Font.GetOSInstalledFontNames();
             //Assign them to a serialized intValue
             propContentFont.intValue = EditorGUILayout.Popup(EditorGUIUtility.TrTextContent("Font"), propContentFont.enumValueIndex, fonts);
             */

            #endregion
        }

        private static Color GetGUIColor(float t = 0.4f)
        {
            return Color.Lerp(Color.white, Color.cyan, t);
        }

        private void InitSerializedProperties()
        {
            propHeader = serializedObject.FindProperty("header");
            propContent = serializedObject.FindProperty("content");
            propHeaderSize = serializedObject.FindProperty("headerSize");
            propContentSize = serializedObject.FindProperty("contentSize");
            propPanelColor = serializedObject.FindProperty("panelColor");
            propHeaderColor = serializedObject.FindProperty("headerColor");
            propContentColor = serializedObject.FindProperty("contentColor");
            propBorderColor = serializedObject.FindProperty("borderColor");
            propBorderWidth = serializedObject.FindProperty("borderWidth");
            propBorderRadius = serializedObject.FindProperty("borderRadius");
            propContentFont = serializedObject.FindProperty("contentFont");
            propHeaderFont = serializedObject.FindProperty("headerFont");
            propFadeCurve = serializedObject.FindProperty("fadeCurve");
            propFadeIn = serializedObject.FindProperty("fadeIn");
            propFadeOut = serializedObject.FindProperty("fadeOut");
            propPopupDelay = serializedObject.FindProperty("popupDelay");

            hideInspector = serializedObject.FindProperty("hideInspector");
        }

        private void SaveSerializedEditorProperties()
        {
            EditorPrefs.SetBool(PROP_KEY + "displayProperties", _displayProperties);
            EditorPrefs.SetBool(PROP_KEY + "displayCanvas", _displayCanvas);
            EditorPrefs.SetBool(PROP_KEY + "displayText", _displayText);
            EditorPrefs.SetBool(PROP_KEY + "displayAnimation", _displayAnimation);
        }

        private void LoadSerializedEditorProperties()
        {
            _displayProperties = EditorPrefs.GetBool(PROP_KEY + "displayProperties", false);
            _displayCanvas = EditorPrefs.GetBool(PROP_KEY + "displayCanvas", true);
            _displayText = EditorPrefs.GetBool(PROP_KEY + "displayText", true);
            _displayAnimation = EditorPrefs.GetBool(PROP_KEY + "displayAnimation", true);
        }
    }
}