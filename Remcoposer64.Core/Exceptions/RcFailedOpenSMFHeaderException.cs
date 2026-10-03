using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Remcoposer64.Core.Exceptions
{
    public class RcFailedOpenSMFHeaderException : Exception
    {
        public RcFailedOpenSMFHeaderException()
        {
        }

        public RcFailedOpenSMFHeaderException(string message) : base(message)
        {
        }

        public RcFailedOpenSMFHeaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RcFailedOpenSMFHeaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
