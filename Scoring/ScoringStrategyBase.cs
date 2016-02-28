using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.Shared.ConversionStrategies;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.Scoring
{
    public abstract class ScoringStrategyBase : IScoringStrategy
    {
        private double _lowestError = Double.MaxValue;
        public double LowestError
        {
            get { return _lowestError; }
            protected set { _lowestError = value; }
        }

        private List<double> _originalData = null;
        public List<double> OriginalData
        {
            get { return _originalData; }
        }

        private List<double> _currentTransformedData = null;
        public List<double> CurrentTransformedData
        {
            get { return _currentTransformedData; }
            set { _currentTransformedData = value; }
        }

        private ITransformation _currentTransform = null;
        public ITransformation CurrentTransform
        {
            get { return _currentTransform; }
        }

        private IOHLCVToDoubleStrategy _conversionStrategy = null;
        public IOHLCVToDoubleStrategy ConversionStrategy
        {
            get { return _conversionStrategy; }
        }

        private ITransformation _bestTransform = null;
        public ITransformation BestTransform
        {
            get { return _bestTransform; }
        }

        private List<double> _bestTransformedData = null;
        public List<double> BestTransformedData
        {
            get { return _bestTransformedData; }
        }

        public ScoringStrategyBase(IOHLCVToDoubleStrategy ConversionStrategy)
        {
            _conversionStrategy = ConversionStrategy;
            _lowestError = Double.MaxValue;
        }

        public void StartNewTransformation(List<OHLCV> OriginalData, ITransformation Transform)
        {
            foreach (OHLCV value in OriginalData)
                _originalData.Add(this.ConversionStrategy.Convert(value));

            _currentTransformedData = new List<double>();
            _currentTransform = Transform;
        }

        public void StartNewTransformation(List<double> OriginalData, ITransformation Transform)
        {
            _originalData = OriginalData;

            _currentTransformedData = new List<double>();
            _currentTransform = Transform;
        }


        public void AddTransformedValue(OHLCV Value)
        {
            if (_currentTransformedData == null)
                throw new ApplicationException("Must first start new transformation.");

            AddTransformedValue(this.ConversionStrategy.Convert(Value));
        }

        public void AddTransformedValue(double Value)
        {
            if (_currentTransformedData == null)
                throw new ApplicationException("Must first start new transformation.");

            _currentTransformedData.Add(Value);
        }


        public void AddTransformedValues(List<OHLCV> Values)
        {
            if (_currentTransformedData == null)
                throw new ApplicationException("Must first start new transformation.");

            foreach (OHLCV value in Values)
                AddTransformedValue(this.ConversionStrategy.Convert(value));
        }


        public void AddTransformedValues(List<double> Values)
        {
            if (_currentTransformedData == null)
                throw new ApplicationException("Must first start new transformation.");

            _currentTransformedData = Values;
        }

        public void Score()
        {
            //if (_currentTransformedData.Count != _originalData.Count)
            //    throw new ApplicationException("Current and original data must be the same length.");

            if (IsBetter())
            {
                _bestTransform = _currentTransform;
                _bestTransformedData = (List<double>)Utilities.DeepClone(_currentTransformedData);
            }
        }

        internal abstract bool IsBetter();
    }
}
