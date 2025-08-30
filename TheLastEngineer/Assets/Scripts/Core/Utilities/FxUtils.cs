using UnityEngine;

public static class FxUtils
{
    // Método de extensión para reproducir partículas de manera segura
    public static void SafePlay(this ParticleSystem ps)
    {
        if (ps != null && !ps.isPlaying)
            ps.Play();
    }

    // Método de extensión para detener partículas de manera segura
    public static void SafeStop(this ParticleSystem ps, bool clear = false)
    {
        if (ps != null)
        {
            if (clear)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                ps.Stop();
        }
    }
}
