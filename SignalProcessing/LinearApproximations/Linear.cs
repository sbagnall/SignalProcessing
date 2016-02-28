﻿using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;

namespace SteveBagnall.Trading.SignalProcessing.LinearApproximations
{
    public class Linear : LinearBase
    {
        public override double GetX(double OriginalX)
        {
            return OriginalX;
        }

        public override double GetY(double OriginalY)
        {
            return OriginalY;
        }

        public override double GetRegression(double a, double b, double x)
        {
            return (a * x) + b;
        }

        public override TransformationBase GetTransform(double a, double b, double LastX)
        {
            return new LinearTransformation(a, b, LastX);
        }
    }
}
