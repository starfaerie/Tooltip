using UnityEngine;
using UnityEngine.EventSystems;

namespace Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string header;
        [SerializeField] [TextArea] private string content;
        [SerializeField] private int headerSize = 32;
        [SerializeField] private int contentSize = 30;
        [SerializeField] private Color panelColor, headerColor, contentColor, borderColor;
        [SerializeField] private int borderWidth = 3;
        [SerializeField] private int borderRadius = 10;
        [SerializeField] private int contentFont, headerFont;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField] private bool fadeIn = true, fadeOut;
        [SerializeField] private float popupDelay = 1f;
        //Unity Editor Properties
        [SerializeField] private bool hideInspector = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipSystem.SetColors(panelColor,borderColor,contentColor,headerColor);
            TooltipSystem.DrawGUI(borderWidth,borderRadius,content,contentSize,header,headerSize,popupDelay,contentFont,headerFont);
            TooltipSystem.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipSystem.Hide();
        }
    }
}