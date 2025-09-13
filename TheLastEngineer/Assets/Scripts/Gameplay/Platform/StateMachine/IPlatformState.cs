public interface IPlatformState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}