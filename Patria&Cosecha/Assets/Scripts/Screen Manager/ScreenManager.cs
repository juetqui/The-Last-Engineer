using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    Stack<IScreen> _stack;

    public string lastResult;

    static public ScreenManager Instance;

    void Awake()
    {
        Instance = this;

        _stack = new Stack<IScreen>();
    }

    public void Pop()
    {
        if (_stack.Count < 1) return;

        lastResult = _stack.Pop().Free();

        if (_stack.Count > 0)
        {
            _stack.Peek().Activate();
        }

        Time.timeScale = 1;
    }

    public void Push(IScreen screen)
    {
        Time.timeScale = 0;
        if (_stack.Count > 0)
        {
            Debug.Log("Hay algo en Stack");
            _stack.Peek().Deactivate();
            Pop();
        }

        _stack.Push(screen);

        screen.Activate();
    }

    public void Push(string resource)
    {
        var go = Instantiate(Resources.Load<GameObject>(resource));

        Push(go.GetComponent<IScreen>());
    }
}
