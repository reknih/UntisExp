using System;
using System.Collections.Generic;
using UntisExp.Containers;

namespace UntisExp.EventHandlers
{
    /// <summary>
    /// Contains info about finished <see cref="Group"/>-retreiving operations.
    /// </summary>
    public class GroupEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance of the Helper
        /// </summary>
        /// <param name="g">The retreived list of groups</param>
        public GroupEventArgs(List<Group> g)
        {
            _groups = g;
        }

        private List<Group> _groups;

        public List<Group> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }
    }
}