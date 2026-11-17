using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives the <see cref="RouteStationLight"/>s of a platform's stations from its events: with power, the
/// station it heads to blinks and the rest stay lit; without power they go dark, except the target, which keeps
/// blinking until the platform comes to rest. Stations without a <see cref="RouteStationLight"/> are skipped.
/// </summary>
public class PlatformStationLights : MonoBehaviour
{
    [SerializeField] private PlatformController _platform;

    private readonly Dictionary<Transform, RouteStationLight> _lights = new Dictionary<Transform, RouteStationLight>();
    private RouteStationLight _target;
    private bool _powered;
    private bool _subscribed;

    private void Awake()
    {
        if (_platform == null)
            _platform = GetComponent<PlatformController>();

        if (_platform == null)
        {
            Debug.LogError($"{nameof(PlatformStationLights)} on '{name}' needs a {nameof(PlatformController)}; disabling it.", this);
            enabled = false;
            return;
        }

        // Read from the serialized route, so the lights are known even if the platform hasn't run Awake yet.
        foreach (StationsStops stop in _platform.Stops)
        {
            if (stop == null || !stop.IsStation || stop.Position == null) continue;

            if (stop.Position.TryGetComponent(out RouteStationLight stationLight))
                _lights[stop.Position] = stationLight;
        }
    }

    // Subscribed in OnEnable, which runs before Connection.Start announces connections that start plugged.
    private void OnEnable()
    {
        if (_platform == null || _subscribed) return;

        _platform.PoweredOn += HandlePoweredOn;
        _platform.PoweredOff += HandlePoweredOff;
        _platform.BecameInactive += HandleBecameInactive;
        _platform.StationReached += HandleStationReached;
        _platform.TargetStationChanged += HandleTargetStationChanged;
        _subscribed = true;
    }

    private void OnDisable()
    {
        if (_subscribed && _platform != null)
        {
            _platform.PoweredOn -= HandlePoweredOn;
            _platform.PoweredOff -= HandlePoweredOff;
            _platform.BecameInactive -= HandleBecameInactive;
            _platform.StationReached -= HandleStationReached;
            _platform.TargetStationChanged -= HandleTargetStationChanged;
        }
        _subscribed = false;
    }

    private void HandlePoweredOn()
    {
        _powered = true;
        _target = LightAt(_platform.TargetStation);

        foreach (RouteStationLight stationLight in _lights.Values)
        {
            if (stationLight == _target) stationLight.StartBlinking();
            else stationLight.TurnOn();
        }
    }

    private void HandlePoweredOff()
    {
        _powered = false;

        // The target keeps blinking while the platform finishes its trip; BecameInactive turns it off.
        foreach (RouteStationLight stationLight in _lights.Values)
        {
            if (stationLight != _target)
                stationLight.TurnOff();
        }
    }

    private void HandleBecameInactive()
    {
        _powered = false;
        _target = null;

        foreach (RouteStationLight stationLight in _lights.Values)
            stationLight.TurnOff();
    }

    private void HandleStationReached(Transform station)
    {
        if (!_powered) return;

        RouteStationLight stationLight = LightAt(station);
        if (stationLight != null)
            stationLight.TurnOn();
    }

    private void HandleTargetStationChanged(Transform station)
    {
        // While unpowered the next target only matters once power comes back (HandlePoweredOn reads it).
        if (!_powered) return;

        _target = LightAt(station);
        if (_target != null)
            _target.StartBlinking();
    }

    private RouteStationLight LightAt(Transform station)
    {
        return station != null && _lights.TryGetValue(station, out RouteStationLight stationLight) ? stationLight : null;
    }
}
