using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class LogorithmicTransformation : LinearTransformation
    {
        public LogorithmicTransformation(double a, double b, double LastX)
            : base(a, b, LastX)
        {
        }

        public override double ValueAt(double X)
        {
            return (a * Math.Log(X)) + b;
        }
    }
}
