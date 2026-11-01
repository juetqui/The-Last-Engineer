// Estado puro del caldero: cuántos pasos avanzó el pivot hacia el target. No sabe de transforms ni tweens;
// la posición se deriva siempre de Progress, así un avance seguido de un retroceso vuelve exactamente al mismo punto.
public class CauldronModel
{
    private readonly int _maxSteps;
    private int _currentStep = 0;

    public int CurrentStep => _currentStep;
    public int MaxSteps => _maxSteps;

    // Sin pasos posibles (lista vacía) el progreso queda en 0 para evitar dividir por cero.
    public float Progress => _maxSteps > 0 ? (float)_currentStep / _maxSteps : 0f;

    public CauldronModel(int maxSteps)
    {
        _maxSteps = maxSteps < 0 ? 0 : maxSteps;
    }

    // Devuelven si el contador cambió: en los extremos el evento se ignora y no hay nada que animar.
    public bool StepForward() => SetStep(_currentStep + 1);
    public bool StepBack() => SetStep(_currentStep - 1);

    private bool SetStep(int step)
    {
        if (step < 0) step = 0;
        else if (step > _maxSteps) step = _maxSteps;

        if (step == _currentStep) return false;

        _currentStep = step;
        return true;
    }
}
