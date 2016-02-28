using Meta.Numerics.Statistics;
using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;
using System.Collections.Generic;


namespace SteveBagnall.Trading.SignalProcessing.TrendRemovers
{
    public class Poly : ITrendRemover
    {
        private const double OBSERVED_ERROR = 0.0000001;

        private int _order = 0;
        public int Order
        {
            get { return _order; }
        }

        public Poly(int Order)
        {
            _order = Order;
        }

        public List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            DataSet d = new DataSet();
            for (int i = 0; i < Data.Count; i++)
                d.Add((i + 1), Data[i], OBSERVED_ERROR);

            double[] parameters = d.FitToPolynomial((int)this.Order).Parameters();

            List<double> detrendedData = (List<double>)Utilities.DeepClone(Data);

            double lastX = 0.0;
            for (int i = 0; i < Data.Count; i++)
            {
                double regression = parameters[0]
                    + (parameters[1] * (i + 1))
                    + ((this.Order == 2) ? ((parameters.Length > 2) ? parameters[2] * Math.Pow(i + 1, 2) : 0.0) : 0.0)
                    + ((this.Order == 3) ? ((parameters.Length > 3) ? parameters[3] * Math.Pow(i + 1, 3) : 0.0) : 0.0)
                    + ((this.Order == 4) ? ((parameters.Length > 4) ? parameters[4] * Math.Pow(i + 1, 4) : 0.0) : 0.0);

                detrendedData[i] -= regression;
                lastX = (i + 1);
            }

            Transforms.Push(new PolyTransformation(lastX, parameters));

            return detrendedData;
        }
    }
}
