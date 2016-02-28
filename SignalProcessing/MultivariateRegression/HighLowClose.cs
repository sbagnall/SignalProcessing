using Meta.Numerics.Statistics;
using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing.MultivariateRegression
{
    public class HighLowClose : ITransformStrategy
    {
        List<double> _highs = null;
        public List<double> Highs
        {
            get { return _highs; }
        }

        List<double> _lows = null;
        public List<double> Lows
        {
            get { return _lows; }
        }

        List<double> _closes = null;
        public List<double> Closes
        {
            get { return _closes; }
        }

        public HighLowClose(List<double> Highs, List<double> Lows, List<double> Closes)
        {
            _highs = Highs;
            _lows = Lows;
            _closes = Closes;
        }

        public List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            double yestHigh = 0.0;
            double yestLow = 0.0;
            double lastHigh = 0.0;
            double lastLow = 0.0;

            MultivariateSample mvS = new MultivariateSample(3);
            for (int i = 0; i < Data.Count; i++)
            {
                yestHigh = lastHigh;
                yestLow = lastLow;
                lastHigh = Highs[i];
                lastLow = Lows[i];

                if (i > 0)
                    mvS.Add(yestHigh, yestLow, Closes[i]);
            }

            List<double> detrendedData = (List<double>)Utilities.DeepClone(Data);

            double[] parameters = mvS.LinearRegression(2).Parameters();

            for (int i = 0; i < Data.Count; i++)
            {
                double regression;

                if (i > 0)
                {
                    regression =
                        (parameters[0] * Highs[i - 1])
                        + (parameters[1] * Lows[i - 1])
                        + parameters[2];
                }
                else
                {
                    regression = Closes[i];
                }

                detrendedData[i] -= regression;
            }

            Transforms.Push(new HighLowTransformation(yestHigh, yestLow, lastHigh, lastLow, parameters));

            return detrendedData;
        }
    }
}
