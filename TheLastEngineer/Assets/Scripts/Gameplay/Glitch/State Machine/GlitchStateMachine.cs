public class GlitchStateMachine
{
    public IState Current {  get; private set; }

    public void Change(IState next)
    {
        if (ReferenceEquals(Current, next)) return;

        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Tick(float dt) => Current.Tick(dt);
}
