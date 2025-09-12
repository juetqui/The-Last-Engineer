//using UnityEngine;

//public class WaitingState : IPlatformState
//{
//    private PlatformController _pc;
//    private PlatformStateMachine _FSM;

//    public void Enter()
//    {
//        // Reinicia la cuenta
//        _pc.BeginWait();
//        _pc.StopPassenger(); // por si venía de Moving
//    }

//    public void Tick()
//    {
//        if (_pc.WaitTimer > 0f)
//        {
//            _pc.WaitTimer -= Time.deltaTime;
//            if (_pc.WaitTimer <= 0f)
//            {
//                _FSM.TransitionTo(new MovingState()); // O usar instancia cacheada si preferís (ver nota abajo)
//            }
//        }
//    }

//    public void Exit() { }
//}