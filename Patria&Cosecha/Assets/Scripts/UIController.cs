using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private PlayerTDController _player;
    [SerializeField] private CowsTaskManager _taskManager;
    [SerializeField] private TextMeshProUGUI _objectiveText, _completedText;
    [SerializeField] private Image _descriptionContainer, _objectiveContainer, _completedContainer;

    void Start()
    {
        
    }

    void Update()
    {
        if (_taskManager.TaskStarted)
        {
            _descriptionContainer.gameObject.SetActive(true);
            //_objectiveContainer.gameObject.SetActive(true);
            //_objectiveText.text = $"PUNTOS CONECTADOS {_taskManager.CurrentPoints}/{_taskManager.TotalPoints}";
        }
        else
        {
            _descriptionContainer.gameObject.SetActive(false);
            _objectiveContainer.gameObject.SetActive(false);
        }

        _completedText.text = $"ITEM: {_player.CurrentNode}";
    }
}
