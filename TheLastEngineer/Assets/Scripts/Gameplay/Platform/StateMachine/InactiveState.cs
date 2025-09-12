//public class InactiveState : IPlatformState
//{
//    private PlatformController _pc;

//    public void Enter()
//    {
//        _pc.WaitTimer = 0f;
//        _pc.StopPassenger(); // asegura desplazamiento cero
//    }

//    public void Tick()
//    {
//        // No hace nada mientras el cable/condición no habilite
//        // El cambio a Waiting lo dispara el evento OnConnectionChanged
//    }
//    public void Exit() { }

//}