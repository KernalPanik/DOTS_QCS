using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace QCS
{
    [Obsolete]
    public struct Energy : IComponentData
    {
        public float Value;
    }

    public struct QuantumState : IComponentData
    {
        /// <summary>
        /// Alpha amplitude describes state |0>
        /// Beta amplitude describes state |1>
        /// </summary>
        public float Alpha;
        public float Beta;

        // If 'Locked' is set to 1, it means that qubit was measured, and it's state should be saved
        public byte Locked; 
    }

    public struct QubitComponent : IComponentData
    {
        public int Id;
    }

    public struct NoiseComponent : IComponentData
    {
    }

    public struct ErrorCorrectionComponent : IComponentData
    {

    }
}