using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AWS.AutoScale.Console
{
    public class DataLoadingException : Exception
    {
        public DataLoadingException()
            : base() { }
    
        public DataLoadingException(string message)
            : base(message) { }
    
        public DataLoadingException(string format, params object[] args)
            : base(string.Format(format, args)) { }
    
        public DataLoadingException(string message, Exception innerException)
            : base(message, innerException) { }
    
        public DataLoadingException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected DataLoadingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
