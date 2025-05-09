using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialImages : MonoBehaviour
{
    [SerializeField] Image[] myImage;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] NodeType requiredNode;
    // Start is called before the first frame update
    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += EnableImage;
    }
    public void EnableImage(bool a=true, NodeType b=NodeType.Blue)
    {
        text.enabled = !text.enabled;

        foreach (var itme in myImage)
        {
            itme.enabled = !itme.enabled;

        }
    }
}
