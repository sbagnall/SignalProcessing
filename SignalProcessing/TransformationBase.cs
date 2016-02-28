using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;

namespace SteveBagnall.Trading.SignalProcessing
{
    public abstract class TransformationBase : ITransformation
    {
        public abstract double ValueAt(double X);

        private double _lastX;
        public double LastX
        {
            get { return _lastX; }
        }

        public TransformationBase(double LastX)
        {
            _lastX = LastX;
        }

        public void Undo(UndoOptions DaysAhead, ref double Target)
        {
            double x = LastX + (int)DaysAhead;
            Target += ValueAt(x);
        }

        public decimal ValueAt(decimal X)
        {
            throw new NotImplementedException();
        }
    }
}
