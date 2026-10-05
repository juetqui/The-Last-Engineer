using System;

// Fuente que puede "activar" algo (puertas, etc.). Cada fuente decide cuando esta satisfecha,
// asi quien escucha no necesita conocer su tipo concreto (Connection, Inspectionable...).
public interface IActivationSource
{
    bool IsActive { get; }
    event Action<bool> OnActivationChanged;
}
