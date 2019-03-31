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
    public class InvalidRegionException: Exception
    {
        public InvalidRegionException()
            : base() { }
    
        public InvalidRegionException(string message)
            : base(message) { }
    
        public InvalidRegionException(string format, params object[] args)
            : base(string.Format(format, args)) { }
    
        public InvalidRegionException(string message, Exception innerException)
            : base(message, innerException) { }
    
        public InvalidRegionException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected InvalidRegionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
