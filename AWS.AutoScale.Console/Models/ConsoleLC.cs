/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model for existing account Launch Configurations displayed in console main window
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.AutoScaling.Model;
using System.Collections.ObjectModel;
using Amazon.EC2.Model;


namespace AWS.AutoScale.Console.Models
{
    public class ConsoleLC : NotifyPropertyChangeBase
    {
        private LaunchConfiguration launchConfiguration;

        /// <summary>
        /// Base launch configuration object from AWS
        /// </summary>
        public LaunchConfiguration LaunchConfiguration
        {
            get
            {
                if (this.launchConfiguration == null)
                    this.launchConfiguration = new LaunchConfiguration();
                return this.launchConfiguration;
            }
            set
            {
                this.launchConfiguration = value;
                this.OnPropertyChanged("LaunchConfiguration");
            }
        }

        /// <summary>
        /// String displaying security groups associated with the Launch Configuration
        /// </summary>
        public string SecurityGroups
        {
            get
            {
                string sgs = string.Empty;
                foreach (string sg in this.LaunchConfiguration.SecurityGroups)
                {
                    sgs = string.Concat(sgs, !string.IsNullOrEmpty(sgs) ? ", " : "", sg);
                }
                return sgs;
            }
        }

    }
}
