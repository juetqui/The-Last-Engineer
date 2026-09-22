/// <summary>
/// Nivel de carga de glitch de un objeto, de 0 a 2. Reemplaza al viejo NodeType binario:
/// la corrupcion dejo de ser un flag que salta entre el nodo y el objeto y paso a ser un
/// recurso que se transfiere de a una unidad.
///
/// No existe un valor centinela para "sin nodo en mano": Clean es un nivel legitimo, asi que
/// esa condicion se pregunta con PlayerNodeHandler.HasNode.
/// </summary>
public enum GlitchState
{
    Clean = 0,
    Intangible = 1,
    Glitched = 2
}
