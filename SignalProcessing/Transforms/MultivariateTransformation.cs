using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;

namespace SteveBagnall.Trading.SignalProcessing.Transforms
{
    public class HighLowTransformation : ITransformation
    {
        private double[] _parameters;
        private double _yestHigh;
        private double _yestLow;
        private double _lastHigh;
        private double _lastLow;

        public HighLowTransformation(
            double YesterdaysHigh,
            double YesterdaysLow,
            double LastHigh,
            double LastLow,
            double[] Parameters)
        {
            _yestHigh = YesterdaysHigh;
            _yestLow = YesterdaysLow;
            _lastHigh = LastHigh;
            _lastLow = LastLow;
            _parameters = Parameters;
        }

        public void Undo(UndoOptions DaysAhead, ref double Target)
        {
            double regression;

            switch (DaysAhead)
            {
                case UndoOptions.SimpleUndo:
                    regression = (_parameters[0] * _yestHigh) + (_parameters[1] * _yestLow) + _parameters[2];
                    break;

                case UndoOptions.Prediction:
                default:
                    regression = (_parameters[0] * _lastHigh) + (_parameters[1] * _lastLow) + _parameters[2];
                    break;
            }

            Target += regression;
        }

        public double ValueAt(double X)
        {
            throw new NotImplementedException();
        }
    }
}
