/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
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
