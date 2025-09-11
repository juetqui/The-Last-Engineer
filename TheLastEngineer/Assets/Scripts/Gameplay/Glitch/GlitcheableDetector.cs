using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlitcheableDetector : MonoBehaviour
{
    public GlitcheableDetector(float detectionRange, LayerMask selectionLayer)
    {
        _detectionRange = detectionRange;
        _selectionLayer = selectionLayer;
    }

    private float _detectionRange = 10f;
    private LayerMask _selectionLayer;

    public Glitcheable GetNearestGlitcheable(Vector3 position)
    {
        List<Glitcheable> glitcheables = new List<Glitcheable>();
        
        Collider[] hitColliders = Physics.OverlapSphere(position, _detectionRange, _selectionLayer);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                if (glitcheables.Contains(glitcheable)) glitcheables.Remove(glitcheable);
                else glitcheables.Add(glitcheable);
            }
        }

        return glitcheables.OrderBy(glitch => Vector3.Distance(position, glitch.transform.position)).FirstOrDefault();
    }
}
