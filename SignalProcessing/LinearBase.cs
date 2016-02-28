using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.LinearApproximations;
using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing
{
    public abstract class LinearBase : ITrendRemover
    {
        public abstract double GetX(double OriginalX);

        public abstract double GetY(double OriginalY);

        public abstract double GetRegression(double a, double b, double x);

        public abstract TransformationBase GetTransform(double a, double b, double LastX);

        public List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            if (Transforms == null)
            {
                Transforms = new Stack<ITransformation>();
            }

            double[] y = new double[Data.Count];
            double[,] x = new double[2, Data.Count];
            double[] w = new double[Data.Count];

            for (int j = 0; j < Data.Count; j++)
            {
                y[j] = GetY(Data[j]);
                x[0, j] = 1;    // constant term
                x[1, j] = GetX(j + 1);
                w[j] = 1.0;
            }

            double a = Double.MinValue;
            double b = Double.MinValue;

            LinearRegression lr = new LinearRegression();
            if (lr.Regress(y, x, w))
            {
                a = lr.Coefficients[1];
                b = lr.Coefficients[0];
            }

            List<double> detrendedData = (List<double>)Utilities.DeepClone(Data);

            double lastX = 0.0;
            for (int i = 0; i < Data.Count; i++)
            {
                double regression = GetRegression(a, b, (i + 1));
                detrendedData[i] -= regression;
                lastX = (i + 1);
            }

            Transforms.Push(GetTransform(a, b, lastX));

            return detrendedData;
        }


    }
}
