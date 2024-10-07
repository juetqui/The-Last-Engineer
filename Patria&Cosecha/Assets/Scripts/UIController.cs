using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private PlayerTDController _player;
    [SerializeField] private TaskManager _taskManager;
    //[SerializeField] private TextMeshProUGUI _objectiveText, _completedText;
    //[SerializeField] private Image _descriptionContainer, _objectiveContainer, _completedContainer;

    void Start()
    {
        
    }

    void Update()
    {
        //if (_taskManager.Running)
        //{
        //    _objectiveContainer.gameObject.SetActive(true);
        //    _objectiveText.text = "NODOS CONECTADOS CORRECTAMENTE";
        //}
        //else
        //{
        //    _descriptionContainer.gameObject.SetActive(false);
        //    _objectiveContainer.gameObject.SetActive(false);
        //}

        //if (_player.CurrentNode != NodeType.None)
        //{
        //    _completedContainer.gameObject.SetActive(true);
        //    _completedText.text = $"ITEM: {_player.CurrentNode}";
        //}
        //else _completedContainer.gameObject.SetActive(false);
    }
}
