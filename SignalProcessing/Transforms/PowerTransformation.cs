using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class PowerTransformation : LinearTransformation
    {
        public PowerTransformation(double a, double b, double LastX)
            : base(a, b, LastX)
        {
        }

        public override double ValueAt(double X)
        {
            return b * Math.Pow(X, a);
        }
    }
}
