/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Notification Configuration Window to create notifications on an ASG
*/

using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Amazon.EC2.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS.AutoScale.Console.DataBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AWS.AutoScale.Console
{
    /// <summary>
    /// Interaction logic for InstancesWindow.xaml
    /// </summary>
    public partial class NcWindow : Window
    {
        private PutNotificationConfigurationRequest pncrequest;
        public event EventHandler NcAdded;

        private List<string> notificationTypes = new List<string>();
        public List<string> snstopics = new List<string>();

        /// <summary>
        /// AWS Notification Configuration Put Request object created from data entered in this window
        /// </summary>
        public PutNotificationConfigurationRequest PutNcRequest
        {
            get
            {
                if (this.pncrequest == null)
                    this.pncrequest = new PutNotificationConfigurationRequest();
                return this.pncrequest;
            }

            set
            {
                if (this.pncrequest != value)
                {
                    this.pncrequest = value;
                }
            }
        }


        /// <summary>
        /// Event bubbler when notification configuration added
        /// </summary>
        protected void OnNcAdded()
        {
            if (this.NcAdded != null)
            {
                this.NcAdded(this, EventArgs.Empty);
                this.Close();
            }
        }


        /// <summary>
        /// Notification configuration window constructor
        /// </summary>
        public NcWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Notification configuration window constructor
        /// </summary>
        /// <param name="model">Notification configuration window constructor</param>
        public NcWindow(ViewModel model)
            : this()
        {
            this.DataContext = model;
            Loaded += NcWindow_Loaded;
        }

        /// <summary>
        /// Notification configuration window load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NcWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AmazonAutoScalingConfig config = new AmazonAutoScalingConfig();
                config.ServiceURL = ((ViewModel)this.DataContext).Region.Url;
                AmazonAutoScalingClient client = new AmazonAutoScalingClient(config);
                DescribeAutoScalingNotificationTypesRequest dasntreq = new DescribeAutoScalingNotificationTypesRequest();
                DescribeAutoScalingNotificationTypesResponse dasntresp = client.DescribeAutoScalingNotificationTypes(dasntreq);
                foreach (string asnt in dasntresp.DescribeAutoScalingNotificationTypesResult.AutoScalingNotificationTypes)
                {
                    this.notificationTypes.Add(asnt);
                }

                AmazonSimpleNotificationServiceConfig snsconfig = new AmazonSimpleNotificationServiceConfig();
                config.ServiceURL = ((ViewModel)this.DataContext).Region.Url;
                AmazonSimpleNotificationServiceClient snsclient = new AmazonSimpleNotificationServiceClient(snsconfig);
                ListTopicsRequest ltrequest = new ListTopicsRequest();
                ListTopicsResponse ltresp = snsclient.ListTopics(ltrequest);
                foreach (Topic topic in ltresp.ListTopicsResult.Topics)
                {
                    this.snstopics.Add(topic.TopicArn);
                }

                rlbNcTypes.ItemsSource = this.notificationTypes;
                cboTopics.ItemsSource = this.snstopics;
            }
            catch
            {
                MessageBox.Show(Window.GetWindow(this), "Error occured while loading the notification configuration options.", "Error", MessageBoxButton.OK);
                this.Close();
            }
        }

        /// <summary>
        /// Event handler for close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for Create Notification Configuration button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCreateNC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Adding Notification Configuration";

                this.PutNcRequest
                    .WithAutoScalingGroupName(((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName)
                    .WithTopicARN(cboTopics.SelectedItem.ToString());

                foreach (string nt in rlbNcTypes.SelectedItems)
                {
                    this.PutNcRequest.NotificationTypes.Add(nt);
                }

                this.OnNcAdded();
            }
            catch (InvalidParametersException ex)
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), ex.Message, "Invalid Parameters", MessageBoxButton.OK);
            }
            catch
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while creating launch configuration. Please ensure your parameters are correct.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Event handler for Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCancelNC_Click(object sender, RoutedEventArgs e)
        {
            ((ViewModel)this.DataContext).ASGroup = null;
            this.Close();
        }

    }
}
