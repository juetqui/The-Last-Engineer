namespace MaskTransitions
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Rendering;

    public class CutoutMaskUI : Image
    {
        private Material cachedMaterial;

        public CompareFunction stencilComparison = CompareFunction.NotEqual;

        public override Material materialForRendering
        {
            get
            {
                if (cachedMaterial == null)
                {
                    cachedMaterial = new Material(base.materialForRendering);
                }
                cachedMaterial.SetInt("_StencilComp", (int)stencilComparison);
                return cachedMaterial;
            }
        }

        protected override void OnDestroy()
        {
            if (cachedMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(cachedMaterial);
                }
                else
                {
                    DestroyImmediate(cachedMaterial);
                }
            }
            base.OnDestroy();
        }

        public void SetStencilComparison(CompareFunction comparison)
        {
            stencilComparison = comparison;
            SetMaterialDirty(); // Forzar a que el material se actualice
        }
    }

}

