//public class MovingState : IPlatformState
//{
//    private PlatformController _pc;
//    public void Enter()
//    {
//        // Nada especial; el target se eval�a cada frame desde RouteManager
//    }

//    public void Tick()
//    {
//        if (_pc.ReachedTarget())
//        {
//            _pc.StopPassenger();
//            _pc.AdvanceRouteAndWait(); // Avanza al pr�ximo punto y vuelve a esperar
//            return;
//        }

//        _pc.MoveStep();
//    }

//    public void Exit() { }
//}