using NUnit.Framework;
using PersistentWAVL.Version;

namespace PersistentWAVL.Tests
{
    public class VersionTreeTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSuccessor()
        {
            var first = new VersionNode();
            var successor = first.GetSuccessor();
            var anotherSuccessor = first.GetSuccessor();

            Assert.IsTrue(first.CompareTo(successor) < 0);
            Assert.IsTrue(first.CompareTo(anotherSuccessor) < 0);
            Assert.IsTrue(anotherSuccessor.CompareTo(successor) < 0);
        }

        [Test]
        public void TestFromList()
        {
            VersionNode[] array = { new VersionNode(), new VersionNode(), new VersionNode(), new VersionNode(), new VersionNode() };

            VersionNode.FromList(null, array, 0);

            for(int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                    Assert.Greater(array[j].CompareTo(array[i]), 0);
            }
        }
    }
}