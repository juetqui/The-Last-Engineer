using UnityEngine;
using UnityEngine.EventSystems;

public class DirectionalButtonListener : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    private static int lastSelectedIndex = -1;

    public void OnSelect(BaseEventData eventData)
    {
        CalculateDir();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        CalculateDir();
    }

    private void PlaySoundUp()
    {
        UIButtonsManager.Instance.PlaySoundUp();
    }

    private void PlaySoundDown()
    {
        UIButtonsManager.Instance.PlaySoundDown();
    }
    
    private void CalculateDir()
    {
        int index = transform.GetSiblingIndex();

        if (lastSelectedIndex == -1 || lastSelectedIndex == index)
        {
            lastSelectedIndex = index;
            return; 
        }

        if (index > lastSelectedIndex)
            PlaySoundDown();
        else if (index < lastSelectedIndex)
            PlaySoundUp();

        lastSelectedIndex = index;
    }
}
