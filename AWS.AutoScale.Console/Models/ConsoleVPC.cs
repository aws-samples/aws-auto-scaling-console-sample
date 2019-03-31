/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
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
