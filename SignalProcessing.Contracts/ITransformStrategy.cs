using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing.Contracts
{
    public interface ITransformStrategy
    {
        List<double> Remove(List<double> Data, ref Stack<ITransformation> Transforms);
    }
}
