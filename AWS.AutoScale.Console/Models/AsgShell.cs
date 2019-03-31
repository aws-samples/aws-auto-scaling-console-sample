/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model shell for editing or creating an Auto Scaling Group in the console
*/

using Amazon.AutoScaling.Model;
using Amazon.ElasticLoadBalancing.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EC2 = Amazon.EC2.Model;

namespace AWS.AutoScale.Console.Models
{
    public class AsgShell : NotifyPropertyChangeBase, IDataErrorInfo
    {
        private string name;
        private ConsoleLC launchConfiguration;
        private List<EC2.AvailabilityZone> availabilityZones;
        private List<ConsoleSubnet> subnets;
        private int minSize;
        private int maxSize;
        private int desiredCapacity;
        private int gracePeriod = 300;
        private int cooldown;
        private LoadBalancerDescription loadBalancer;
        public Dictionary<string, string> Errors = new Dictionary<string, string>();
        public bool IsValidating = false;

        /// <summary>
        /// Error Property required for data error info
        /// </summary>
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Error message retrieval based on property name
        /// </summary>
        /// <param name="columnName">Name of property to retrieve error message for</param>
        /// <returns>Error message string</returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                if (!IsValidating) return result;
                Errors.Remove(columnName);
                switch (columnName)
                {
                    case "Name": if (string.IsNullOrEmpty(Name)) result = "Name is required"; break;
                    case "LaunchConfiguration": if (string.IsNullOrEmpty(Name)) result = "Launch Configuration is required"; break;
                    case "MinSize": if (MinSize < 0) result = "Minimum Size must be >= 0"; break;
                    case "MaxSize": if (MaxSize < 0) result = "Maximum Size must be >= 0"; break;
                }

                if (result != string.Empty) Errors.Add(columnName, result);
                return result;
            }
        }

        /// <summary>
        /// Determines validity of data properties
        /// </summary>
        /// <returns>true/false based on property validity</returns>
        public bool IsValid()
        {
            IsValidating = true;
            try
            {
                base.OnPropertyChanged("Name");
                base.OnPropertyChanged("LaunchConfiguration");
                base.OnPropertyChanged("MinSize");
                base.OnPropertyChanged("MaxSize");
            }
            finally
            {
                IsValidating = false;
            }
            return (Errors.Count() == 0);
        }

        /// <summary>
        /// Name of Auto Scaling Group
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
                base.OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Launch Configuration for the Auto Scaling Group
        /// </summary>
        public ConsoleLC LaunchConfiguration
        {
            get
            {
                return this.launchConfiguration;
            }
            set
            {
                this.launchConfiguration = value;
                base.OnPropertyChanged("LaunchConfiguration");
            }
        }

        /// <summary>
        /// Availability Zones applied to the Auto Scaling Group
        /// </summary>
        public List<EC2.AvailabilityZone> Zones
        {
            get
            {
                if (this.availabilityZones == null)
                {
                    this.availabilityZones = new List<EC2.AvailabilityZone>();
                }

                return this.availabilityZones;
            }
            set
            {
                if (this.availabilityZones != value)
                {
                    this.availabilityZones = value;
                    base.OnPropertyChanged("Zones");
                }
            }
        }

        /// <summary>
        /// Subnets used in the Auto Scaling Group
        /// </summary>
        public List<ConsoleSubnet> Subnets
        {
            get
            {
                if (this.subnets == null)
                {
                    this.subnets = new List<ConsoleSubnet>();
                }

                return this.subnets;
            }
            set
            {
                if (this.subnets != value)
                {
                    this.subnets = value;
                    base.OnPropertyChanged("Subnets");
                }
            }
        }

        /// <summary>
        /// The minimum size of the Auto Scaling group
        /// </summary>
        public int MinSize
        {
            get
            {
                return this.minSize;
            }
            set
            {
                this.minSize = value;
                base.OnPropertyChanged("MinSize");
            }
        }

        /// <summary>
        /// The maximum size of the Auto Scaling group
        /// </summary>
        public int MaxSize
        {
            get
            {
                return this.maxSize;
            }
            set
            {
                this.maxSize = value;
                base.OnPropertyChanged("MaxSize");
            }
        }

        /// <summary>
        /// The number of Amazon EC2 instances that should be running in the group
        /// </summary>
        public int DesiredCapacity
        {
            get
            {
                return this.desiredCapacity;
            }
            set
            {
                this.desiredCapacity = value;
                base.OnPropertyChanged("DesiredCapacity");
            }
        }

        /// <summary>
        /// Length of time in seconds after a new Amazon EC2 instance comes into service that Auto Scaling starts checking its health
        /// </summary>
        public int GracePeriod
        {
            get
            {
                return this.gracePeriod;
            }
            set
            {
                this.gracePeriod = value;
                base.OnPropertyChanged("GracePeriod");
            }
        }

        /// <summary>
        /// The amount of time, in seconds, after a scaling activity completes before any further trigger-related scaling activities can start
        /// </summary>
        public int Cooldown
        {
            get
            {
                return this.cooldown;
            }
            set
            {
                this.cooldown = value;
                base.OnPropertyChanged("Cooldown");
            }
        }

        /// <summary>
        /// Load balancers to use in the Auto Scaling Group
        /// </summary>
        public LoadBalancerDescription LoadBalancer
        {
            get
            {
                return this.loadBalancer;
            }
            set
            {
                this.loadBalancer = value;
                base.OnPropertyChanged("LoadBalancer");
            }
        }
    }
}
