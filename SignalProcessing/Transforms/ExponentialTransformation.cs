using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class ExponentialTransformation : LinearTransformation
    {
        public ExponentialTransformation(double a, double b, double LastX)
            : base(a, b, LastX)
        {
        }

        public override double ValueAt(double X)
        {
            return b * Math.Exp(a * X);
        }
    }
}
