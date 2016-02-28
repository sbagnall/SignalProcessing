using SteveBagnall.Trading.Scoring;
using SteveBagnall.Trading.Shared;
using SteveBagnall.Trading.SignalProcessing.Contracts;
using SteveBagnall.Trading.SignalProcessing.LinearApproximations;
using SteveBagnall.Trading.SignalProcessing.TrendRemovers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteveBagnall.Trading.SignalProcessing
{
    public class TrendRemover : ITransformStrategy
    {
        private IScoringStrategy _scoringStrategy = null;

        public TrendRemover(IScoringStrategy ScoringStrategy)
        {
            _scoringStrategy = ScoringStrategy;
        }

        public List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms)
        {
            List<double> retVal = null;

            List<ITrendRemover> removers = new List<ITrendRemover>();
            removers.Add(new Linear());
            removers.Add(new Exponential());
            removers.Add(new Logarithmic());
            removers.Add(new Power());
            removers.Add(new Poly(2));
            removers.Add(new Poly(3));
            removers.Add(new Poly(4));

            double lowestError = Double.MaxValue;
            ITransformation lowestTransformation = null;
            _scoringStrategy.StartNewTransformation(Data, null);

            foreach (ITrendRemover remover in removers)
            {
                Stack<ITransformation> transforms = new Stack<ITransformation>();
                List<double> thisData = remover.Remove(Data, ref transforms);

#if DEBUG

                StringBuilder sbCompDecycle = new StringBuilder();

                for (int i = 0; i < thisData.Count; i++)
                    sbCompDecycle.AppendLine(String.Format("{0},{1}", Data[i], thisData[i]));

#endif

                _scoringStrategy.AddTransformedValues(thisData);
                _scoringStrategy.Score();

                double error = _scoringStrategy.LowestError;

                if (error < lowestError)
                {
                    lowestError = error;
                    lowestTransformation = transforms.Pop();
                    retVal = (List<double>)Utilities.DeepClone(thisData);
                }
            }

            if (lowestTransformation != null)
                Transforms.Push(lowestTransformation);

            return retVal;
        }
    }
}
