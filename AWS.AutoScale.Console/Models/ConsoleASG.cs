/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Data model for existing account Auto Scaling Groups displayed in console main window
*/

using Amazon.AutoScaling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWS.AutoScale.Console.Models
{
    public class ConsoleASG : NotifyPropertyChangeBase
    {
        private AutoScalingGroup autoScalingGroup;
        private List<NotificationConfiguration> notificationsConfigurations;

        /// <summary>
        /// Base Auto Scaling Group object from AWS
        /// </summary>
        public AutoScalingGroup AutoScalingGroup
        {
            get
            {
                if (this.autoScalingGroup == null)
                    this.autoScalingGroup = new AutoScalingGroup();
                return this.autoScalingGroup;
            }
            set
            {
                this.autoScalingGroup = value;
                this.OnPropertyChanged("AutoScalingGroup");
            }
        }

        /// <summary>
        /// Availability Zones for the Auto Scaling group
        /// </summary>
        public string Zones
        {
            get
            {
                string zones = string.Empty;
                foreach (string z in this.AutoScalingGroup.AvailabilityZones)
                {
                    zones = string.Concat(zones, !string.IsNullOrEmpty(zones) ? ", " : "", z);
                }
                return zones;
            }
        }

        /// <summary>
        /// Elastic load balancer to used with Auto Scaling Group
        /// </summary>
        public string ElasticLoadBalancer
        {
            get
            {
                string elbs = string.Empty;
                foreach (string lb in this.AutoScalingGroup.LoadBalancerNames)
                {
                    elbs = string.Concat(elbs, !string.IsNullOrEmpty(elbs) ? ", " : "", lb);
                }
                return elbs;
            }
        }

        /// <summary>
        /// List of notification actions associated with Auto Scaling groups for specified events
        /// </summary>
        public List<NotificationConfiguration> NotificationConfigurations
        {
            get
            {
                if (this.notificationsConfigurations == null)
                    this.notificationsConfigurations = new List<NotificationConfiguration>();
                return this.notificationsConfigurations;
            }
            set
            {
                if (this.notificationsConfigurations != value)
                {
                    this.notificationsConfigurations = value;
                    this.OnPropertyChanged("NotificationConfigurations");
                    this.OnPropertyChanged("Notifications");
                }
            }
        }

        /// <summary>
        /// String of notification actions associated with Auto Scaling groups for specified events
        /// </summary>
        public string Notifications
        {
            get
            {
                string notifications = string.Empty;
                foreach (NotificationConfiguration nc in this.NotificationConfigurations)
                {
                    notifications = string.Concat(notifications, !string.IsNullOrEmpty(notifications) ? ", " : "", nc.NotificationType);
                }
                return notifications;
            }
        }

    }
}
