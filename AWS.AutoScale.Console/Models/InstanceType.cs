/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for instance types
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class InstanceType
    {
        private string name;

        /// <summary>
        /// base constructor
        /// </summary>
        public InstanceType()
		{
		}

        /// <summary>
        /// constructor with name
        /// </summary>
        /// <param name="name">Name of instance type</param>
        public InstanceType(string name)
		{
			Name = name;
		}

        /// <summary>
        /// Display name of instance type
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }
    }
}
