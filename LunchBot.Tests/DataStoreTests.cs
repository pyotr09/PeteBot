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
            DataStore.AddRequest("El Senor Sol", "Peter");
            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
        }

        [TestMethod]
        public void Second()
        {
            DataStore.AddRequest("El Senor Sol", "Peter");
            DataStore.AddRequest("El Senor Sol", "Kip");
            Assert.IsTrue(DataStore.IsSeconded("El Senor Sol"));
        }

        [TestMethod]
        public void Veto()
        {
            DataStore.AddRequest("El Senor Sol", "Peter");
            DataStore.AddRequest("El Senor Sol", "Charles");
            DataStore.Veto("El Senor Sol", "John");
            Assert.IsTrue(DataStore.IsVetoed("El Senor Sol"));
            Assert.IsFalse(DataStore.IsNominated("El Senor Sol"));
            Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
            Assert.IsFalse(DataStore.CanVeto("John"));
        }

        [TestMethod]
        public void NoVetosRemaining()
        {
            DataStore.AddRequest("El Senor Sol", "Peter");
            DataStore.Veto("El Senor Sol", "John");
            DataStore.AddRequest("Jose Oshea's", "Peter");
            DataStore.AddRequest("Jose Oshea's", "Kip");
            DataStore.Veto("Jose Oshea's", "John");
            Assert.IsFalse(DataStore.IsVetoed("Jose Oshea's"));
        }

        [TestMethod]
        public void Status()
        {
            DataStore.AddRequest("El Senor Sol", "Peter");
            Assert.AreEqual("nominated", DataStore.Status("El Senor Sol"));
            DataStore.AddRequest("El Senor Sol", "John");
            Assert.AreEqual("seconded", DataStore.Status("El Senor Sol"));
            DataStore.Veto("El Senor Sol", "Kip");
            Assert.AreEqual("vetoed", DataStore.Status("El Senor Sol"));
        }

        [TestMethod]
        public void CantNominateAVeto()
        {
            DataStore.Veto("El Senor Sol", "Peter");
            DataStore.AddRequest("El Senor Sol", "John");
            Assert.IsTrue(DataStore.IsVetoed("El Senor Sol"));
			Assert.IsFalse(DataStore.IsNominated("El Senor Sol"));
			Assert.IsFalse(DataStore.IsSeconded("El Senor Sol"));
		}

		[TestMethod]
        public void Remove()
        {
            DataStore.MakeAdmin("Peter");
            DataStore.AddRequest("El Senor Sol", "Kip");
            DataStore.AddRequest("El Senor Sol", "John");
            DataStore.Remove("El Senor Sol", "Peter");
            Assert.IsFalse(DataStore.GetNominations().Contains("El Senor Sol"));
            Assert.IsFalse(DataStore.GetSeconds().Contains("El Senor Sol"));
        }

		[TestMethod]
        public void RemoveClearsVeto()
        {
            DataStore.MakeAdmin("Peter");
            DataStore.Veto("Tin Star", "John");
            DataStore.Remove("Tin Star", "Peter");
            DataStore.AddRequest("Tin Star", "Peter");
            Assert.IsTrue(DataStore.GetNominations().Contains("Tin Star"));
        }

		[TestMethod]
        public void RemoveMustBeAdmin()
        {
            DataStore.AddRequest("Tin Star", "John");
            Assert.IsFalse(DataStore.Remove("Tin Star", "Peter"));
		    DataStore.MakeAdmin("Peter");
            DataStore.Remove("Tin Star", "Peter");
            Assert.IsTrue(DataStore.Remove("Tin Star", "Peter"));
        }

        [TestMethod]
        public void SecondTwice()
        {
            DataStore.AddRequest("El Senor Sol", "Peter");
            DataStore.AddRequest("El Senor Sol", "Kip");
            DataStore.AddRequest("El Senor Sol", "John");

            Assert.AreEqual(1, DataStore.GetNominations().Count);
            Assert.AreEqual(1, DataStore.GetSeconds().Count);
        }

        [TestMethod]
        public void UseTitleCase()
        {
            DataStore.AddRequest("el Senor sol", "Peter");
            DataStore.AddRequest("el seNOr Sol", "Kip");

            Assert.IsTrue(DataStore.IsNominated("El Senor Sol"));
            Assert.IsTrue(DataStore.IsSeconded("El Senor Sol"));
        }
    }
}
