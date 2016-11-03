using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchBot.Tests
{
    [TestClass]
    public class VotingTests
    {
        [TestMethod]
        public void CastOneVote()
        {
            var ballot = new Ballot("Jimmy Johns", "Tin Star", "Noodles", "El Senor Sol");
            ballot.Cast("Peter", "El Senor Sol, Tin Star, Noodles, Jimmy Johns");
            Assert.AreEqual("El Senor Sol", ballot.GetOrderedResults()[0].Text);
            Assert.AreEqual(4, ballot.GetOrderedResults()[0].Value);
            Assert.AreEqual(3, ballot.GetOrderedResults()[1].Value);
            Assert.AreEqual(2, ballot.GetOrderedResults()[2].Value);
            Assert.AreEqual(1, ballot.GetOrderedResults()[3].Value);
        }

        [TestMethod]
        public void CastTwoVotes()
        {
            var ballot = new Ballot("Jimmy Johns", "Tin Star", "Noodles", "El Senor Sol");
            ballot.Cast("Peter", "El Senor Sol, Tin Star, Noodles, Jimmy Johns");
            ballot.Cast("John", "Noodles, El Senor Sol, Jimmy Johns, Tin Star");
            Assert.AreEqual("El Senor Sol", ballot.GetOrderedResults()[0].Text);
            Assert.AreEqual(7, ballot.GetOrderedResults()[0].Value);
            Assert.AreEqual(6, ballot.GetOrderedResults()[1].Value);
            Assert.AreEqual(4, ballot.GetOrderedResults()[2].Value);
            Assert.AreEqual(3, ballot.GetOrderedResults()[3].Value);
        }

        [TestMethod]
        public void RecastVote()
        {
            var ballot = new Ballot("Jimmy Johns", "Tin Star", "Noodles", "El Senor Sol");
            ballot.Cast("Peter", "Tin Star, El Senor Sol, Noodles, Jimmy Johns");
            ballot.Cast("Peter", "El Senor Sol, Tin Star, Noodles, Jimmy Johns");
            ballot.Cast("John", "Noodles, El Senor Sol, Jimmy Johns, Tin Star");
            Assert.AreEqual("El Senor Sol", ballot.GetOrderedResults()[0].Text);
            Assert.AreEqual(7, ballot.GetOrderedResults()[0].Value);
            Assert.AreEqual(6, ballot.GetOrderedResults()[1].Value);
            Assert.AreEqual(4, ballot.GetOrderedResults()[2].Value);
            Assert.AreEqual(3, ballot.GetOrderedResults()[3].Value);
        }

        [TestMethod]
        public void CastNumberedVotes()
        {
            var ballot = new Ballot("Jimmy Johns", "Tin Star", "Noodles", "El Senor Sol");
            ballot.Cast("Peter", "4, 2, 3, 1");
            ballot.Cast("John", "3, 4, 1, 2");
            Assert.AreEqual("El Senor Sol", ballot.GetOrderedResults()[0].Text);
            Assert.AreEqual(7, ballot.GetOrderedResults()[0].Value);
            Assert.AreEqual(6, ballot.GetOrderedResults()[1].Value);
            Assert.AreEqual(4, ballot.GetOrderedResults()[2].Value);
            Assert.AreEqual(3, ballot.GetOrderedResults()[3].Value);
        }
    }
}
