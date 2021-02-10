using UnityEngine;

namespace Tooltip
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem current;
        public Tooltip tooltip;

        public void Awake()
        {
            current = this;
        }

        public static void Show()
        {
            current.tooltip.gameObject.SetActive(true);
        }

        public static void SetColors(Color panelColor, Color borderColor, Color contentTextColor, Color headerTextColor)
        {
            current.tooltip.SetColors(panelColor, borderColor, contentTextColor, headerTextColor);
        }

        public static void DrawGUI(int width, int radius,string content, int contentSize, float popupDelay, string header = "", int headerSize = 0)
        {
            current.tooltip.DrawGUI(width,radius,content,contentSize,popupDelay,header,headerSize);
        }
        
        public static void DrawGUI(int width, int radius,string content, int contentSize, string header, int headerSize, float popupDelay, int contentFont = 0, int headerFont = 0)
        {
            if (contentFont == 0 && headerFont == 0)
                DrawGUI(width, radius, content, contentSize, popupDelay, header, headerSize);
            else
                current.tooltip.DrawGUI(width, radius, content, contentSize,popupDelay, header, headerSize, contentFont,
                    headerFont);
        }

        public static void Hide()
        {
            current.tooltip.gameObject.SetActive(false);
        }
    }
}
