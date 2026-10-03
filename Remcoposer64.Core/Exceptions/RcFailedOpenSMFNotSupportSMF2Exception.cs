using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Remcoposer64.Core.Exceptions
{
    public class RcFailedOpenSMFNotSupportSMF2Exception : Exception
    {
        public RcFailedOpenSMFNotSupportSMF2Exception()
        {
        }

        public RcFailedOpenSMFNotSupportSMF2Exception(string message) : base(message)
        {
        }

        public RcFailedOpenSMFNotSupportSMF2Exception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RcFailedOpenSMFNotSupportSMF2Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
