using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Draws the toon outline over the camera color BEFORE transparents, so glass, particles and
/// beams composite on top of the lines instead of the lines being stamped over them.
///
/// Runs in two render graph passes instead of one:
///   1. Edge coverage into a private R8 target, exposed as _ToonOutlineMask.
///   2. The outline color blended over the camera color using that mask.
///
/// The split is what lets ToonPosterize (AfterRenderingPostProcessing) read the same coverage and
/// skip banding on the line pixels; it also avoids the color buffer copy a read-modify-write blit
/// would need, since pass 2 only writes through hardware blending.
/// </summary>
public class ToonOutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Has to run after ToonDepthNormalsFeature and before the transparent queue.")]
        public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingTransparents;

        [Tooltip("Material using TheLastEngineer/PostProcess/ToonOutline.")]
        public Material outlineMaterial;
    }

    [SerializeField] private Settings _settings = new Settings();

    private ToonOutlinePass _pass;

    public override void Create()
    {
        _pass = new ToonOutlinePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_settings.outlineMaterial == null)
            return;

        _pass.Setup(_settings);
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _pass = null;
    }

    private class ToonOutlinePass : ScriptableRenderPass
    {
        private const string k_PassName = "Toon Outline";

        // Indices into ToonOutline.shader.
        private const int k_MaskPassIndex = 0;
        private const int k_CompositePassIndex = 1;

        private static readonly int k_OutlineMaskId = Shader.PropertyToID("_ToonOutlineMask");
        private static readonly int k_BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");

        private static readonly MaterialPropertyBlock k_PropertyBlock = new MaterialPropertyBlock();

        private RTHandle _maskHandle;
        private Material _material;

        private class PassData
        {
            internal Material material;
            internal int passIndex;
        }

        public ToonOutlinePass()
        {
            profilingSampler = new ProfilingSampler(k_PassName);
        }

        public void Setup(Settings settings)
        {
            renderPassEvent = settings.injectionPoint;
            _material = settings.outlineMaterial;
        }

        public void Dispose()
        {
            _maskHandle?.Release();
            _maskHandle = null;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (!resourceData.activeColorTexture.IsValid())
                return;

            AllocateMask(cameraData);

            // Imported rather than Render Graph managed: the posterize pass reaches the mask by
            // global name, which the graph cannot see, so a transient texture could be aliased
            // away between BeforeRenderingTransparents and AfterRenderingPostProcessing.
            TextureHandle mask = renderGraph.ImportTexture(_maskHandle);

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName + " Mask", out PassData passData, profilingSampler))
            {
                passData.material = _material;
                passData.passIndex = k_MaskPassIndex;

                // Every pixel is written by the full screen triangle, background included, so the
                // target needs no separate clear.
                builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(mask, k_OutlineMaskId);

                // The posterize material samples this by global name; without this the graph sees
                // no consumer for that binding.
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    DrawFullScreen(context.cmd, data));
            }

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName + " Composite", out PassData passData, profilingSampler))
            {
                passData.material = _material;
                passData.passIndex = k_CompositePassIndex;

                builder.UseTexture(mask, AccessFlags.Read);

                // ReadWrite, not Write: the pass blends over what the opaque queue already drew,
                // so the attachment has to be loaded instead of discarded.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    DrawFullScreen(context.cmd, data));
            }
        }

        private static void DrawFullScreen(RasterCommandBuffer cmd, PassData data)
        {
            k_PropertyBlock.Clear();
            // Blit.hlsl's Vert builds the full screen triangle from this, so it has to be set even
            // on the mask pass, which never samples _BlitTexture.
            k_PropertyBlock.SetVector(k_BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));

            cmd.DrawProcedural(Matrix4x4.identity, data.material, data.passIndex,
                MeshTopology.Triangles, 3, 1, k_PropertyBlock);
        }

        private void AllocateMask(UniversalCameraData cameraData)
        {
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;

            // Kept single sampled on purpose: the mask is sampled as a regular texture at
            // AfterRenderingPostProcessing, and it is never an MRT companion of the camera color,
            // so it does not have to follow the camera target's MSAA count.
            descriptor.msaaSamples = 1;
            descriptor.bindMS = false;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            descriptor.enableRandomWrite = false;
            descriptor.depthBufferBits = 0;
            descriptor.graphicsFormat = GraphicsFormat.R8_UNorm;

            RenderingUtils.ReAllocateHandleIfNeeded(ref _maskHandle, descriptor,
                FilterMode.Point, TextureWrapMode.Clamp, name: "_ToonOutlineMask");
        }
    }
}
