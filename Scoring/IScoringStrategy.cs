using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.Scoring
{
    public interface IScoringStrategy
    {
        void Score();

        void StartNewTransformation(List<OHLCV> OriginalData, ITransformation Transform);

        void StartNewTransformation(List<double> OriginalData, ITransformation Transform);

        void AddTransformedValue(OHLCV Value);

        void AddTransformedValue(double Value);

        void AddTransformedValues(List<OHLCV> Values);

        void AddTransformedValues(List<double> Values);

        double LowestError { get; }

        List<double> OriginalData { get; }

        List<double> CurrentTransformedData { get; set; }

        ITransformation CurrentTransform { get; }

        //IOHLCVToDoubleStrategy ConversionStrategy { get; }

        ITransformation BestTransform { get; }

        List<double> BestTransformedData { get; }

    }
}
