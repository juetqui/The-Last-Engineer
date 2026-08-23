using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

/// <summary>
/// Fills a private depth + normals pair, exposed as _ToonDepthTexture and _ToonNormalsTexture,
/// with opaque AND selected transparent geometry.
///
/// URP builds its own DepthNormals prepass with RenderQueueRange.opaque hardcoded, so transparents
/// never reach _CameraDepthTexture / _CameraNormalsTexture. ToonPostProcess used to read those, which
/// meant a pixel covered by glass reported the depth of whatever was BEHIND it: the outline of the
/// object behind was drawn on top of the glass, and the glass itself got no outline at all.
/// </summary>
public class ToonDepthNormalsFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Has to run before the Toon full screen pass, which sits at AfterRenderingPostProcessing.")]
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPrePasses;

        [Tooltip("Opaque geometry that contributes edges. Mirror the renderer's Opaque Layer Mask here.")]
        public LayerMask opaqueLayerMask = ~0;

        [Tooltip("Transparent geometry that contributes edges. Uncheck full screen effects such as fog, " +
                 "clouds or laser beams: they would fill the whole depth buffer and erase every edge behind them.")]
        public LayerMask transparentLayerMask = ~0;
    }

    [SerializeField] private Settings _settings = new Settings();

    private ToonDepthNormalsPass _pass;

    public override void Create()
    {
        _pass = new ToonDepthNormalsPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _pass.Setup(_settings);
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _pass = null;
    }

    private class ToonDepthNormalsPass : ScriptableRenderPass
    {
        private const string k_PassName = "Toon DepthNormals";

        // Same tags URP's own prepass draws. Handwritten shaders (URP Lit) expose "DepthNormals",
        // Shader Graph ones expose "DepthNormalsOnly". Both force ZWrite On / ZTest LEqual regardless
        // of the surface type, so transparents resolve nearest-first without needing a sorted draw.
        private static readonly List<ShaderTagId> k_ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("DepthNormals"),
            new ShaderTagId("DepthNormalsOnly"),
        };

        private static readonly int k_ToonDepthTextureId = Shader.PropertyToID("_ToonDepthTexture");
        private static readonly int k_ToonNormalsTextureId = Shader.PropertyToID("_ToonNormalsTexture");
        private static readonly int k_ToonDepthTexelSizeId = Shader.PropertyToID("_ToonDepthTexture_TexelSize");

        private RTHandle _depthHandle;
        private RTHandle _normalsHandle;

        private LayerMask _opaqueLayerMask;
        private LayerMask _transparentLayerMask;

        private class PassData
        {
            internal RendererListHandle opaqueList;
            internal RendererListHandle transparentList;
            internal Vector4 depthTexelSize;
        }

        public ToonDepthNormalsPass()
        {
            profilingSampler = new ProfilingSampler(k_PassName);
        }

        public void Setup(Settings settings)
        {
            renderPassEvent = settings.injectionPoint;
            _opaqueLayerMask = settings.opaqueLayerMask;
            _transparentLayerMask = settings.transparentLayerMask;
        }

        public void Dispose()
        {
            _depthHandle?.Release();
            _normalsHandle?.Release();
            _depthHandle = null;
            _normalsHandle = null;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
                return;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            AllocateTargets(cameraData);

            // Imported rather than Render Graph managed: nothing inside the graph declares a read on
            // these, so a transient texture could be aliased away before the Toon material samples it.
            TextureHandle normals = renderGraph.ImportTexture(_normalsHandle);
            TextureHandle depth = renderGraph.ImportTexture(_depthHandle);

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName, out PassData passData, profilingSampler))
            {
                builder.SetRenderAttachment(normals, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(depth, AccessFlags.ReadWrite);

                passData.opaqueList = CreateRendererList(renderGraph, renderingData, cameraData, lightData,
                    RenderQueueRange.opaque, _opaqueLayerMask);
                passData.transparentList = CreateRendererList(renderGraph, renderingData, cameraData, lightData,
                    RenderQueueRange.transparent, _transparentLayerMask);

                builder.UseRendererList(passData.opaqueList);
                builder.UseRendererList(passData.transparentList);

                builder.SetGlobalTextureAfterPass(normals, k_ToonNormalsTextureId);
                builder.SetGlobalTextureAfterPass(depth, k_ToonDepthTextureId);

                // Render Graph binds those globals by RenderTargetIdentifier, and that overload does
                // NOT fill in the companion _TexelSize. Left to Unity it would stay at zero and the
                // Roberts cross would collapse onto a single texel: no outline at all.
                passData.depthTexelSize = new Vector4(
                    1f / _depthHandle.rt.width, 1f / _depthHandle.rt.height,
                    _depthHandle.rt.width, _depthHandle.rt.height);

                // The Toon material reaches these by global name, which Render Graph cannot see, so
                // without this the whole pass looks unused and gets culled.
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalVector(k_ToonDepthTexelSizeId, data.depthTexelSize);
                    context.cmd.ClearRenderTarget(RTClearFlags.ColorDepth, Color.black, 1f, 0);
                    context.cmd.DrawRendererList(data.opaqueList);
                    context.cmd.DrawRendererList(data.transparentList);
                });
            }
        }

        private void AllocateTargets(UniversalCameraData cameraData)
        {
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.bindMS = false;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            descriptor.enableRandomWrite = false;

            // Signed format so the normals survive the [-1, 1] range, matching URP's own prepass.
            RenderTextureDescriptor normalsDescriptor = descriptor;
            normalsDescriptor.depthBufferBits = 0;
            normalsDescriptor.graphicsFormat = DepthNormalOnlyPass.GetGraphicsFormat();
            RenderingUtils.ReAllocateHandleIfNeeded(ref _normalsHandle, normalsDescriptor,
                FilterMode.Point, TextureWrapMode.Clamp, name: "_ToonNormalsTexture");

            RenderTextureDescriptor depthDescriptor = descriptor;
            depthDescriptor.graphicsFormat = GraphicsFormat.None;
            depthDescriptor.depthStencilFormat = GraphicsFormat.D32_SFloat;
            RenderingUtils.ReAllocateHandleIfNeeded(ref _depthHandle, depthDescriptor,
                FilterMode.Point, TextureWrapMode.Clamp, name: "_ToonDepthTexture");
        }

        private static RendererListHandle CreateRendererList(RenderGraph renderGraph,
            UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData,
            RenderQueueRange queueRange, LayerMask layerMask)
        {
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                k_ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
            drawingSettings.perObjectData = PerObjectData.None;

            FilteringSettings filteringSettings = new FilteringSettings(queueRange, layerMask);

            return renderGraph.CreateRendererList(
                new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings));
        }
    }
}
