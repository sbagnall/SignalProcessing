using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class WaveTransformation : TransformationBase
    {
        private CompositeWave _wave;

        public WaveTransformation(CompositeWave Wave, double LastX) : base(LastX)
        {
            _wave = Wave;
        }

        public override double ValueAt(double X)
        {
            return _wave.ValueAt(X);
        }
    }
}
