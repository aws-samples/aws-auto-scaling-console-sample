/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
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
