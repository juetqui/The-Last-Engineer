using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Outline + relleno "rayos X" del jugador cuando una pared SOLIDA lo tapa.
///
/// En los niveles con verticalidad casi siempre hay un piso semitransparente (FloorDitherManager)
/// entre la camara y el jugador, y ese dither es opaco con alpha clip: escribe depth en los pixeles
/// que sobreviven. Por eso no alcanza con un ZTest Greater contra el depth de camara (saldria un
/// outline punteado) y se arma un depth propio filtrado por Rendering Layers:
///   1. Occluder Depth: depth-only de los opacos, sin el jugador ni los pisos dithered.
///   2. Mask: el jugador (override material) a un R8G8: R = jugador, G = jugador tapado.
///   3. Composite: relleno en G y outline alrededor de G, solo fuera de la silueta.
///
/// Se filtra por Rendering Layer y no por layer fisica porque Walls/Floor/Glitched se usan en
/// raycasts y Glitcheable las cambia en runtime.
/// </summary>
public class PlayerXRayFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Cuando se llena el depth de oclusores. Tiene que ser antes del evento de composicion.")]
        public RenderPassEvent occluderDepthEvent = RenderPassEvent.AfterRenderingPrePasses;

        [Tooltip("Mascara + composicion. Despues de transparentes para que la niebla volumetrica no tape el efecto.")]
        public RenderPassEvent compositeEvent = RenderPassEvent.AfterRenderingTransparents;

        [Tooltip("Material con TheLastEngineer/PostProcess/PlayerXRay.")]
        public Material xrayMaterial;

        [Tooltip("Layers fisicas que cuentan como pared. Sacar de aca Glitched/Intangible si no deben tapar.")]
        public LayerMask occluderLayerMask = ~0;

        [Tooltip("Rendering Layer que PlayerXRayTarget pone en el jugador.")]
        public RenderingLayerMask playerRenderingLayer;

        [Tooltip("Rendering Layer que FloorDitherManager pone en los pisos con opacidad reducida.")]
        public RenderingLayerMask ditheredRenderingLayer;

        [Tooltip("Dibujar tambien en la Scene view (util para ajustar el material).")]
        public bool includeSceneView = false;
    }

    [SerializeField] private Settings _settings = new Settings();

    private OccluderDepthPass _occluderPass;
    private XRayPass _xrayPass;

    public override void Create()
    {
        _occluderPass = new OccluderDepthPass();
        _xrayPass = new XRayPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Sin jugador visible (muerto, en cinematica, sin el componente) no se paga ningun pass.
        if (_settings.xrayMaterial == null || PlayerXRayTarget.ActiveCount == 0)
            return;

        _occluderPass.Setup(_settings);
        _xrayPass.Setup(_settings);
        renderer.EnqueuePass(_occluderPass);
        renderer.EnqueuePass(_xrayPass);
    }

    private static bool ShouldRender(UniversalCameraData cameraData, bool includeSceneView)
    {
        if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
            return false;

        return includeSceneView || cameraData.cameraType != CameraType.SceneView;
    }

    // Lleva el depth de oclusores de un pass al otro: van en eventos distintos, asi que son dos
    // ScriptableRenderPass y la textura transient se comparte por el frameData del grafo.
    private class XRayFrameData : ContextItem
    {
        public TextureHandle occluderDepth;

        public override void Reset()
        {
            occluderDepth = TextureHandle.nullHandle;
        }
    }

    private class OccluderDepthPass : ScriptableRenderPass
    {
        private const string k_PassName = "XRay Occluder Depth";

        // Lo exponen tanto los shaders de URP escritos a mano como los de Shader Graph. Respeta el
        // alpha clip de cada material: un piso a opacidad 0 no escribe nada aunque no este filtrado.
        private static readonly List<ShaderTagId> k_ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("DepthOnly"),
        };

        private static readonly int k_OccluderDepthId = Shader.PropertyToID("_XRayOccluderDepth");

        private LayerMask _layerMask;
        private uint _renderingLayerMask;
        private bool _includeSceneView;

        private class PassData
        {
            internal RendererListHandle rendererList;
        }

        public OccluderDepthPass()
        {
            profilingSampler = new ProfilingSampler(k_PassName);
        }

        public void Setup(Settings settings)
        {
            renderPassEvent = settings.occluderDepthEvent;
            _layerMask = settings.occluderLayerMask;
            _renderingLayerMask = ~(settings.playerRenderingLayer.value | settings.ditheredRenderingLayer.value);
            _includeSceneView = settings.includeSceneView;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (!ShouldRender(cameraData, _includeSceneView))
                return;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.bindMS = false;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            descriptor.graphicsFormat = GraphicsFormat.None;
            descriptor.depthStencilFormat = GraphicsFormat.D32_SFloat;

            // Transient: el unico consumidor es el pass de mascara, que declara la lectura, asi que el
            // grafo no lo puede cullear ni aliasear antes de tiempo.
            TextureHandle depth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor,
                "_XRayOccluderDepth", false);

            XRayFrameData xrayData = frameData.Create<XRayFrameData>();
            xrayData.occluderDepth = depth;

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName, out PassData passData, profilingSampler))
            {
                builder.SetRenderAttachmentDepth(depth, AccessFlags.Write);

                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    k_ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
                drawingSettings.perObjectData = PerObjectData.None;

                FilteringSettings filteringSettings =
                    new FilteringSettings(RenderQueueRange.opaque, _layerMask, _renderingLayerMask);

                passData.rendererList = renderGraph.CreateRendererList(
                    new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings));
                builder.UseRendererList(passData.rendererList);

                builder.SetGlobalTextureAfterPass(depth, k_OccluderDepthId);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(RTClearFlags.Depth, Color.clear, 1f, 0);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }
        }
    }

    private class XRayPass : ScriptableRenderPass
    {
        private const string k_PassName = "XRay";

        // Indices en PlayerXRay.shader.
        private const int k_MaskPassIndex = 0;
        private const int k_CompositePassIndex = 1;

        // Pases "de color" del jugador. El override material reemplaza el shader, estos tags solo
        // deciden que materiales entran: Lit/Shader Graph Lit, Unlit y los Forward-only.
        private static readonly List<ShaderTagId> k_ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
        };

        private static readonly int k_XRayMaskId = Shader.PropertyToID("_XRayMask");
        private static readonly int k_XRayMaskTexelId = Shader.PropertyToID("_XRayMaskTexel");
        private static readonly int k_BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");

        private static readonly MaterialPropertyBlock k_PropertyBlock = new MaterialPropertyBlock();

        private Material _material;
        private uint _playerRenderingLayer;
        private bool _includeSceneView;

        private class MaskPassData
        {
            internal RendererListHandle rendererList;
        }

        private class CompositePassData
        {
            internal Material material;
            internal Vector4 maskTexel;
        }

        public XRayPass()
        {
            profilingSampler = new ProfilingSampler(k_PassName);
        }

        public void Setup(Settings settings)
        {
            renderPassEvent = settings.compositeEvent;
            _material = settings.xrayMaterial;
            _playerRenderingLayer = settings.playerRenderingLayer.value;
            _includeSceneView = settings.includeSceneView;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Si el pass de oclusores no corrio (camara filtrada), no hay nada contra que comparar.
            if (!frameData.Contains<XRayFrameData>())
                return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (!ShouldRender(cameraData, _includeSceneView))
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (!resourceData.activeColorTexture.IsValid())
                return;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            TextureHandle occluderDepth = frameData.Get<XRayFrameData>().occluderDepth;

            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.bindMS = false;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;

            RenderTextureDescriptor maskDescriptor = descriptor;
            maskDescriptor.depthStencilFormat = GraphicsFormat.None;
            maskDescriptor.graphicsFormat = GraphicsFormat.R8G8_UNorm;
            TextureHandle mask = UniversalRenderer.CreateRenderGraphTexture(renderGraph, maskDescriptor,
                "_XRayMask", false);

            // Depth propio del jugador: solo sirve para quedarse con su fragmento mas cercano.
            RenderTextureDescriptor maskDepthDescriptor = descriptor;
            maskDepthDescriptor.graphicsFormat = GraphicsFormat.None;
            maskDepthDescriptor.depthStencilFormat = GraphicsFormat.D32_SFloat;
            TextureHandle maskDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, maskDepthDescriptor,
                "_XRayMaskDepth", false);

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName + " Mask", out MaskPassData passData, profilingSampler))
            {
                builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(maskDepth, AccessFlags.Write);

                // El shader lo lee por el global que dejo el pass de oclusores; declarar la lectura es
                // lo que encadena los dos passes en el grafo.
                builder.UseTexture(occluderDepth, AccessFlags.Read);

                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    k_ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
                drawingSettings.perObjectData = PerObjectData.None;
                drawingSettings.overrideMaterial = _material;
                drawingSettings.overrideMaterialPassIndex = k_MaskPassIndex;

                // Todas las colas: el overlay intangible del jugador puede ser transparente y el
                // override lo vuelve a dibujar igual, sin efecto en la mascara.
                FilteringSettings filteringSettings =
                    new FilteringSettings(RenderQueueRange.all, ~0, _playerRenderingLayer);

                passData.rendererList = renderGraph.CreateRendererList(
                    new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings));
                builder.UseRendererList(passData.rendererList);

                builder.SetGlobalTextureAfterPass(mask, k_XRayMaskId);

                builder.SetRenderFunc((MaskPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(RTClearFlags.ColorDepth, Color.clear, 1f, 0);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass(k_PassName + " Composite", out CompositePassData passData,
                       profilingSampler))
            {
                passData.material = _material;
                passData.maskTexel = new Vector4(1f / maskDescriptor.width, 1f / maskDescriptor.height,
                    maskDescriptor.width, maskDescriptor.height);

                builder.UseTexture(mask, AccessFlags.Read);

                // ReadWrite, no Write: se mezcla sobre lo ya dibujado, el attachment se tiene que cargar.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                builder.SetRenderFunc((CompositePassData data, RasterGraphContext context) =>
                {
                    k_PropertyBlock.Clear();
                    // Blit.hlsl arma el triangulo full screen con esto.
                    k_PropertyBlock.SetVector(k_BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));
                    k_PropertyBlock.SetVector(k_XRayMaskTexelId, data.maskTexel);

                    context.cmd.DrawProcedural(Matrix4x4.identity, data.material, k_CompositePassIndex,
                        MeshTopology.Triangles, 3, 1, k_PropertyBlock);
                });
            }
        }
    }
}
