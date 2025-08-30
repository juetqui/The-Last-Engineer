using UnityEngine;

public static class FxUtils
{
    // M�todo de extensi�n para reproducir part�culas de manera segura
    public static void SafePlay(this ParticleSystem ps)
    {
        if (ps != null && !ps.isPlaying)
            ps.Play();
    }

    // M�todo de extensi�n para detener part�culas de manera segura
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
