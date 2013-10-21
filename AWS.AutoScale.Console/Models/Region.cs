/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for regions
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class Region
    {
        private string name;

        /// <summary>
        /// constructor
        /// </summary>
		public Region()
		{
		}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Display name of the region</param>
        public Region(string name)
		{
			Name = name;
		}

        /// <summary>
        /// Display name of the region
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
        /// Auto Scaling regional SSL endpoint URL
        /// </summary>
		public string Url 
		{
			get
			{
                return string.Concat("https://autoscaling.", this.name, ".amazonaws.com");
			}
		}

        /// <summary>
        /// EC2 regional SSL endpoint url
        /// </summary>
        public string Ec2Url
        {
            get
            {
                return string.Concat("https://ec2.", this.name, ".amazonaws.com");
            }
        }

        /// <summary>
        /// ELB regional SSL endpoint url
        /// </summary>
        public string ElbUrl
        {
            get
            {
                return string.Concat("https://elasticloadbalancing.", this.name, ".amazonaws.com");
            }
        }

    }
}
