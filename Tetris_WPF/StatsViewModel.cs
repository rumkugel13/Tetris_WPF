namespace Tetris_WPF
{
    using System;
    using System.ComponentModel;

    public class StatsViewModel : INotifyPropertyChanged
    {
        private int points, lines, level;
        private double speed;
        private TimeSpan time;

        public StatsViewModel()
        {
            Points = 0;
            Lines = 0;
            Level = 1;
            Speed = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Points
        {
            get
            {
                return points;
            }

            set
            {
                if (points != value)
                {
                    points = value;
                    RaisePropertyChanged("Points");
                }
            }
        }

        public int Lines
        {
            get
            {
                return lines;
            }

            set
            {
                if (lines != value)
                {
                    lines = value;
                    RaisePropertyChanged("Lines");
                }
            }
        }

        public int Level
        {
            get
            {
                return level;
            }

            set
            {
                if (level != value)
                {
                    level = value;
                    RaisePropertyChanged("Level");
                }
            }
        }

        public double Speed
        {
            get
            {
                return speed;
            }

            set
            {
                if (speed != value)
                {
                    speed = value;
                    RaisePropertyChanged("SpeedFormat");
                }
            }
        }

        public string SpeedFormat
        {
            get
            {
                return Speed.ToString("#.00");
            }
        }

        public TimeSpan Time
        {
            get
            {
                return time;
            }

            set
            {
                if (time != value)
                {
                    time = value;
                    RaisePropertyChanged("TimeFormat");
                }
            }
        }

        public string TimeFormat
        {
            get
            {
                return time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00") + ":" + (time.Milliseconds / 100).ToString();
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
