using UnityEditor;
using UnityEngine;

namespace Tooltip
{
    [ExecuteInEditMode]
    public class Tooltip : MonoBehaviour
    {
        #region Tooltip Fields

        [SerializeField] public Color color = Color.white;
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private int characterWrapLimit = 80; //change this to width
        [SerializeField] private bool hideInspector;
        private float popupDelay = 1f;
        
        //These names must match the name of the .ttf files in the Font folder EXACTLY
        public static readonly string[] FONTS =
        {
            "Arial",
            "Berlin Sans FB",
            "Calibri",
            "Cambria",
            "Century Gothic",
            "Segoe UI",
            "Times New Roman",
            "Trebuchet MS",
            "Verdana"
        };

        #endregion

        private RectTransform rectTransform;

        private int borderWidth, borderRadius, contentSize, headerSize;
        private string content, header;
        private Color headerTextColor, contentTextColor;
        private Font headerFont, contentFont;
        
        private const int margin = 10;
        private bool mouseStopped;
        private bool _fadeIn;
        private float _fadeInTimer;

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetColors(Color panelColor, Color borderColor, Color contentTextColor, Color headerTextColor)
        {
            //Main color
            color = panelColor;
            //tooltip.color = panelColor;
            
            //Outline color
            outlineColor = borderColor;

            //Text color
            this.contentTextColor = contentTextColor;
            this.headerTextColor = headerTextColor;
            //textMaterial.SetColor(_FaceColor,textColor);
        }

        public void DrawGUI(int borderWidth, int borderRadius, string content, int contentSize, float popupDelay, string header = "",int headerSize = 0)
        {
            this.borderWidth = borderWidth;
            this.borderRadius = borderRadius;
            
            this.contentSize = contentSize;
            this.headerSize = headerSize;

            this.content = content;
            this.header = header;

            this.popupDelay = popupDelay;
        }
        
        public void DrawGUI(int borderWidth, int borderRadius, string content, int contentSize, float popupDelay, string header,int headerSize, int contentFont = 0, int headerFont = 0)
        {
            DrawGUI(borderWidth,borderRadius,content,contentSize,popupDelay,header,headerSize);

            this.contentFont = LoadFont(contentFont);
            this.headerFont = LoadFont(headerFont);
        }

        private void OnGUI()
        {
            //If the user is not clicking
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) return;
            Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            //And the mouse has stopped moving
            mouseStopped = !(direction.magnitude < 0.1f);
            if (mouseStopped)
            {
                //Start the _fadeInTimer
                _fadeInTimer = 0f;
                _fadeIn = true;
            }

            //Once the _fadeInTimer ends
            if (_fadeIn) return;
            
            //Draw the tooltip GUI
            Vector2 previewSize = new Vector2(400, 100);
            Vector2 mousePosition = GetMousePosition();
            float pivotX = mousePosition.x + Screen.width / mousePosition.x - previewSize.x * 0.5f;
            float pivotY = Screen.height - mousePosition.y - previewSize.y * 0.5f;
            Rect previewRect = new Rect(new Vector2(pivotX, pivotY), previewSize);
            OnPreviewGUI(previewRect);
        } 
        
        private void OnPreviewGUI(Rect r)
        {
            //reset GUI.color to default for this OnPreviewGUI block
            GUI.color = Color.white;

            //Border settings
            Color borderColor = outlineColor;
            
            //Background Texture settings
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, color);
            texture.Apply();

            //Header Label settings
            GUIStyle headerStyle = HeaderStyle();
            Rect headerRect = new Rect(r.x + margin, r.y + margin, r.width - margin * 2, r.height - margin);
            GUIContent headerText = new GUIContent(header);
            
            GUIStyle contentStyle = ContentStyle();
            Rect contentRect = new Rect(r.x + margin,
                r.y + headerStyle.CalcHeight(headerText, r.width - margin) + margin * 2, r.width - margin * 2,
                r.height - margin);
            GUIContent contentText = new GUIContent(content);

            //resetting the height of the background rect based on the content size
            r.height = margin * 4 + headerStyle.CalcHeight(headerText, r.width) +
                       contentStyle.CalcHeight(contentText, r.width);

            //resetting the width of the background rect if the content size is smaller than the current width
            if (contentStyle.CalcSize(contentText).x + margin * 2 < r.width && headerStyle.CalcSize(headerText).x + margin * 2 < r.width)
            {
                r.width = Mathf.Max(contentStyle.CalcSize(contentText).x + margin * 2,
                    headerStyle.CalcSize(headerText).x + margin * 2);
            }
            
            //Drawing everything
            GUI.DrawTexture(r, texture, ScaleMode.StretchToFill,false,0,color,r.width,borderRadius);
            if (borderWidth > 0)
            {
                //Border Texture settings
                Texture2D borderTexture = new Texture2D(1, 1);
                borderTexture.SetPixel(1, 1, borderColor);
                borderTexture.Apply();
                GUI.DrawTexture(r, borderTexture, ScaleMode.StretchToFill, false, 0, borderColor, borderWidth,
                    borderRadius);
            }
            GUI.Label(headerRect,headerText,headerStyle);
            GUI.Label(contentRect,contentText,contentStyle);
        }
        
        private GUIStyle ContentStyle()
        {
            return new GUIStyle
            {
                fontSize = contentSize,
                normal = {textColor = contentTextColor},
                font = contentFont,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
        }

        private GUIStyle HeaderStyle()
        {
            return new GUIStyle
            {
                fontSize = headerSize,
                normal = {textColor = headerTextColor},
                fontStyle = FontStyle.Bold,
                font = headerFont,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
            };
        }

        private void HideInspector()
        {
            if (hideInspector)
            {
                foreach (Component component in GetComponents<Component>())
                {
                    component.hideFlags = HideFlags.HideInInspector;
                }

                GetComponent<Tooltip>().hideFlags = HideFlags.None;
            }
            else
            {
                foreach (Component component in GetComponents<Component>())
                {
                    component.hideFlags = HideFlags.None;
                }
            }
        }

        private void Update()
        {
            if (Application.isEditor)
            {
                HideInspector();
            }
            
            rectTransform.position = GetMousePosition();
            Vector2 position = rectTransform.position;
            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;
            
            rectTransform.pivot = new Vector2(pivotX,pivotY);

            if (_fadeIn)
            {
                WaitForSeconds(popupDelay);
            }
        }

        private static Vector2 GetMousePosition()
        {
            //Unity Built in Input System
            return Input.mousePosition;
            
            /*
            //New Unity UIInput System
            using (InputSystemUIInputModule inputModule)
            {
                return inputModule.point.action.ReadValue<Vector2>();
            }
            */
        }

        private void WaitForSeconds(float seconds)
        {
            if (!_fadeIn) return;
            if (_fadeInTimer < seconds)
            {
                _fadeInTimer += Time.deltaTime;
            }
            else
            {
                _fadeIn = false;
            }
        }
        
        private static Font LoadFont(int font)
        {
            const string path = "Assets/Tooltip/Fonts/";
            const string ttf = ".ttf";
            return (Font)AssetDatabase.LoadAssetAtPath(path + font + ttf, typeof(Font));
        }
    }
}