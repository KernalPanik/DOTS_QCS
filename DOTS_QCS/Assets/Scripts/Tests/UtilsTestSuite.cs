﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using QCS;
using Unity.Mathematics;

namespace Tests
{
    public class UtilsTestSuite
    {
        [Test]
        public void TensorProductTest()
        {
            float2 a = new float2(3f, 5f);
            float2 b = new float2(2f, 3f);

            float4 expectedResult = new float4(6f, 9f, 10f, 15f);
            float4 result = ExtraMath.TensorProduct2x2(a, b);

            Assert.AreEqual(expectedResult, result);
        }
    }
}