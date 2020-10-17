using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace QCS
{
    [Obsolete]
    public struct GateIdComponent : IComponentData
    {
        public int Value;
    }

    public struct SingleQubitGate : IComponentData
    {
        /// <summary>
        /// GateCode is an opcode of a gate, see the list //TODO:
        /// QubitIds is the array of qubits a gate should be applied to
        /// </summary>
        public int GateCode;
        public int Qubit;
    }

    public struct DoubleQubitGate : IComponentData
    {
        public int GateCode;
        public int ControlQubit;
        public int TargetQubit;
    }

    public struct TripleQubitGate : IComponentData
    {
        public int GateCode;
        public int C1Qubit;
        public int C2Qubit;
        public int TargetQubit;
    }
}