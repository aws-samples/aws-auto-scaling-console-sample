/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Launch Configuration Window
*/

using Amazon.AutoScaling.Model;
using Amazon.EC2.Model;
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
    public partial class LcWindow : Window
    {
        private CreateLaunchConfigurationRequest lcrequest;
        public event EventHandler LcAdded;

        /// <summary>
        /// AWS Launch Configuration Creation Request object created from data entered in this window
        /// </summary>
        public CreateLaunchConfigurationRequest LcRequest
        {
            get
            {
                if (this.lcrequest == null)
                    this.lcrequest = new CreateLaunchConfigurationRequest();
                return this.lcrequest;
            }

            set
            {
                if (this.lcrequest != value)
                {
                    this.lcrequest = value;
                }
            }
        }

        /// <summary>
        /// Event bubbler upon creation of launch configuration on this window
        /// </summary>
        protected void OnLcAdded()
        {
            if (this.LcAdded != null)
            {
                this.LcAdded(this, EventArgs.Empty);
                this.Close();
            }
        }


        /// <summary>
        /// Launch configuration window constructor
        /// </summary>
        public LcWindow()
        {
            InitializeComponent();
            //this.DataContext = this.Model;
        }

        /// <summary>
        /// Launch configuration window constructor
        /// </summary>
        /// <param name="model">Master View Model for ASG console</param>
        public LcWindow(ViewModel model)
            : this()
        {
            this.DataContext = model;
        }

        /// <summary>
        /// Close Button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Create button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCreateLC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Creating Launch Configuration";
                if (!((ViewModel)this.DataContext).LaunchConfiguration.IsValid())
                {
                    string msg = "Please ensure your parameters are correct.\n";
                    foreach (KeyValuePair<String, String> err in ((ViewModel)this.DataContext).LaunchConfiguration.Errors)
                    {
                        msg += string.Concat(err.Value, "\n");
                    }
                    throw new InvalidParametersException(msg);
                }

                this.LcRequest
                    .WithImageId(((ViewModel)this.DataContext).LaunchConfiguration.Ami)
                    .WithInstanceType(((ViewModel)this.DataContext).LaunchConfiguration.InstanceType.Name)
                    .WithLaunchConfigurationName(((ViewModel)this.DataContext).LaunchConfiguration.Name)
                    .WithKeyName(((ViewModel)this.DataContext).LaunchConfiguration.Key);

                foreach (Models.ConsoleSG sg in this.rlbLcSecurityGroups.SelectedItems)
                {
                    this.LcRequest.SecurityGroups.Add(sg.SecurityGroup.GroupId);
                }

                ((ViewModel)this.DataContext).LaunchConfiguration = null;

                this.OnLcAdded();
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
        /// Cancel button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCancelLC_Click(object sender, RoutedEventArgs e)
        {
            ((ViewModel)this.DataContext).ASGroup = null;
            this.Close();
        }

    }
}
