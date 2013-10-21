/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
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
