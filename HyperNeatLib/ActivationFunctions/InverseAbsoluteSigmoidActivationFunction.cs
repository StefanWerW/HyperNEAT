﻿using System;
using System.ComponentModel.Composition;

using HyperNeatLib.Interfaces;

namespace HyperNeatLib.ActivationFunctions
{
    [Export(typeof(IActivationFunction))]
    public class InverseAbsoluteSigmoidActivationFunction : IActivationFunction
    {
        public object Clone()
        {
            return new PlainSigmoidActivationFunction();
        }

        public double Calc(double input)
        {
            return 0.5 + (input / (2.0 * (0.2 + Math.Abs(input))));
        }

        public bool AcceptsAuxValues => false;

        public double[] AuxValues { get; set; }

        public void RandomizeAuxValues()
        {
            throw new NotImplementedException();
        }

        public void MutateAuxValues(Random random)
        {
            throw new NotImplementedException();
        }
    }
}