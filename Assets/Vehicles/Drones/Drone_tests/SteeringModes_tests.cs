using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GyroscopeTests
    {
        [Test]
        public void test_Angle2OMO_0()
        {
            float result = Gyroscope.Angle2OneMinusOne(0f);
            Assert.AreEqual(result, 0f);
        }
        [Test]
        public void test_Angle2OMO_179()
        {
            float result = Gyroscope.Angle2OneMinusOne(179f);
            Assert.AreEqual(result, 0.994444489f);
        }
        [Test]
        public void test_Angle2OMO_181()
        {
            float result = Gyroscope.Angle2OneMinusOne(181f);
            Assert.AreEqual(result, -0.99444437f);
        }
        [Test]
        public void test_Angle2OMO_359()
        {
            float result = Gyroscope.Angle2OneMinusOne(359f);
            Assert.AreEqual(result, -0.00555562973f);
        }
    }
}
