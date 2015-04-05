using System;
using System.Collections.Generic;
using UntisExp.Containers;

namespace UntisExp.EventHandlers
{
    /// <summary>
    /// Contains info about operations that failed
    /// </summary>
    public class ErrorMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance of the container
        /// </summary>
        /// <param name="h">The heading for the error message</param>
        /// <param name="b">The body of the error message</param>
        /// <param name="c">The caption on the dismiss button</param>
        public ErrorMessageEventArgs(string h, string b, string c)
        {
            _msgBody = b;
            _msgHead = h;
            _msgBtn = c;
        }

        private string _msgHead;

        public string MessageHead
        {
            get { return _msgHead; }
            set { _msgHead = value; }
        }

        private string _msgBody;

        public string MessageBody
        {
            get { return _msgBody; }
            set { _msgBody = value; }
        }

        private string _msgBtn;

        public string MessageButton
        {
            get { return _msgBtn; }
            set { _msgBtn = value; }
        }
    }
}