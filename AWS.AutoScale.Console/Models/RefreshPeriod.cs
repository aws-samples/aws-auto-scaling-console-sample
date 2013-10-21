/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for refresh period
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class RefreshPeriod
    {
        private string name;
        private int period;

        /// <summary>
        /// constructor
        /// </summary>
        public RefreshPeriod()
		{
		}

        /// <summary>
        /// constructor with name and period
        /// </summary>
        /// <param name="name">Name of refresh period</param>
        /// <param name="period">Number of seconds for refresh period</param>
        public RefreshPeriod(string name, int period)
		{
			Name = name;
            Period = period;
		}

        /// <summary>
        /// Display name of the refresh period
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

        /// <summary>
        /// Number of seconds for refresh period
        /// </summary>
        public int Period
        {
            get
            {
                return this.period;
            }

            set
            {
                this.period = value;
            }
        }
    }
}
