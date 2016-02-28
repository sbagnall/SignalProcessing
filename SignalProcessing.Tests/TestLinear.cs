using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.LinearApproximations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalProcessing.Tests
{
    [TestClass]
    public class TestLinear
    {
        static double RANGE = 100;
        static double TOLERANCE = RANGE * 0.05;

        private static double[] GetTrend()
        {
            double[] trend = new double[100];

            for (int i = 0; i < 100; i += 1)
            {
                trend[i] = i * RANGE / 100;
            }

            return trend;
        }

        private static double[] GetSignal()
        {
            double[] signal = new double[100];

            for (int i =0; i < 100; i += 1)
            {
                signal[i] = Math.Sin(i * 0.1) * RANGE / 5;
            }

            return signal;
        }

        private static double[] Signal;
        private static List<double> TrendingSignal;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Signal = GetSignal();
            TrendingSignal = GetTrend().Zip(Signal, (x, y) => x + y).ToList();
        }
        
    
        [TestMethod]
        public void test_lnear_transform_detrending()
        {
            Stack<ITransformation> transforms = null;

            List<double> actual = new Linear().Remove(TrendingSignal, ref transforms);

            var rms = Math.Sqrt(Signal.Zip(actual, (x, y) => Math.Pow((x - y), 2)).Average());

            Assert.IsTrue(rms <= TOLERANCE);
        }

        [TestMethod]
        public void test_lnear_transform_transformation()
        {
            Stack<ITransformation> transforms = null;

            var actual = new Linear().Remove(TrendingSignal, ref transforms);

            var transformation = transforms.Pop();

            double[] retrended = new double[100];

            for (int i = 0; i < actual.Count; i += 1)
            {
                retrended[i] = actual[i] + transformation.ValueAt(i);
            }

            var rms = Math.Sqrt(TrendingSignal.Zip(retrended, (x, y) => Math.Pow((x - y), 2)).Average());

            Assert.IsTrue(rms <= TOLERANCE);
        }
    }
}
