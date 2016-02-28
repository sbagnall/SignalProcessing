using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.Shared.ConversionStrategies;
using System;

namespace SteveBagnall.Trading.Scoring
{
    public class LowestSquareStrategy : ScoringStrategyBase
    {
        public LowestSquareStrategy(IOHLCVToDoubleStrategy ConversionStrategy)
            : base(ConversionStrategy)
        {

        }

        internal override bool IsBetter()
        {
            double stdDev = Utilities.StandardDeviation(this.CurrentTransformedData);

            if (stdDev < this.LowestError)
            {
                this.LowestError = stdDev;
                return true;
            }

            return false;
        }
    }
}

