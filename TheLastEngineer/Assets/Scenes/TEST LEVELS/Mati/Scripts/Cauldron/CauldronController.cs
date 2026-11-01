using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class CauldronController : MonoBehaviour
{
    [Header("Pivot")]
    [SerializeField] private Transform targetPos;
    [SerializeField] private CauldronView pivotMover;

    [Header("Cracks")]
    [SerializeField] private List<CauldronCrack> cracks;

    [Header("Eases")]
    // Una sola curva para ambas direcciones: la posición depende solo del contador, así el movimiento es reversible.
    [SerializeField] private Ease progressCurve = Ease.InExpo;
    [SerializeField] private Ease approachEase = Ease.OutQuad;
    [SerializeField] private Ease retreatEase = Ease.InOutQuad;

    private IPivotMover _pivotMover;
    private CauldronModel _model;
    private Vector3 _startPosition;

    private void Awake()
    {
        _pivotMover = pivotMover as IPivotMover;
        _startPosition = _pivotMover.Position;

        // La cantidad máxima de pasos es la cantidad de grietas.
        _model = new CauldronModel(cracks.Count);
    }

    private void OnEnable() => Subscribe(true);
    private void OnDisable() => Subscribe(false);

    private void Subscribe(bool subscribe)
    {
        foreach (var c in cracks)
        {
            if (c == null) continue;

            if (subscribe)
                c.OnCrackCovered += UpdateCoveredCracks;
            else
                c.OnCrackCovered -= UpdateCoveredCracks;
        }
    }

    private void UpdateCoveredCracks(bool isCovered)
    {
        bool changed = isCovered ? _model.StepForward() : _model.StepBack();

        if (!changed) return;

        _pivotMover.MoveTo(CalculateDestination(), isCovered ? approachEase : retreatEase);
    }

    // Único lugar que traduce contador → posición. Con t en [0,1] y curvas que no rebotan (InExpo, InQuad...)
    // el destino queda entre la posición inicial y el target; una curva Back/Elastic sí se pasaría.
    private Vector3 CalculateDestination()
    {
        float curvedProgress = Easing.Evaluate(_model.Progress, progressCurve);
        return Vector3.LerpUnclamped(_startPosition, targetPos.position, curvedProgress);
    }
}
