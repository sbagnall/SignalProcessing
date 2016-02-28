using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;

namespace SteveBagnall.Trading.SignalProcessing.LinearApproximations
{
    public class Logarithmic : LinearBase
    {
        public override double GetX(double OriginalX)
        {
            return Math.Log(OriginalX);
        }

        public override double GetY(double OriginalY)
        {
            return OriginalY;
        }

        public override double GetRegression(double a, double b, double x)
        {
            return (a * Math.Log(x)) + b;
        }

        public override TransformationBase GetTransform(double a, double b, double LastX)
        {
            return new LogorithmicTransformation(a, b, LastX);
        }
    }
}
