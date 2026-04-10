using System;

public class GlitchStateMachine
{
    public Action<IState> OnStateChanged;
    
    public IState Current {  get; private set; }

    public void Change(IState next)
    {
        if (ReferenceEquals(Current, next)) return;

        Current?.Exit();
        Current = next;
        Current?.Enter();
        OnStateChanged?.Invoke(Current);
    }

    public void Tick(float dt) => Current.Tick(dt);
}
