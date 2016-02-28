using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteveBagnall.Trading.Scoring;
using SteveBagnall.Trading.Shared.ConversionStrategies;
using SteveBagnall.Trading.SignalProcessing;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.LinearApproximations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalProcessing.Tests
{
    [TestClass]
    public class TestEhler
    {
        const double ALPHA = 0.07; // measure of sensitivity (higher, more sensitive)

        const double SINE_WAVE_DELAY = 11.0;
        const double CYCLE_DELAY = 10.0;
        static int AddForSineWave = (int)Math.Ceiling(SINE_WAVE_DELAY);
        static int AddForCycle = (int)Math.Ceiling(CYCLE_DELAY);

        const int NUM_PREDICTION_DAYS = 1;

        const bool isRecursive = true;
        const int minLength = 100;

        static IScoringStrategy scoring = new LowestSquareStrategy(new ClosePriceStrategy());

        static double RANGE = 100;
        // TODO: this is not very good - check
        static double TOLERANCE = RANGE * 0.3;

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

        private static double[] Trend;
        private static List<double> TrendingSignal;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Trend = GetTrend();
            TrendingSignal = Trend.Zip(GetSignal(), (x, y) => x + y).ToList();
        }
        
    
        [TestMethod]
        public void test_ehler_transform_decycling()
        {
            Stack<ITransformation> transforms = null;

            Ehler ehler = new Ehler(
                ALPHA, 
                AddForSineWave, 
                AddForCycle, 
                NUM_PREDICTION_DAYS, 
                isRecursive, 
                minLength, 
                scoring,
                scoring);

            List<double> actual = ehler.Remove(TrendingSignal, ref transforms);

            var rms = Math.Sqrt(Trend.Zip(actual, (x, y) => Math.Pow((x - y), 2)).Average());

            Assert.IsTrue(rms <= TOLERANCE);
        }

        [TestMethod]
        public void test_ehler_transform_transformation()
        {
            Stack<ITransformation> transforms = null;

            Ehler ehler = new Ehler(
                ALPHA,
                AddForSineWave,
                AddForCycle,
                NUM_PREDICTION_DAYS,
                isRecursive,
                minLength,
                scoring,
                scoring);

            List<double> actual = ehler.Remove(TrendingSignal, ref transforms);

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
