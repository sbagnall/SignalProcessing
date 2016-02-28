using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;

namespace SteveBagnall.Trading.SignalProcessing.LinearApproximations
{
    public class Power : LinearBase
    {
        public override double GetX(double OriginalX)
        {
            return Math.Log(OriginalX);
        }

        public override double GetY(double OriginalY)
        {
            return Math.Log(OriginalY);
        }

        public override double GetRegression(double a, double b, double x)
        {
            return b * Math.Pow(x, a);
        }

        public override TransformationBase GetTransform(double a, double b, double LastX)
        {
            return new PowerTransformation(a, b, LastX);
        }
    }
}
