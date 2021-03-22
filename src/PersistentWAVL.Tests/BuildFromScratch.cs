using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL.Tests
{
    class BuildFromScratch
    {
        [Test]
        public void TestBuild() { 
            
            // Set up
            
            var t0 = Tree<IntClass, int>.GetNew;
            var t1 = t0.Insert(100, 100);
            var t2 = t1.Insert(200, 200);
            var t3 = t2.Insert(0, 0);
            var t1point5 = t1.Insert(0, -1);

            // Check tree structure

            var t1root = t1.Root;
            Assert.IsNotNull(t1root);
            Assert.IsNull(t1root.Left);
            Assert.IsNull(t1root.Right);

            var t2root = t2.Root;
            Assert.IsNotNull(t2root);
            Assert.IsNull(t2root.Left);
            Assert.IsNotNull(t2root.Right);

            var t3root = t3.Root;
            Assert.IsNotNull(t3root);
            Assert.IsNotNull(t3root.Left);
            Assert.IsNotNull(t3root.Right);

            var t1point5root = t1point5.Root;
            Assert.IsNotNull(t1point5root);
            Assert.IsNotNull(t1point5root.Left);
            Assert.IsNull(t1point5root.Right);
        }
    }
}
