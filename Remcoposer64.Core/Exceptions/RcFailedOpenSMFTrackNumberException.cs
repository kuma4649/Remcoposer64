using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Remcoposer64.Core.Exceptions
{
    public class RcFailedOpenSMFTrackNumberException : Exception
    {
        public RcFailedOpenSMFTrackNumberException()
        {
        }

        public RcFailedOpenSMFTrackNumberException(string message) : base(message)
        {
        }

        public RcFailedOpenSMFTrackNumberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RcFailedOpenSMFTrackNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
