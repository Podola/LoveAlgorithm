using UnityEngine;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

/// <summary>
/// Add this to your Standard UI Response Button(s). In your dialogue entries, you can:
/// 1. Set the Description field to use it as tooltip text.
/// 2. Add a custom field named 'Activate On Hover' with the name of a GameObject
///    to activate when hovering over the response.
/// Note: You could implement IPointerMove and reposition the text over the button if you prefer.
/// </summary>
[RequireComponent(typeof(StandardUIResponseButton))]
public class ActivateOnResponseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [Tooltip("A Text element in which to display the dialogue entry's Description text.")]
    public UnityEngine.UI.Text tooltip;

    private GameObject activateOnHover;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var response = GetComponent<StandardUIResponseButton>().response;
        if (tooltip != null) tooltip.text = Field.LookupValue(response.destinationEntry.fields, "Description");
        var gameObjectName = Field.LookupValue(response.destinationEntry.fields, "Activate On Hover");
        activateOnHover = string.IsNullOrEmpty(gameObjectName) ? null : Tools.GameObjectHardFind(gameObjectName);
        if (activateOnHover != null) activateOnHover.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Hide();
    }

    private void Hide()
    {
        if (tooltip != null) tooltip.text = string.Empty;
        if (activateOnHover != null) activateOnHover.SetActive(false);
    }

}
