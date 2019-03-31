/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AWS.AutoScale.Console
{
    [Serializable]
    public class InvalidParametersException : Exception
    {
        public InvalidParametersException()
            : base() { }

        public InvalidParametersException(string message)
            : base(message) { }

        public InvalidParametersException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public InvalidParametersException(string message, Exception innerException)
            : base(message, innerException) { }

        public InvalidParametersException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected InvalidParametersException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
