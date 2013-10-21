/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
 *  
 * Description: Create/Update Auto Scaling Group Window
*/

using Amazon.AutoScaling.Model;
using Amazon.EC2.Model;
using AWS.AutoScale.Console.DataBinding;
using AWS.AutoScale.Console.Models;
using AWS.AutoScale.Console.Utility;
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
    public partial class AsgWindow : Window
    {
        private bool edit = false;
        private bool isVpc = false;
        private CreateAutoScalingGroupRequest asgrequest;
        private UpdateAutoScalingGroupRequest asgupdaterequest;

        public event EventHandler AsgAdded;
        public event EventHandler AsgUpdated;

        /// <summary>
        /// AWS Auto Scaling Group Creation Request object created from data entered in this window
        /// </summary>
        public CreateAutoScalingGroupRequest ASGRequest
        {
            get
            {
                if (this.asgrequest == null)
                    this.asgrequest = new CreateAutoScalingGroupRequest();
                return this.asgrequest;
            }

            set
            {
                if (this.asgrequest != value)
                {
                    this.asgrequest = value;
                }
            }
        }

        /// <summary>
        /// AWS Auto Scaling Group Update Request object created from data updated in this window
        /// </summary>
        public UpdateAutoScalingGroupRequest ASGUpdateRequest
        {
            get
            {
                if (this.asgupdaterequest == null)
                    this.asgupdaterequest = new UpdateAutoScalingGroupRequest();
                return this.asgupdaterequest;
            }

            set
            {
                if (this.asgupdaterequest != value)
                {
                    this.asgupdaterequest = value;
                }
            }
        }

        /// <summary>
        /// Event bubbler for auto scaling group being added
        /// </summary>
        protected void OnAsgAdded()
        {
            if (this.AsgAdded != null)
            {
                this.AsgAdded(this, EventArgs.Empty);
                this.Close();
            }
        }

        /// <summary>
        /// Event bubbler for auto scaling group being updated
        /// </summary>
        protected void OnAsgUpdated()
        {
            if (this.AsgUpdated != null)
            {
                this.AsgUpdated(this, EventArgs.Empty);
                this.Close();
            }
        }

        /// <summary>
        /// Auto Scaling Group Window constructor
        /// </summary>
        public AsgWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Auto Scaling Group Window constructor
        /// </summary>
        /// <param name="model">Master View Model for ASG console</param>
        /// <param name="editMode">Edit mode true = update mode; false = create mode</param>
        public AsgWindow(ViewModel model, bool editMode)
            : this()
        {
            InitializeComponent();
            this.DataContext = model;
            this.edit = editMode;
            this.isVpc = model.IsVpc;

            Loaded += AsgWindow_Loaded;
        }

        /// <summary>
        /// Auto Scaling group window loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsgWindow_Loaded(object sender, RoutedEventArgs e)
        {
            spAsgAZ.Visibility = this.isVpc ? Visibility.Collapsed : Visibility.Visible;
            spAsgSubnets.Visibility = this.isVpc ? Visibility.Visible : Visibility.Collapsed;
            spAsgAZEdit.Visibility = this.isVpc ? Visibility.Collapsed : Visibility.Visible;
            spAsgSubnetsEdit.Visibility = this.isVpc ? Visibility.Visible : Visibility.Collapsed;
            grAddAsGroup.Visibility = this.edit ? Visibility.Hidden : Visibility.Visible;
            grUpdateAsGroup.Visibility = this.edit ? Visibility.Visible : Visibility.Hidden;
            rMode.Text = this.edit ? "/ Edit" : "/ New";

            if (((ViewModel)this.DataContext).IsVpc)
            {
                rlbAsSubnets.ItemsSource = ((ViewModel)this.DataContext).SelectedVpc.Subnets;
                rlbAsSubnetsEdit.ItemsSource = ((ViewModel)this.DataContext).SelectedVpc.Subnets;
            }

            if (this.edit)
            {
                ((ViewModel)this.DataContext).ASGroup = new Models.AsgShell();
                ((ViewModel)this.DataContext).ASGroup.Name = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                ((ViewModel)this.DataContext).ASGroup.MaxSize = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.MaxSize;
                ((ViewModel)this.DataContext).ASGroup.MinSize = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.MinSize;
                Models.ConsoleLC lc = ((ViewModel)this.DataContext).LaunchConfigurations.Where(o => o.LaunchConfiguration.LaunchConfigurationName == ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.LaunchConfigurationName).FirstOrDefault();
                if (lc != null)
                {
                    ((ViewModel)this.DataContext).ASGroup.LaunchConfiguration = lc;
                } 
                
                if (this.isVpc)
                {
                    foreach (ConsoleSubnet subnet in rlbAsSubnetsEdit.Items)
                    {
                        if(((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.VPCZoneIdentifier.Contains(subnet.Subnet.SubnetId))
                        {
                            rlbAsSubnetsEdit.SelectedItems.Add(subnet);
                        }
                    }
                }
                else
                {
                    foreach (AvailabilityZone az in rlbAsZonesEdit.Items)
                    {
                        string saz = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AvailabilityZones.Where(o => o == az.ZoneName).FirstOrDefault();
                        if (!string.IsNullOrEmpty(saz))
                        {
                            rlbAsZonesEdit.SelectedItems.Add(az);
                        }
                    }
                }

                ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AvailabilityZones.First();
                ((ViewModel)this.DataContext).ASGroup.Cooldown = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.DefaultCooldown;
                ((ViewModel)this.DataContext).ASGroup.GracePeriod = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.HealthCheckGracePeriod;
                ((ViewModel)this.DataContext).ASGroup.DesiredCapacity = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.DesiredCapacity;

            }
        }

        /// <summary>
        /// Close button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ((ViewModel)this.DataContext).ASGroup = null;
            this.Close();
        }

        /// <summary>
        /// Create button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCreateAG_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Creating Auto Scaling Group";
                if (!((ViewModel)this.DataContext).ASGroup.IsValid())
                {
                    string msg = "Please ensure your parameters are correct.\n";
                    foreach (KeyValuePair<String, String> err in ((ViewModel)this.DataContext).ASGroup.Errors)
                    {
                        msg += string.Concat(err.Value, "\n");
                    }
                    throw new InvalidParametersException(msg);
                }

                this.ASGRequest
                    .WithAutoScalingGroupName(((ViewModel)this.DataContext).ASGroup.Name)
                    .WithLaunchConfigurationName(((ViewModel)this.DataContext).ASGroup.LaunchConfiguration.LaunchConfiguration.LaunchConfigurationName)
                    .WithMaxSize(((ViewModel)this.DataContext).ASGroup.MaxSize)
                    .WithMinSize(((ViewModel)this.DataContext).ASGroup.MinSize)
                    .WithHealthCheckGracePeriod(((ViewModel)this.DataContext).ASGroup.GracePeriod)
                    .WithDefaultCooldown(((ViewModel)this.DataContext).ASGroup.Cooldown);

                if (((ViewModel)this.DataContext).IsVpc)
                {
                    if (((ViewModel)this.DataContext).ASGroup.LoadBalancer != null)
                    {
                        foreach (Models.ConsoleSubnet subnet in rlbAsSubnets.SelectedItems)
                        {
                            if (((ViewModel)this.DataContext).ASGroup.LoadBalancer.Subnets.Where(o => o.ToString() == subnet.Subnet.SubnetId).Count() == 0)
                            {
                                MessageBox.Show(Window.GetWindow(this), string.Concat("The subnet ", subnet.Subnet.SubnetId, " is not defined in the selected ELB"), "Make Selection", MessageBoxButton.OK);
                                ((ViewModel)this.DataContext).IsBusy = false;
                                return;
                            }
                        }
                        this.ASGRequest.LoadBalancerNames.Add(((ViewModel)this.DataContext).ASGroup.LoadBalancer.LoadBalancerName);
                    }

                    foreach (Models.ConsoleSubnet subnet in rlbAsSubnets.SelectedItems)
                    {
                        this.ASGRequest.VPCZoneIdentifier += string.Concat(!string.IsNullOrEmpty(this.ASGRequest.VPCZoneIdentifier) ? "," : "", subnet.Subnet.SubnetId);
                    }
                }
                else
                {
                    if (((ViewModel)this.DataContext).ASGroup.LoadBalancer != null)
                    {
                        foreach (AvailabilityZone az in rlbAsZones.SelectedItems)
                        {
                            if (((ViewModel)this.DataContext).ASGroup.LoadBalancer.AvailabilityZones.Where(o => o.ToString() == az.ZoneName).Count() == 0)
                            {
                                MessageBox.Show(Window.GetWindow(this), string.Concat("The availablility zone ", az.ZoneName, " is not defined in the selected ELB"), "Make Selection", MessageBoxButton.OK);
                                ((ViewModel)this.DataContext).IsBusy = false;
                                return;
                            }
                        }
                        this.ASGRequest.LoadBalancerNames.Add(((ViewModel)this.DataContext).ASGroup.LoadBalancer.LoadBalancerName);
                    }

                    foreach (AvailabilityZone az in rlbAsZones.SelectedItems)
                    {
                        this.ASGRequest.AvailabilityZones.Add(az.ZoneName);
                    }
                }

                if (((ViewModel)this.DataContext).ASGroup.DesiredCapacity > ((ViewModel)this.DataContext).ASGroup.MaxSize)
                {
                    ((ViewModel)this.DataContext).ASGroup.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.MaxSize;
                }

                if (((ViewModel)this.DataContext).ASGroup.DesiredCapacity < ((ViewModel)this.DataContext).ASGroup.MinSize)
                {
                    ((ViewModel)this.DataContext).ASGroup.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.MinSize;
                }
                this.ASGRequest.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.DesiredCapacity;

                this.OnAsgAdded();

                ((ViewModel)this.DataContext).ASGroup = null;
            }
            catch (InvalidParametersException ex)
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), ex.Message, "Invalid Parameters", MessageBoxButton.OK);
            }
            catch(Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while creating auto scaling group. Please ensure your parameters are correct.", "Error", MessageBoxButton.OK);
            }

        }

        /// <summary>
        /// Cancel button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCancelAG_Click(object sender, RoutedEventArgs e)
        {
            ((ViewModel)this.DataContext).ASGroup = null;
            this.Close();
        }

        /// <summary>
        /// Update button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbUpdateAG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Updating Auto Scaling Group";
                if (!((ViewModel)this.DataContext).ASGroup.IsValid())
                {
                    string msg = "Please ensure your parameters are correct.\n";
                    foreach (KeyValuePair<String, String> err in ((ViewModel)this.DataContext).ASGroup.Errors)
                    {
                        msg += string.Concat(err.Value, "\n");
                    }
                    throw new InvalidParametersException(msg);
                }

                this.ASGUpdateRequest
                    .WithAutoScalingGroupName(((ViewModel)this.DataContext).ASGroup.Name)
                    .WithLaunchConfigurationName(((ViewModel)this.DataContext).ASGroup.LaunchConfiguration.LaunchConfiguration.LaunchConfigurationName)
                    .WithMinSize(((ViewModel)this.DataContext).ASGroup.MinSize)
                    .WithMaxSize(((ViewModel)this.DataContext).ASGroup.MaxSize)
                    .WithDefaultCooldown(((ViewModel)this.DataContext).ASGroup.Cooldown)
                    .WithHealthCheckGracePeriod(((ViewModel)this.DataContext).ASGroup.GracePeriod);

                if (((ViewModel)this.DataContext).IsVpc)
                {
                    if (((ViewModel)this.DataContext).ASGroup.LoadBalancer != null)
                    {
                        foreach (Models.ConsoleSubnet subnet in rlbAsSubnetsEdit.SelectedItems)
                        {
                            if (((ViewModel)this.DataContext).ASGroup.LoadBalancer.Subnets.Where(o => o.ToString() == subnet.Subnet.SubnetId).Count() == 0)
                            {
                                MessageBox.Show(Window.GetWindow(this), string.Concat("The subnet ", subnet.Subnet.SubnetId, " is not defined in the selected ELB"), "Make Selection", MessageBoxButton.OK);
                                ((ViewModel)this.DataContext).IsBusy = false;
                                return;
                            }
                        }
                    }

                    foreach (Models.ConsoleSubnet subnet in rlbAsSubnetsEdit.SelectedItems)
                    {
                        this.ASGUpdateRequest.VPCZoneIdentifier += string.Concat(!string.IsNullOrEmpty(this.ASGUpdateRequest.VPCZoneIdentifier) ? "," : "", subnet.Subnet.SubnetId);
                    }
                }
                else
                {
                    if (((ViewModel)this.DataContext).ASGroup.LoadBalancer != null)
                    {
                        foreach (AvailabilityZone az in rlbAsZonesEdit.SelectedItems)
                        {
                            if (((ViewModel)this.DataContext).ASGroup.LoadBalancer.AvailabilityZones.Where(o => o.ToString() == az.ZoneName).Count() == 0)
                            {
                                MessageBox.Show(Window.GetWindow(this), string.Concat("The availablility zone ", az.ZoneName, " is not defined in the selected ELB"), "Make Selection", MessageBoxButton.OK);
                                ((ViewModel)this.DataContext).IsBusy = false;
                                return;
                            }
                        }
                    }

                    foreach (AvailabilityZone az in rlbAsZonesEdit.SelectedItems)
                    {
                        this.ASGUpdateRequest.AvailabilityZones.Add(az.ZoneName);
                    }
                }


                if (((ViewModel)this.DataContext).ASGroup.DesiredCapacity > ((ViewModel)this.DataContext).ASGroup.MaxSize)
                {
                    ((ViewModel)this.DataContext).ASGroup.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.MaxSize;
                }

                if (((ViewModel)this.DataContext).ASGroup.DesiredCapacity < ((ViewModel)this.DataContext).ASGroup.MinSize)
                {
                    ((ViewModel)this.DataContext).ASGroup.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.MinSize;
                }
                this.ASGRequest.DesiredCapacity = ((ViewModel)this.DataContext).ASGroup.DesiredCapacity;

                this.OnAsgUpdated();

                //((ViewModel)this.DataContext).ASGroup = null;
            }
            catch (InvalidParametersException ex)
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), ex.Message, "Invalid Parameters", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);

                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while creating auto scaling group. Please ensure your parameters are correct.", "Error", MessageBoxButton.OK);
            }
        }

    }
}
