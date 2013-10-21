/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for account VPCs available for use
*/

using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AWS.AutoScale.Console.Models
{
    public class ConsoleVPC : NotifyPropertyChangeBase
    {
        private Vpc vpc;
        private ObservableCollection<ConsoleSubnet> subnets;

        /// <summary>
        /// Collection of subnets within the VPC
        /// </summary>
        public ObservableCollection<ConsoleSubnet> Subnets
        {
            get
            {
                if (subnets == null)
                {
                    subnets = new ObservableCollection<ConsoleSubnet>();
                }
                return this.subnets;
            }
            set
            {
                this.subnets = value;
                this.OnPropertyChanged("Subnets");
            }
        }

        /// <summary>
        /// Base VPC object from AWS
        /// </summary>
        public Vpc VPC
        {
            get
            {
                return this.vpc;
            }
            set
            {
                this.vpc = value;
                this.OnPropertyChanged("VPC");
            }
        }
    }
}
