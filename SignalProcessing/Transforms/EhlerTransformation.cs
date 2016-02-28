using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;
using System.Linq;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class EhlerTransformation : TransformationBase
    {
        private double[] _values = null;

        public EhlerTransformation(double[] Values, int LastX) : base(LastX)
        {
            _values = Values;
        }

        public override double ValueAt(double X)
        {
            if (X >= _values.Length)
                throw new ApplicationException("No predicted value at that index.");

            return _values.ElementAt((int)X);
        }
    }
}
