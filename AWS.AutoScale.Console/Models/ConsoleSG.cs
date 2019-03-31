/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model for existing account Security Groups for use
*/

using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AWS.AutoScale.Console.Models
{
    public class ConsoleSG : NotifyPropertyChangeBase
    {
        private SecurityGroup awsSecurityGroup;
        private string displayname;

        /// <summary>
        /// Display name of Security Group
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
        /// Base Security Group object from AWS
        /// </summary>
        public SecurityGroup SecurityGroup
        {
            get
            {
                return this.awsSecurityGroup;
            }
            set
            {
                this.awsSecurityGroup = value;
                this.OnPropertyChanged("SecurityGroup");
            }
        }
    }
}
