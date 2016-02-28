using System;
using System.Collections.Generic;

namespace SteveBagnall.Trading.SignalProcessing
{
    public class Wave
    {
        private WaveType _waveType = WaveType.NotSet;
        public virtual WaveType WaveType
        {
            protected set { _waveType = value; }
            get { return _waveType; }
        }

        private double _frequency = 0.0;
        public virtual double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        private double _magnitude = 0.0;
        public virtual double Magnitude
        {
            get { return _magnitude; }
            set { _magnitude = value; }
        }

        private double _phase = 0.0;
        public virtual double Phase
        {
            get { return _phase; }
            set { _phase = value; }
        }

        public Wave(WaveType WaveType)
        {
            _waveType = WaveType;
        }

        public Wave(WaveType WaveType, double Frequency, double Magnitude, double Phase)
        {
            _waveType = WaveType;
            _frequency = Frequency;
            _magnitude = Magnitude;
            _phase = Phase;
        }
    }

    public class WaveComparer : IComparer<Wave>
    {
        private bool _sortDescending = false;

        public WaveComparer(bool descending)
        {
            _sortDescending = descending;
        }

        public int Compare(Wave lhs, Wave rhs)
        {
            if (_sortDescending)
                return rhs.Magnitude.CompareTo(lhs.Magnitude);
            else
                return lhs.Magnitude.CompareTo(rhs.Magnitude);
        }
    }
}
