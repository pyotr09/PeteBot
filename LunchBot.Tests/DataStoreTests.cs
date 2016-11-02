using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchBot.Tests
{
    [TestClass]
    public class NomiationTests
    {
        private static DataStore DataStore { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            DataStore = new DataStore();
        }

        [TestMethod]
        public void Nominate()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
        }

        [TestMethod]
        public void Second()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            DataStore.Second("El Senor Sol", "Kip");
            Assert.IsTrue(DataStore.IsSeconded("El Senor Sol"));
        }

        [TestMethod]
        public void SecondingNewOptionNominates()
        {
            DataStore.Second("El Senor Sol", "Kip");
            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
            Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
        }

        [TestMethod]
        public void NominatingTwiceSeconds()
        {
            DataStore.Nominate("El Senor Sol", "Kip");
            DataStore.Nominate("El Senor Sol", "John");
            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
            Assert.IsTrue(DataStore.IsSeconded("El Senor Sol"));
        }

        [TestMethod]
        public void Veto()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            DataStore.Second("El Senor Sol", "Charles");
            DataStore.Veto("El Senor Sol", "John");
            Assert.IsTrue(DataStore.IsVetoed("El Senor Sol"));
            Assert.IsFalse(DataStore.IsNominated("El Senor Sol"));
            Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
            Assert.IsFalse(DataStore.CanVeto("John"));
        }

        [TestMethod]
        public void NoVetosRemaining()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            DataStore.Veto("El Senor Sol", "John");
            DataStore.Nominate("Jose Oshea's", "Peter");
            DataStore.Second("Jose Oshea's", "Kip");
            DataStore.Veto("Jose Oshea's", "John");
            Assert.IsFalse(DataStore.IsVetoed("Jose Oshea's"));
        }

        [TestMethod]
        public void Status()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            Assert.AreEqual("nominated", DataStore.Status("El Senor Sol"));
            DataStore.Second("El Senor Sol", "John");
            Assert.AreEqual("seconded", DataStore.Status("El Senor Sol"));
            DataStore.Veto("El Senor Sol", "Kip");
            Assert.AreEqual("vetoed", DataStore.Status("El Senor Sol"));
        }

        [TestMethod]
        public void CantNominateAVeto()
        {
            DataStore.Veto("El Senor Sol", "Peter");
            DataStore.Nominate("El Senor Sol", "John");
            Assert.IsTrue(DataStore.IsVetoed("El Senor Sol"));
			Assert.IsFalse(DataStore.IsNominated("El Senor Sol"));
			Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
		}

		[TestMethod]
		public void CantSecondAVeto()
		{
			DataStore.Veto("El Senor Sol", "Peter");
			DataStore.Second("El Senor Sol", "John");
			Assert.IsTrue(DataStore.IsVetoed("El Senor Sol"));
			Assert.IsFalse(DataStore.IsNominated("El Senor Sol"));
			Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
		}

		[TestMethod]
        public void Remove()
        {
            DataStore.Nominate("El Senor Sol", "Kip");
            DataStore.Second("El Senor Sol", "John");
            DataStore.Remove("El Senor Sol");
            Assert.IsFalse(DataStore.GetNominations().Contains("El Senor Sol"));
            Assert.IsFalse(DataStore.GetSeconds().Contains("El Senor Sol"));
        }

        [TestMethod]
        public void SecondTwice()
        {
            DataStore.Nominate("El Senor Sol", "Peter");
            DataStore.Second("El Senor Sol", "Kip");
            DataStore.Second("El Senor Sol", "John");

            Assert.AreEqual(1, DataStore.GetNominations().Count);
            Assert.AreEqual(1, DataStore.GetSeconds().Count);
        }

        [TestMethod]
        public void UseTitleCase()
        {
            DataStore.Nominate("el Senor sol", "Peter");
            DataStore.Second("el seNOr Sol", "Kip");

            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
            Assert.IsTrue(DataStore.IsSeconded("El Senor Sol"));
        }
    }
}
