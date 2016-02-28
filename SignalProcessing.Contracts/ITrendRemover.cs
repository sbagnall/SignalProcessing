using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing.Contracts
{
    public interface ITrendRemover
    {
        List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms);
    }
}
