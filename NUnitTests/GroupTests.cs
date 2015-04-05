using UntisExp;
using NUnit.Framework;
using UntisExp.Containers;

namespace NUnitTests
{
    [TestFixture]
    public class GroupTests
    {

        [Test]
        [Category("Constructors")]
        public void NormalConstructorIdWillBeEmpty()
        {
            Assert.AreEqual(0, (new Group().Id));
        }

        [Test]
        [Category("Constructors")]
        public void NormalConstructorNameWillBeEmpty()
        {
            Assert.AreEqual(null, (new Group().ClassName));
        }

        [Test]
        [Category("Constructors")]
        public void PrefilledConstructorWillFillOutName()
        {
            Assert.AreEqual("Q2", (new Group(35, "Q2").ClassName));
        }

        [Test]
        [Category("Constructors")]
        public void PrefilledConstructorWillFillOutId()
        {
            Assert.AreEqual(2, (new Group(2, "5A").Id));
        }

        [Test]
        public void ToStringReturnsClassName()
        {
            var sut = new Group (36, "Q3");
            Assert.AreEqual("Q3", sut.ToString());
        }
    }
}