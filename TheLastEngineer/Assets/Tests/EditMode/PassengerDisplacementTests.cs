using NUnit.Framework;
using UnityEngine;

public class PassengerDisplacementTests
{
    [Test]
    public void Consume_AfterTwoAdds_ReturnsTheSum()
    {
        var displacement = new PassengerDisplacement();

        displacement.Add(new Vector3(1f, 0f, 0f));
        displacement.Add(new Vector3(0f, 2f, 0f));

        Assert.AreEqual(new Vector3(1f, 2f, 0f), displacement.Consume());
    }

    [Test]
    public void Consume_Twice_SecondIsZero()
    {
        var displacement = new PassengerDisplacement();
        displacement.Add(Vector3.one);

        displacement.Consume();

        Assert.AreEqual(Vector3.zero, displacement.Consume());
    }

    [Test]
    public void Consume_NothingAdded_IsZero()
    {
        Assert.AreEqual(Vector3.zero, new PassengerDisplacement().Consume());
    }
}
