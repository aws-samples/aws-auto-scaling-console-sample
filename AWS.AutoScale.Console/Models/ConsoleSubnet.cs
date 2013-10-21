/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for subnets available in a VPC for use
*/

using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AWS.AutoScale.Console.Models
{
    public class ConsoleSubnet : NotifyPropertyChangeBase
    {
        private Subnet subnet;
        private string displayname;

        /// <summary>
        /// Friendly display name of subnet
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.displayname;
            }
            set
            {
                this.displayname = value;
                this.OnPropertyChanged("DisplayName");
            }
        }

        /// <summary>
        /// Base Subnet object from AWS
        /// </summary>
        public Subnet Subnet
        {
            get
            {
                return this.subnet;
            }
            set
            {
                this.subnet = value;
                this.OnPropertyChanged("Subnet");
            }
        }

    }
}
