using System;
using System.Collections.Generic;
using UntisExp.Containers;

namespace UntisExp.EventHandlers
{
    /// <summary>
    /// Contains info about finished <see cref="UntisExp.Containers.News"/>-retreiving operations.
    /// </summary>
    public class MassiveNewsEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance of the Helper
        /// </summary>
        /// <param name="g">The retreived list of groups</param>
        public MassiveNewsEventArgs(List<News> g)
        {
            _news = g;
        }

        private List<News> _news;

        public List<News> News
        {
            get { return _news; }
            set { _news = value; }
        }
    }
}