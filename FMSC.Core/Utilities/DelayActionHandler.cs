using System;
using System.Timers;

namespace FMSC.Core.Utilities
{
    public class DelayActionHandler
    {
        private Timer _Timer;
        private long _Delay = 1000;
        private Action _Action;


        public DelayActionHandler()
        {
            _Timer = new Timer(_Delay);
            _Timer.AutoReset = false;
            _Timer.Elapsed += Timer_Elapsed;
        }

        public DelayActionHandler(long delay) : this()
        {
            _Delay = delay;
            _Timer.Interval = _Delay;
        }

        public DelayActionHandler(Action action, long delay) : this(delay)
        {
            _Action = action;
        }
        

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _Action?.Invoke();
        }
        

        public void DelayInvoke()
        {
            DelayInvoke(_Action, _Delay);
        }

        public void DelayInvoke(Action action)
        {
            DelayInvoke(action, _Delay);
        }

        public void DelayInvoke(Action action, long delay)
        {
            _Timer.Stop();
            _Action = action;

            if (_Delay != delay)
            {
                _Delay = delay;
                _Timer.Interval = _Delay;
            }

            _Timer.Start();
        }


        public void Cancel()
        {
            _Timer.Stop();
        }
    }
}
