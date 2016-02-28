using AForge.Math;
using SteveBagnall.Trading.Scoring;
using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.Shared.Windows;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteveBagnall.Trading.SignalProcessing
{
    public class Fourier : ITransformStrategy
    {
        private IWindow _window;
        private double _zeroPaddingFactor;
        private double _samplingFrequency;
        private double _spectrumPeakStdDevs;
        private IWindow _filterWindowForResults;
        private FourierType _fourierType;
        private IScoringStrategy _scoringStrategy = null;

        public Fourier(
            IWindow Window,
            double ZeroPaddingFactor,
            double SamplingFrequency,
            double SpectrumPeakStdDevs,
            IWindow FilterWindowForResults,
            FourierType FourierType,
            IScoringStrategy ScoringStrategy)
        {
            _window = Window;
            _zeroPaddingFactor = ZeroPaddingFactor;
            _samplingFrequency = SamplingFrequency;
            _spectrumPeakStdDevs = SpectrumPeakStdDevs;
            _filterWindowForResults = FilterWindowForResults;
            _fourierType = FourierType;
            _scoringStrategy = ScoringStrategy;
        }

        public List<double> Remove(
            List<double> Data,
            ref Stack<ITransformation> Transforms)
        {
            if (Transforms == null)
            {
                Transforms = new Stack<ITransformation>();
            }

            var n = Data.Count;
            if (_fourierType == FourierType.FFT && ((n & (n - 1)) != 0))
            {
                throw new ApplicationException("for FFT data size needs to be a power of two");
            }

            List<Wave> waves = null;
            double[] t;

            DoFourier(Data, out waves, out t);


            double[] mags = new double[waves.Count];
            for (int i = 0; i < waves.Count; i++)
            {
                Wave wave = waves[i];
                mags[i] = (int)Math.Floor(wave.Magnitude);
            }

            double stdDevOfMags = Utilities.StandardDeviation(new List<double>(mags));

            _scoringStrategy.StartNewTransformation(Data, null);

            List<double> decycledData = (List<double>)Utilities.DeepClone(Data);
            List<double> previousDecycledData = (List<double>)Utilities.DeepClone(decycledData);
            double lowestError = 0.0;
            double[] compositeWaveValues = new double[Data.Count];
            int indexOfLowest = 0;

            for (int i = 0; i < waves.Count; i++)
            {
                double frequency = waves[i].Frequency;
                double magnitude = waves[i].Magnitude;
                double phase = waves[i].Phase;

                //if (magnitude / stdDevOfMags < _spectrumPeakStdDevs)
                //    break;

                for (int j = 0; j < decycledData.Count; j++)
                {
                    compositeWaveValues[j] += Math.Cos((2 * Math.PI * frequency * t[j]) + phase) * magnitude;
                    decycledData[j] -= compositeWaveValues[j];
                    _scoringStrategy.AddTransformedValue(Data[j] - compositeWaveValues[j]);
                }

                _scoringStrategy.Score();

                double error = _scoringStrategy.LowestError;

                if (error < lowestError)
                {
                    lowestError = error;
                    indexOfLowest = i;
                    decycledData = (List<double>)Utilities.DeepClone(Data);
                }
            }

            compositeWaveValues = new double[Data.Count];

            CompositeWave compositeWave = new CompositeWave(WaveType.Cosine);

            double lastX = 0.0;
            for (int i = 0; i <= indexOfLowest; i++)
            {
                double frequency = waves[i].Frequency;
                double magnitude = waves[i].Magnitude;
                double phase = waves[i].Phase;

                for (int j = 0; j < decycledData.Count; j++)
                {
                    compositeWaveValues[j] += Math.Cos((2 * Math.PI * frequency * t[j]) + phase) * magnitude;
                    decycledData[j] -= (compositeWaveValues[j] * _filterWindowForResults.ValueAt(decycledData.Count, j));

                    lastX = t[j];
                }

                compositeWave.Add(new Wave(WaveType.Cosine, frequency, magnitude, phase));
            }

            Transforms.Push(new WaveTransformation(compositeWave, lastX));

#if DEBUG

            StringBuilder sbCompDecycle = new StringBuilder();

            for (int i = 0; i < compositeWaveValues.Length; i++)
            {
                double comp = compositeWaveValues[i] * _filterWindowForResults.ValueAt(decycledData.Count, i);
                double decycle = decycledData[i];
                sbCompDecycle.AppendLine(String.Format("{0},{1}", comp, decycle));
            }

#endif

            return decycledData;
        }


        private void DoFourier(List<double> Data, out List<Wave> Waves, out double[] TimeVector)
        {
            int n = (int)Math.Floor(Data.Count * _zeroPaddingFactor);

            Complex[] timeDomainSignal = new Complex[n];
            Complex[] freqDomainTransform = new Complex[n];

            int index = 0;
            int dataIndex = 0;

            for (; index < Data.Count; index++, dataIndex++)
            {
                timeDomainSignal[index].Re = Data[dataIndex] * _window.ValueAt(n, dataIndex);
                freqDomainTransform[index].Re = timeDomainSignal[index].Re;
            }

            for (; index < (n - Data.Count); index++)
            {
                timeDomainSignal[index].Re = 0.0;
                freqDomainTransform[index].Re = 0.0;
            }

            TimeVector = new double[(int)Math.Floor(n * _zeroPaddingFactor)];
            for (int i = 0; i < n; i++)
                TimeVector[i] = (double)i / (double)_zeroPaddingFactor;

            switch (_fourierType)
            {
                case FourierType.DFT:
                    FourierTransform.DFT(freqDomainTransform, FourierTransform.Direction.Forward);
                    break;
                case FourierType.FFT:
                case FourierType.NotSet:
                default:
                    FourierTransform.FFT(freqDomainTransform, FourierTransform.Direction.Forward);
                    break;
            }

            

            // nyquist frequency
            int cutOff = (int)Math.Ceiling(n / (double)2);

            // FFT is symmetric, take first half
            Complex[] y1 = new Complex[cutOff];
            for (int i = 0; i < cutOff; i++)
                y1[i] = freqDomainTransform[i];

            // compensate for the energy of the other half
            for (int i = 1; i < cutOff - 1; i++)
                y1[i] = y1[i] * 2;

            double[] p = new double[cutOff];
            for (int i = 0; i < cutOff; i++)
            {
                double phase = Math.Atan2(y1[i].Im, y1[i].Re);
                if (i != 0)
                    if (Math.Abs(p[i - 1] - phase) > Math.PI)
                        phase = (phase > p[i - 1]) ? phase - (2 * Math.PI) : phase + (2 * Math.PI);   // unwrap

                p[i] = phase;
            }

            double[] f = new double[n];             // Frequency vector
            int fIndex = 0;
            for (double i = 0; fIndex < n; i += _samplingFrequency / (double)(n - 1))
            {
                f[fIndex] = i;
                fIndex++;
            }

#if DEBUG
            StringBuilder sbPlot = new StringBuilder();
            sbPlot.AppendLine(String.Format("{0},{1},{2},{3},{4}", "Time", "Amplitude", "Frequency", "Magnitude", "Phase (rad)"));

            for (int i = 0; i < n; i++)
            {
                sbPlot.AppendLine(String.Format("{0},{1},{2},{3},{4}",
                    TimeVector[i],
                    timeDomainSignal[i].Re,
                    (i < cutOff) ? Convert.ToString(f[i]) : "",
                    (i < cutOff) ? Convert.ToString(Math.Sqrt(Math.Pow(y1[i].Re, 2) + Math.Pow(y1[i].Im, 2))) : "",
                    (i < cutOff) ? Convert.ToString(p[i]) : ""));
            }
#endif

            Waves = new List<Wave>();
            for (int i = 0; i < cutOff; i++)
            {
                Wave wave = new Wave(WaveType.Cosine);
                wave.Frequency = f[i];
                wave.Magnitude = Math.Sqrt(Math.Pow(y1[i].Re, 2) + Math.Pow(y1[i].Im, 2));
                wave.Phase = p[i];
                Waves.Add(wave);
            }

            Waves.Sort(new WaveComparer(true));
        }
    }
}
