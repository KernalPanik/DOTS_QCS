using System.Collections;
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

        [Test]
        public void RandomNumTest()
        {
            float[] stateAmps = { 0.5916079783099616f, 0.806225774829855f };
            int[] states = { 0b1001, 0b1111 };

            var val = ExtraMath.PickValueFromAmplitudes(stateAmps, states);
        }

        [Test]
        public void MatrixMulTest()
        {
            float[,] matrix = new float[2, 2] { {7,2}, {0,3}};
            float[] vector = new float[2] {4, 1};

            var result = ExtraMath.Matrix2x2VectorMultiplication(matrix, vector);
            if(result[0] == 30 && result[1] == 3)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
