using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grieta del caldero: avisa cuando pasa de vacía a tapada y viceversa.
///
/// La entrada la detecta PhysX (por eso el Rigidbody kinematic: sin él ningún lado de la colisión
/// tiene Rigidbody y no hay eventos de trigger). La salida NO puede venir de OnTriggerExit: el
/// Glitcheable se va apagando su collider, se mueve y lo prende en el destino, y Unity no manda
/// Exit cuando un collider se apaga adentro del trigger. Por eso, mientras un Glitcheable está
/// adentro, la grieta escucha su FSM y lo da por salido cuando el collider quedó apagado.
///
/// Invariante: estar suscripto a la FSM de un Glitcheable == tenerlo en _inside. Si siguiéramos
/// suscriptos después de que se fue, al apagar su collider en OTRA posición contaríamos una
/// salida sin que haya vuelto a entrar.
///
/// Límites conocidos: si un Glitcheable se destruye estando adentro nadie avisa y la grieta queda
/// tapada; si origen y destino caen en la misma grieta, en el salto avisa false y después true.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CauldronCrack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    public Action<bool> OnCrackCovered;

    public bool IsCovered => _inside.Count > 0;
    
    private ParticleSystem _ps;

    // Se guarda el delegate de cada uno: OnStateChanged no dice quién cambió, así que cada
    // handler captura su Glitcheable, y para desuscribir hace falta esa misma instancia.
    private readonly Dictionary<Glitcheable, Action<IState>> _inside = new Dictionary<Glitcheable, Action<IState>>();

    private void Awake()
    {
        OnCrackCovered += UpdateCovered;
        _ps = GetComponentInChildren<ParticleSystem>();
    }

    private void Reset()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Glitcheable g)) Register(g);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Glitcheable g)) Unregister(g);
    }

    private void OnDisable()
    {
        if (_inside.Count == 0) return;

        foreach (var pair in _inside)
            Unsubscribe(pair.Key, pair.Value);

        _inside.Clear();
        Notify(false);
    }

    private void Register(Glitcheable g)
    {
        if (g.FSM == null) return;

        Action<IState> handler = _ => OnGlitchStateChanged(g);

        if (!_inside.TryAdd(g, handler)) return;

        g.FSM.OnStateChanged += handler;
        
        if (_inside.Count == 1) Notify(true);
    }

    private void OnGlitchStateChanged(Glitcheable g)
    {
        if (g._coll != null && g._coll.enabled) return;
        Unregister(g);
    }

    private void Unregister(Glitcheable g)
    {
        if (!_inside.Remove(g, out var handler)) return;
        Unsubscribe(g, handler);
        if (_inside.Count == 0) Notify(false);
    }

    private static void Unsubscribe(Glitcheable g, Action<IState> handler)
    {
        var fsm = g.FSM;
        if (fsm != null) fsm.OnStateChanged -= handler;
    }

    private void Notify(bool covered) => OnCrackCovered?.Invoke(covered);

    private void UpdateCovered(bool covered)
    {
        if (!covered) _ps.Play();
        else _ps.Stop();
    }
}
