using System;
using System.Timers;
using GeoDoorServer3.Data;

namespace GeoDoorServer3.CustomService.Models
{
    public class SystemStatus
    {
        private TimeSpan _onlineTimeSpan;
        private DateTime _startTime;
        private GateStatus _gateStatus;
        private bool _isGateMoving;
        private bool _openHabStatus;

        private Timer _gateMotionTimer;
        private int _gateTimeout;

        private readonly object _onlineTimeSpanLock = new object();
        private readonly object _startTimeLock = new object();
        private readonly object _gateStatusLock = new object();
        private readonly object _isGateMovingLock = new object();
        private readonly object _openHabStatusLock = new object();
        
        public TimeSpan OnlineTimeSpan
        {
            get
            {
                lock (_onlineTimeSpanLock)
                {
                    return _onlineTimeSpan;
                }
            }
            set
            {
                lock (_onlineTimeSpanLock)
                {
                    _onlineTimeSpan = value;
                }
            }
        }

        public DateTime StartTime
        {
            get
            {
                lock (_startTimeLock)
                {
                    return _startTime;
                }
            }
            set
            {
                lock (_startTimeLock)
                {
                    _startTime = value;
                }
            }
        }

        public GateStatus GateStatus
        {
            get
            {
                lock (_gateStatusLock)
                {
                    return _gateStatus;
                }
            }
            set
            {
                lock (_gateStatusLock)
                {
                    if (!IsGateMoving)
                        _gateStatus = value;
                }
            }
        }

        public bool IsGateMoving
        {
            get
            {
                lock (_isGateMovingLock)
                {
                    return _isGateMoving;
                }
            }
            set
            {
                lock (_isGateMovingLock)
                {
                    if (value)
                    {
                        if (GateStatus.Equals(GateStatus.GateOpen))
                        {
                            StartGateMotionTimer();
                            _gateStatus = GateStatus.GateClosing;
                        }
                        else if (GateStatus.Equals(GateStatus.GateClosed))
                        {
                            StartGateMotionTimer();
                            _gateStatus = GateStatus.GateOpening;
                        }
                        else if (GateStatus.Equals(GateStatus.GateClosing))
                        {
                            _gateStatus = GateStatus.GateOpening;
                        }
                        else if (GateStatus.Equals(GateStatus.GateOpening))
                        {
                            _gateStatus = GateStatus.GateClosing;
                        }
                    }
                    else
                    {
                        if (GateStatus.Equals(GateStatus.GateClosing))
                        {
                            _gateStatus = GateStatus.GateClosed;
                        }
                        else if (GateStatus.Equals(GateStatus.GateOpening))
                        {
                            _gateStatus = GateStatus.GateOpen;
                        }
                    }
                    _isGateMoving = value;
                }
            }
        }

        public bool OpenHabStatus
        {
            get
            {
                lock (_openHabStatusLock)
                {
                    return _openHabStatus;
                }
            }
            set
            {
                lock (_openHabStatusLock)
                {
                    _openHabStatus = value;
                }
            }
        }

        public int GateTimeout
        {
            get => _gateTimeout;
            set => _gateTimeout = value;
        }

        public SystemStatus()
        {
            _onlineTimeSpan = TimeSpan.Zero;
            _startTime = DateTime.Now;
            _gateStatus = GateStatus.GateOpen;
            _openHabStatus = false;

            _gateMotionTimer = new Timer();
            _gateMotionTimer.Interval = 30000;
            _gateMotionTimer.Elapsed += (sender, args) =>
            {
                _gateMotionTimer.Stop();
                IsGateMoving = false;
            };
        }

        private void StartGateMotionTimer()
        {
            _gateMotionTimer.Interval = _gateTimeout;
            _gateMotionTimer.Start();
        }
    }

    public enum GateStatus
    {
        GateOpen,
        GateOpening,
        GateClosing,
        GateClosed
    }
}
