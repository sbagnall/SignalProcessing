using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing
{
    public class CompositeWave
    {
        private WaveType _waveType = WaveType.NotSet;
        private List<Wave> _waves = new List<Wave>();

        public CompositeWave(WaveType WaveType)
        {
            _waveType = WaveType;
        }

        public bool Add(Wave Wave)
        {
            if (Wave.WaveType == _waveType)
            {
                _waves.Add(Wave);
                return true;
            }
            else
                return false;
        }

        public double ValueAt(double X)
        {
            double value = 0.0;
            foreach (Wave wave in _waves)
                value += Math.Cos((2 * Math.PI * wave.Frequency * X) + wave.Phase) * wave.Magnitude;

            return value;
        }
    }
}
