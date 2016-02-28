using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.Shared.ConversionStrategies;
using System;

namespace SteveBagnall.Trading.Scoring
{
    public class MinStandardDeviationStrategy : ScoringStrategyBase
    {
        public MinStandardDeviationStrategy(IOHLCVToDoubleStrategy ConversionStrategy)
            : base(ConversionStrategy)
        {

        }

        internal override bool IsBetter()
        {
            int count = this.OriginalData.Count;
            double error = 0.0;

            for (int i = 0; i < count; i++)
                if (i >= (this.OriginalData.Count - this.CurrentTransformedData.Count))
                    error += Math.Pow(this.OriginalData[i] - this.CurrentTransformedData[i - (this.OriginalData.Count - this.CurrentTransformedData.Count)], 2);

            error = error / count;

            if (error < this.LowestError)
            {
                this.LowestError = error;
                return true;
            }

            return false;
        }
    }
}

