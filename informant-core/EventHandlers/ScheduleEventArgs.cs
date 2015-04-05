using System;
using System.Collections.Generic;
using UntisExp.Containers;

namespace UntisExp.EventHandlers
{
    /// <summary>
    /// Contains info about finished <see cref="UntisExp.Containers.Data"/>-retreiving operations.
    /// </summary>
    public class ScheduleEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance of the Helper
        /// </summary>
        /// <param name="g">The retreived list of groups</param>
        public ScheduleEventArgs(List<Data> g)
        {
            _schedule = g;
        }

        private List<Data> _schedule;

        public List<Data> Schedule
        {
            get { return _schedule; }
            set { _schedule = value; }
        }
    }
}