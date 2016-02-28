using System;

namespace SteveBagnall.Trading.SignalProcessing.Contracts
{
    public interface ITransformation
	{
        void Undo(UndoOptions DaysAhead, ref double Target);
        double ValueAt(double X);
    }
}
