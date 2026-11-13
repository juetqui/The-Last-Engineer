using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Contract of the platform fix: a kinematic platform moved in Update (before the passenger) through
/// Rigidbody.position carries a CharacterController without it losing ground or drifting, whatever the
/// frame rate vs the 0.02 fixed step. Covers the rules + physics approach, not PlatformController's
/// wiring (Assembly-CSharp can't be referenced from a test asmdef); that is checked by hand.
/// </summary>
public class PlatformCarryTests
{
    private const float HighFrameDelta = 1f / 144f; // more frames than physics steps (vSync off)
    private const float LowFrameDelta = 1f / 30f;   // fewer frames than physics steps
    private const float PlatformSpeed = 2f;
    private const float RideDistance = 1.5f;
    private const int SettleFrames = 30;
    private const float OffsetTolerance = 0.02f;
    private static readonly Vector3 PlatformSize = new Vector3(4f, 0.5f, 4f);

    private GameObject _platform;
    private GameObject _passengerObject;
    private TestPlatformDriver _driver;
    private TestPassenger _passenger;
    private bool _previousAutoSync;

    [SetUp]
    public void SetUp()
    {
        // The project runs with autoSyncTransforms off; the fix must work without it.
        _previousAutoSync = Physics.autoSyncTransforms;
        Physics.autoSyncTransforms = false;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_platform != null) Object.Destroy(_platform);
        if (_passengerObject != null) Object.Destroy(_passengerObject);
        Time.captureDeltaTime = 0f;
        Physics.autoSyncTransforms = _previousAutoSync;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Ride_HorizontalAt144Fps_PassengerKeepsItsOffsetAndGround()
    {
        yield return Ride(Vector3.right * RideDistance, HighFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_HorizontalAt30Fps_PassengerKeepsItsOffsetAndGround()
    {
        yield return Ride(Vector3.right * RideDistance, LowFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_VerticalUpAt144Fps_PassengerStaysGroundedEveryFrame()
    {
        yield return Ride(Vector3.up * RideDistance, HighFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_VerticalUpAt30Fps_PassengerStaysGroundedEveryFrame()
    {
        yield return Ride(Vector3.up * RideDistance, LowFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_VerticalDownAt144Fps_PassengerStaysGroundedEveryFrame()
    {
        yield return Ride(Vector3.down * RideDistance, HighFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_VerticalDownAt30Fps_PassengerStaysGroundedEveryFrame()
    {
        yield return Ride(Vector3.down * RideDistance, LowFrameDelta, carryPassenger: true);
        AssertCarried();
    }

    [UnityTest]
    public IEnumerator Ride_ToStop_PlatformEndsExactlyOnTargetAndPassengerGotTheWholePath()
    {
        Vector3 path = new Vector3(1.2f, 0.4f, -0.7f);

        yield return Ride(path, HighFrameDelta, carryPassenger: true);

        Assert.AreEqual(_driver.Target, _platform.transform.position);
        Assert.AreEqual(_driver.Target, _driver.Body.position);
        Assert.AreEqual(0f, (_driver.TotalDelta - path).magnitude, 1e-4f);
    }

    [UnityTest]
    public IEnumerator Ride_PassengerNotRegistered_IsNotCarried()
    {
        Vector3 start = Vector3.zero;

        yield return Ride(Vector3.right, HighFrameDelta, carryPassenger: false, onStart: p => start = p);

        Vector3 moved = _passengerObject.transform.position - start;
        Assert.Less(Mathf.Abs(moved.x), OffsetTolerance, "A passenger nobody reports to must not ride along.");
    }

    private IEnumerator Ride(Vector3 path, float frameDelta, bool carryPassenger, System.Action<Vector3> onStart = null)
    {
        Time.captureDeltaTime = frameDelta;
        Build(carryPassenger);

        for (int i = 0; i < SettleFrames; i++)
            yield return null;

        Assert.IsTrue(_passenger.Controller.isGrounded, "Passenger did not land on the platform before the ride.");

        Vector3 startOffset = _passengerObject.transform.position - _platform.transform.position;
        onStart?.Invoke(_passengerObject.transform.position);

        _driver.Target = _platform.transform.position + path;
        _driver.Moving = true;
        _passenger.Track = true;

        float maxDeviation = 0f;
        int frameBudget = Mathf.CeilToInt(path.magnitude / PlatformSpeed / frameDelta) * 2 + SettleFrames;

        while (!_driver.Arrived && frameBudget-- > 0)
        {
            yield return null;
            Vector3 offset = _passengerObject.transform.position - _platform.transform.position;
            maxDeviation = Mathf.Max(maxDeviation, (offset - startOffset).magnitude);
        }

        _passenger.Track = false;
        Assert.IsTrue(_driver.Arrived, "Platform never reached its target.");
        MaxOffsetDeviation = maxDeviation;
    }

    private float MaxOffsetDeviation { get; set; }

    private void AssertCarried()
    {
        Assert.Greater(_passenger.TrackedFrames, 0);
        Assert.AreEqual(0, _passenger.UngroundedFrames, "Passenger lost the ground while riding.");
        Assert.Less(MaxOffsetDeviation, OffsetTolerance, "Passenger drifted relative to the platform.");
    }

    private void Build(bool carryPassenger)
    {
        _platform = new GameObject("Test Platform");
        var box = _platform.AddComponent<BoxCollider>();
        box.size = PlatformSize;
        var body = _platform.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.None;

        _passengerObject = new GameObject("Test Passenger");
        var controller = _passengerObject.AddComponent<CharacterController>();
        float halfHeight = controller.height * 0.5f;
        _passengerObject.transform.position = new Vector3(0f, PlatformSize.y * 0.5f + halfHeight + controller.skinWidth * 2f, 0f);
        _passenger = _passengerObject.AddComponent<TestPassenger>();
        _passenger.Controller = controller;

        _driver = _platform.AddComponent<TestPlatformDriver>();
        _driver.Body = body;
        _driver.Speed = PlatformSpeed;
        _driver.Passenger = carryPassenger ? _passenger.Displacement : null;

        Physics.SyncTransforms(); // test setup only: objects created this frame
    }
}
