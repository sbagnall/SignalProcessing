using SteveBagnall.Trading.Scoring;
using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteveBagnall.Trading.SignalProcessing
{
    public class Ehler : ITransformStrategy
    {
        private double _alpha;
        private int _maxSineWaveDelay;
        private int _maxCycleDelay;
        private int _numPredictionDays;
        private bool _isRecursive = false;
        private int _minLength;
        private IScoringStrategy _phaseShiftScoringStrategy = null;
        private IScoringStrategy _recursiveScoringStrategy = null;

        public Ehler(
            double Alpha,
            int MaxSineWaveDelay,
            int MaxCycleDelay,
            int NumPredictionDays,
            bool IsRecursive,
            int MinLength,
            IScoringStrategy PhaseShiftScoringStrategy,
            IScoringStrategy RecursiveScoringStrategy)
        {
            _alpha = Alpha;
            _maxSineWaveDelay = MaxSineWaveDelay;
            _maxCycleDelay = MaxCycleDelay;
            _numPredictionDays = NumPredictionDays;
            _isRecursive = IsRecursive;
            _minLength = MinLength;
            _phaseShiftScoringStrategy = PhaseShiftScoringStrategy;
            _recursiveScoringStrategy = RecursiveScoringStrategy;
        }

        public List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            if (Transforms == null)
            {
                Transforms = new Stack<ITransformation>();
            }

            double bestError = Double.MaxValue;
            List<double> detrendedData = null;

            List<double> data = (List<double>)Utilities.DeepClone(Data);

            do
            {
                detrendedData = RemoveBestLengthBestShiftedCycle(data, ref Transforms);

                if (detrendedData != null)
                {
                    _recursiveScoringStrategy.StartNewTransformation(data, Transforms.Peek());
                    _recursiveScoringStrategy.CurrentTransformedData = detrendedData;
                    _recursiveScoringStrategy.Score();

                    double error = _recursiveScoringStrategy.LowestError;

                    if (error < bestError)
                    {
                        bestError = error;
                    }
                    else
                    {
                        Transforms.Pop();
                        detrendedData = (List<double>)Utilities.DeepClone(data);
                        break;
                    }
                }

#if DEBUG
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("{0},{1}", "Original", "Ehler"));

                for (int i = 0; i < detrendedData.Count; i++)
                {
                    double d = Data[(Data.Count - detrendedData.Count) + i];
                    double dt = detrendedData[i];

                    sb.AppendLine(String.Format("{0},{1}", d, dt));
                }
#endif
                data = (List<double>)Utilities.DeepClone(detrendedData);
            }
            while (_isRecursive);

            return detrendedData;

        }

        private List<double> RemoveBestLengthBestShiftedCycle(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            double lowestError = Double.MaxValue;
            ITransformation bestTransform = null;
            List<double> bestDecycled = null;

            for (int i = Data.Count; i >= _minLength; i = (int)Math.Floor(i / 2.0))
            {
                ITransformation transform = null;
                double error = 0.0;

                List<double> decycled = RemoveBestPhaseShiftedCycle(Data.GetRange(Data.Count - i, i), out transform, out error);

                if (error < lowestError)
                {
                    lowestError = error;
                    bestTransform = transform;
                    bestDecycled = (List<double>)Utilities.DeepClone(decycled);
                }
            }

            Transforms.Push(bestTransform);

            return bestDecycled;
        }

        private List<double> RemoveBestPhaseShiftedCycle(List<double> Data, out ITransformation Transform, out double Error)
        {
            double[] cycle = GetCycle((Double[])Data.ToArray());

            for (int phase = (_maxCycleDelay * -1); phase < _maxCycleDelay; phase++)
            {
                List<double> decycledData = new List<double>();

                double lastX = 0.0;
                for (int i = 0; i < Data.Count; i++)
                {
                    if (((i + phase) < cycle.Length) && ((i + phase) >= 0))
                    {
                        decycledData.Add(Data[i] - cycle[i + phase]);
                        lastX = i + phase;
                    }

                }

                if (decycledData.Count < _minLength)
                    continue;

                _phaseShiftScoringStrategy.StartNewTransformation(Data, new EhlerTransformation(cycle, (int)lastX));
                int decycleIndex = 0;

                for (int i = 0; i < Data.Count; i++)
                    if (((i + phase) < cycle.Length) && ((i + phase) >= 0))
                        _phaseShiftScoringStrategy.AddTransformedValue(decycledData[decycleIndex++]);

                _phaseShiftScoringStrategy.Score();
            }

            Error = _phaseShiftScoringStrategy.LowestError;
            Transform = _phaseShiftScoringStrategy.BestTransform;

            return _phaseShiftScoringStrategy.BestTransformedData;
        }

        /// <summary>
        /// 1.5 bar lag
        /// </summary>
        /// <param name="Prices"></param>
        /// <returns></returns>
        private double[] GetCycle(double[] Prices)
        {
            double[] aSmoothArray = new double[Prices.Length + _maxSineWaveDelay + _numPredictionDays];
            double[] aCycleArray = new double[Prices.Length + _maxSineWaveDelay + _numPredictionDays];

            for (int i = 0; i < (Prices.Length + _maxSineWaveDelay + _numPredictionDays); i++)
            {
                //aSmoothArray[i] = (((i < Prices.Length) ? Prices[i] : 0.0) + (2 * (((i >= 1) && ((i - 1) < Prices.Length)) ? Prices[i - 1] : 0)) + (2 * (((i >= 2) && ((i - 2) < Prices.Length)) ? Prices[i - 2] : 0)) + (((i >= 3) && ((i - 3) < Prices.Length)) ? Prices[i - 3] : 0)) / 6.0;

                double smooth = 0.0;
                int smoothCount = 0;

                // ((i < Prices.Length) ? Prices[i] : 0.0)
                if (i < Prices.Length)
                {
                    smooth += Prices[i];
                    smoothCount++;
                }

                // (2 * (((i >= 1) && ((i - 1) < Prices.Length)) ? Prices[i - 1] : 0))
                if ((i >= 1) && ((i - 1) < Prices.Length))
                {
                    smooth += (2 * Prices[i - 1]);
                    smoothCount += 2;
                }
                else if ((i - 1) >= Prices.Length)
                {
                    smooth += (2 * aSmoothArray[i - 1]);
                    smoothCount += 2;
                }

                // (2 * (((i >= 2) && ((i - 2) < Prices.Length)) ? Prices[i - 2] : 0)) 
                if ((i >= 2) && ((i - 2) < Prices.Length))
                {
                    smooth += (2 * Prices[i - 2]);
                    smoothCount += 2;
                }
                else if ((i - 2) >= Prices.Length)
                {
                    smooth += (2 * aSmoothArray[i - 2]);
                    smoothCount += 2;
                }

                // (((i >= 3) && ((i - 3) < Prices.Length)) ? Prices[i - 3] : 0)
                if ((i >= 3) && ((i - 3) < Prices.Length))
                {
                    smooth += Prices[i - 3];
                    smoothCount++;
                }
                else if ((i - 3) >= Prices.Length)
                {
                    smooth += aSmoothArray[i - 3];
                    smoothCount++;
                }

                aSmoothArray[i] = smooth / (int)smoothCount;


                if (i < 6)
                {
                    //aCycleArray[i] = (((i < Prices.Length) ? Prices[i] : 0.0) - (2.0 * ((i >= 1) ? Prices[i - 1] : 0.0)) + ((i >= 2) ? Prices[i - 2] : 0.0)) / 4.0;

                    double cycle = 0.0;
                    int cycleCount = 0;

                    // ((i < Prices.Length) ? Prices[i] : 0.0)
                    if (i < Prices.Length)
                    {
                        cycle += Prices[i];
                        cycleCount++;
                    }

                    // (2.0 * ((i >= 1) ? Prices[i - 1] : 0.0))
                    if ((i >= 1) && ((i - 1) < Prices.Length))
                    {
                        cycle -= (2 * Prices[i - 1]);
                        cycleCount += 2;
                    }
                    else if ((i - 1) >= Prices.Length)
                    {
                        cycle -= (2 * aCycleArray[i - 1]);
                        cycleCount += 2;
                    }

                    // ((i >= 2) ? Prices[i - 2] : 0.0)
                    if ((i >= 2) && ((i - 2) < Prices.Length))
                    {
                        cycle += Prices[i - 2];
                        cycleCount++;
                    }
                    else if ((i - 2) >= Prices.Length)
                    {
                        cycle += aCycleArray[i - 2];
                        cycleCount++;
                    }

                    aCycleArray[i] = cycle / (int)cycleCount;
                }
                else
                {
                    aCycleArray[i] = (Math.Pow(1.0 - (0.5 * _alpha), 2) * (aSmoothArray[i] - (2.0 * aSmoothArray[i - 1]) + aSmoothArray[i - 2]))
                        + ((2.0 * (1.0 - _alpha)) * aCycleArray[i - 1])
                        - (Math.Pow(1.0 - _alpha, 2) * aCycleArray[i - 2]); // 1 bar lag (by inspection)
                }
            }

            return aCycleArray;
        }

        /// <summary>
        /// TODO: unused
        /// </summary>
        /// <param name="Cycle"></param>
        /// <param name="PredictionLength"></param>
        /// <param name="DCBias"></param>
        private void GetSineIndicator(double[] Cycle, double DCBias)
        {
            double[] aQ1 = new double[Cycle.Length];    // Quadrature
            double[] aInstPeriod = new double[Cycle.Length];
            double[] aI1 = new double[Cycle.Length];    // In phase
            double[] aDeltaPhase = new double[Cycle.Length];
            double[] aPeriod = new double[Cycle.Length];
            double[] aDCPhase = new double[Cycle.Length];

            double[] aSine = new double[Cycle.Length];
            double[] aLeadSine = new double[Cycle.Length];

            double[] aPredictionSine = new double[Cycle.Length];

            for (int i = 0; i < Cycle.Length; i++)
            {
                // TODO: THIS HAS BEEN REARRANGED INCORRECTLY - aInstPeriod IS REQUIRED HERE!! (although in the book p.738 it looks right??)
                // Hilbert transform truncated at four elements
                aQ1[i] = ((0.0962 * Cycle[i]) + (0.5769 * ((i >= 2) ? Cycle[i - 2] : 0.0)) - (0.5769 * ((i >= 4) ? Cycle[i - 4] : 0.0)) - (0.0962 * ((i >= 6) ? Cycle[i - 6] : 0.0)))
                    * (0.5 + (0.08 * ((i >= 1) ? aInstPeriod[i - 1] : 0.0))); // 4 bar lag (from book)

                aI1[i] = ((i >= 3) ? Cycle[i - 3] : 0.0);

                if ((aQ1[i] != 0.0) && (i >= 1) && (aQ1[i - 1] != 0.0))
                    aDeltaPhase[i] = ((aI1[i] / aQ1[i]) - ((i >= 1) ? (aI1[i - 1] / aQ1[i - 1]) : 0.0)) / (1 + ((aI1[i] * aI1[i - 1]) / (aQ1[i] * aQ1[i - 1])));

                if (aDeltaPhase[i] < 0.1)
                    aDeltaPhase[i] = 0.1;

                if (aDeltaPhase[i] > 1.1)
                    aDeltaPhase[i] = 1.1;

                double medianDelta = Utilities.Median(aDeltaPhase, (i > 4) ? i - 4 : 0, (i > 4) ? 5 : i + 1); // 2.5 bar lag (from book)

                double dominantCycle = (medianDelta == 0.0) ? 15.0 : ((2 * Math.PI) / medianDelta) + DCBias;

                aInstPeriod[i] = (0.33 * dominantCycle) + ((1.0 - 0.33) * ((i >= 1) ? aInstPeriod[i - 1] : 0.0)); // smooth

                aPeriod[i] = (0.15 * aInstPeriod[i]) + ((1.0 - 0.15) * ((i >= 1) ? aPeriod[i - 1] : 0.0)); // smooth again - total smoothing: 1.5 bar lag (from book)

                int dcPeriod = (int)Math.Floor(aPeriod[i]);
                double realPart = 0;
                double imagPart = 0;

                for (int count = 0; count < dcPeriod; count++)
                {
                    realPart += Math.Sin((2 * Math.PI * count) / dcPeriod) * ((i >= count) ? Cycle[i - count] : 0.0);
                    imagPart += Math.Cos((2 * Math.PI * count) / dcPeriod) * ((i >= count) ? Cycle[i - count] : 0.0);
                }

                //aDCPhase[i] = Math.Atan2(realPart, imagPart); // use below as seems to put stuff in first half quandrant

                if (Math.Abs(imagPart) > 0.001)
                    aDCPhase[i] = Math.Atan(realPart / imagPart);

                if (Math.Abs(imagPart) <= 0.001)
                    aDCPhase[i] = (Math.PI / 2.0) * ((realPart >= 0) ? +1 : -1);

                aDCPhase[i] += (Math.PI / 2.0);

                if (imagPart < 0)
                    aDCPhase[i] += Math.PI;

                if (aDCPhase[i] > (1.75 * Math.PI))
                    aDCPhase[i] -= (Math.PI * 2.0);

                aSine[i] = Math.Sin(aDCPhase[i]);
                aLeadSine[i] = Math.Sin(aDCPhase[i] + (Math.PI / 4.0));

                if ((i + _maxSineWaveDelay + _numPredictionDays) < aPredictionSine.Length)
                    aPredictionSine[i + _maxSineWaveDelay + _numPredictionDays] = Math.Sin(aDCPhase[i] + (10 * (2 * Math.PI / Math.Floor(aPeriod[i]))));
            }

#if DEBUG

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                "Cycle", "Q1", "Inst Period", "I1", "Delta Phase", "Period", "DC Phase", "Sine", "Lead Sine", "Prediction"));

            for (int i = 0; i < (Cycle.Length - _maxSineWaveDelay - _numPredictionDays); i++)
            {
                sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    Cycle[i],
                    aQ1[i],
                    aInstPeriod[i],
                    aI1[i],
                    aDeltaPhase[i],
                    aPeriod[i],
                    aDCPhase[i],
                    aSine[i],
                    aLeadSine[i],
                    ""));
            }

            for (int i = (Cycle.Length - _maxSineWaveDelay - _numPredictionDays); i < Cycle.Length; i++)
            {
                sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    ((i < (Cycle.Length - _maxSineWaveDelay - _numPredictionDays + _maxCycleDelay)) ? Convert.ToString(Cycle[i]) : ""),
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    aPredictionSine[i]));
            }

#endif
        }
    }
}
