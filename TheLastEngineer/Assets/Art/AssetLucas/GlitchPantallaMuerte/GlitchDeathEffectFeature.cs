using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchDeathEffectFeature : ScriptableRendererFeature
{
    class GlitchRenderPass : ScriptableRenderPass
    {
        Material glitchMaterial;
        string profilerTag;

        public bool isActive = false;

        public float Intensity = 0.5f;
        public float RGBSplit = 1;
        public float Speed = 5;
        public float GlitchTexBlend = 0.5f;

        public GlitchRenderPass(Material material, string tag)
        {
            glitchMaterial = material;
            profilerTag = tag;
        }

        public void Setup() { }

        /*
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureClear(ClearFlag.None, Color.clear);
        }
        */

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!isActive || glitchMaterial == null)
                return;

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            // Seteamos los parámetros del shader
            glitchMaterial.SetFloat("_Intensity", Intensity);
            glitchMaterial.SetFloat("_RGBSplit", RGBSplit);
            glitchMaterial.SetFloat("_Speed", Speed);
            glitchMaterial.SetFloat("_GlitchTexBlend", GlitchTexBlend);

            // Aplicamos el shader directamente sobre la imagen de cámara
            cmd.Blit(cameraColorTarget, cameraColorTarget, glitchMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            Debug.Log("EJECUCION GLITCH");
        }

        /*
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
        */
    }

    [System.Serializable]
    public class GlitchSettings
    {
        public Material glitchMaterial = null;
    }

    public GlitchSettings settings = new GlitchSettings();

    GlitchRenderPass glitchPass;

    public override void Create()
    {
        glitchPass = new GlitchRenderPass(settings.glitchMaterial, "Glitch Effect");
        glitchPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        glitchPass.Setup();
        renderer.EnqueuePass(glitchPass);
    }

    // Activar glitch desde otro script (ej: al morir el jugador)
    public void ActivateGlitch()
    {
        glitchPass.isActive = true;
    }

    public void DeactivateGlitch()
    {
        glitchPass.isActive = false;
    }

    public void SetGlitchValues(float intensity, float rgbSplit, float speed, float glitchTexBlend)
    {
        if (glitchPass != null)
        {
            glitchPass.Intensity = intensity;
            glitchPass.RGBSplit = rgbSplit;
            glitchPass.Speed = speed;
            glitchPass.GlitchTexBlend = glitchTexBlend;
        }
    }
}

