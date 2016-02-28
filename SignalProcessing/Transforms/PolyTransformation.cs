using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class PolyTransformation : TransformationBase
    {
        private double[] _parameters;

        public PolyTransformation(double LastX, double[] Parameters)
            : base(LastX)
        {
            _parameters = Parameters;
        }


        public override double ValueAt(double X)
        {
            return _parameters[0]
                + (_parameters[1] * X)
                + ((_parameters.Length > 2) ? _parameters[2] * Math.Pow(X, 2) : 0.0)
                + ((_parameters.Length > 3) ? _parameters[3] * Math.Pow(X, 3) : 0.0)
                + ((_parameters.Length > 4) ? _parameters[4] * Math.Pow(X, 4) : 0.0);
        }
    }
}
