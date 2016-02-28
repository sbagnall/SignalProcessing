using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class LinearTransformation : TransformationBase
    {
        private double _a;
        public double a
        {
            get { return _a; }
            set { _a = value; }
        }

        private double _b;
        public double b
        {
            get { return _b; }
            set { _b = value; }
        }

        public LinearTransformation(double a, double b, double LastX)
            : base(LastX)
        {
            _a = a;
            _b = b;
        }

        public override double ValueAt(double X)
        {
            return b + (a * X);
        }
    }
}
