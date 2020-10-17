﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace QCS
{
    public struct SphericalCoords
    {
        public float Theta;
        public float Phi;
    }

    public static class ExtraMath
    {
        /// <summary>
        /// A method that converts a given quaternion into spherical coordinates (theta & phi) by 
        /// calculating a direction vector and translating it's coordinates into theta and phi //TODO: rework doc
        /// </summary>
        public static SphericalCoords QuaternionToSpherical(quaternion quat)
        {
            float3 v = math.normalize(math.mul(quat, UnityEngine.Vector3.up));
            SphericalCoords coords = new SphericalCoords();

            coords.Phi = math.atan(v.x / v.z); // lon
            coords.Theta = math.acos(v.y / 1); // lat

            return coords;
        }


        public static float3 ToEulerAngles(quaternion q)
        {
            float3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
            double cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
            angles.x = (float)math.atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
            if (math.abs(sinp) >= 1)
                angles.y = (float)CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.y = (float)math.asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
            double cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
            angles.z = (float)math.atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        private static double CopySign(double a, double b)
        {
            return math.abs(a) * math.sign(b);
        }

        public static float4 TensorProduct2x2(float2 a, float2 b)
        {
            float2 r1 = a.x * b;
            float2 r2 = a.y * b;
            float4 result = new float4(r1.x, r1.y, r2.x, r2.y);
            return result;
        }

        public static float GenerateStates(int qubitCount)
        {
            return math.pow(2, qubitCount);
        }

        public static int PickValueFromAmplitudes(float[] stateAmplitudes, int[] states)
        {
            // Find non zero states
            Dictionary<int, float> nonZeroStatePairs = new Dictionary<int, float>();
            for (int i = 0; i < states.Length; i++)
            {
                if (stateAmplitudes[i] != 0f)
                {
                    nonZeroStatePairs.Add(states[i], math.abs(math.pow(stateAmplitudes[i], 2)));
                }
            }

            // Order non zero states by amplitudes
            float largestState = 0f;
            List<KeyValuePair<int, float>> stateAmpPairs = nonZeroStatePairs.ToList();
            stateAmpPairs.Sort(
                delegate (KeyValuePair<int, float> p1,
                KeyValuePair<int, float> p2)
                {
                    int order = p1.Value.CompareTo(p2.Value);
                    if (order > 0)
                        largestState = p1.Value;
                    else
                        largestState = p2.Value;
                    return order;
                });

            // Generate random number between 0, 99
            System.Random rand = new System.Random();
            var randomNum = (rand.NextDouble() * 99f) / 100f;

            // Pick a state based on random number
            int pickedState = 0;
            float tempAmp = 0f;
            foreach (var stateAmpPair in stateAmpPairs)
            {
                if (tempAmp < randomNum && randomNum < stateAmpPair.Value)
                {
                    pickedState = stateAmpPair.Key;
                    break;
                }
                else if (randomNum > largestState)
                {
                    pickedState = stateAmpPair.Key;
                    break;
                }
                else
                {
                    tempAmp = stateAmpPair.Value;
                }
            }

            return pickedState;
        }
    }
}