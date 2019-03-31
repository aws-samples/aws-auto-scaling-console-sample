/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Auto Scaling console main window
*/

using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.ElasticLoadBalancing;
using Amazon.ElasticLoadBalancing.Model;
using AWS.AutoScale.Console.DataBinding;
using AWS.AutoScale.Console.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AWS.AutoScale.Console
{
    /// <summary>
    /// Interaction logic for ConsoleView.xaml
    /// </summary>
    public partial class ConsoleView : Page
    {
        System.Windows.Threading.DispatcherTimer monitorTimer;
        System.Windows.Threading.DispatcherTimer shutdownTimer;
        private BackgroundWorker loadworker;
        private BackgroundWorker vpcworker;

        ViewModel vm;

        #region "Constructor"

        /// <summary>
        /// Console View window constructor
        /// </summary>
        public ConsoleView()
        {
            InitializeComponent();

            vm = new ViewModel();
            this.DataContext = vm;
            rbClassic.Click += new RoutedEventHandler(rbEnvironment_Checked);
            rbVpc.Click += new RoutedEventHandler(rbEnvironment_Checked);
            cboVPC.SelectionChanged += new SelectionChangedEventHandler(cboVPC_SelectionChanged);
            vm.SelectedLaunchConfigurationChanged += new EventHandler(LaunchConfigurationSelectionChanged);
            vm.SelectedAutoScaleGroupChanged += new EventHandler(AutoScaleGroupSelectionChanged);
            Loaded += ConsoleView_Loaded;
        }

        #endregion

        #region "View Load Event"

        /// <summary>
        /// Auto Scaling Console load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConsoleView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AmazonEC2Client ec2Client = new AmazonEC2Client();
                DescribeRegionsRequest rreq = new DescribeRegionsRequest();
                DescribeRegionsResponse rresp = ec2Client.DescribeRegions(rreq);
                ((ViewModel)this.DataContext).Regions.Clear();
                foreach (Region r in rresp.DescribeRegionsResult.Region)
                {
                    ((ViewModel)this.DataContext).Regions.Add(new Models.Region(r.RegionName));
                }

                ((ViewModel)this.DataContext).Region = ((ViewModel)this.DataContext).Regions.Where(o => o.Name.ToLower() == "us-east-1").FirstOrDefault();

                AmazonAutoScalingClient client = GetAutoScaleClient();
                if (client == null)
                {
                    MessageBoxResult mbr = MessageBox.Show(Window.GetWindow(this), "AWS Credentials and region must be defined in app.config file.");
                    Application.Current.Shutdown();
                }

            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while setting credentials. Please ensure your credentials are correct in app.config.", "Error", MessageBoxButton.OK);
                Application.Current.Shutdown();
            }

        }

        #endregion

        #region "AWS Clients"

        /// <summary>
        /// Creates AWS auto scaling group client
        /// </summary>
        /// <returns>AmazonAutoScalingClient</returns>
        private AmazonAutoScalingClient GetAutoScaleClient()
        {
            if (vm.Region == null)
            {
                throw new InvalidRegionException("No region defined when creating auto scaling client");
            }

            AmazonAutoScalingConfig config = new AmazonAutoScalingConfig();
            config.ServiceURL = vm.Region.Url;

            AmazonAutoScalingClient client = new AmazonAutoScalingClient(config);

            return client;
        }

        /// <summary>
        /// Creates AWS EC2 client
        /// </summary>
        /// <returns>AmazonEC2Client</returns>
        private AmazonEC2Client GetEc2Client()
        {
            if (vm.Region == null)
            {
                throw new InvalidRegionException("No region defined when creating EC2 client");
            }

            AmazonEC2Config config = new AmazonEC2Config();
            config.ServiceURL = vm.Region.Ec2Url;

            AmazonEC2Client client = new AmazonEC2Client(config);

            return client;
        }

        /// <summary>
        /// Creates AWS Elastic Load Balancing client
        /// </summary>
        /// <returns>AmazonElasticLoadBalancingClient</returns>
        private AmazonElasticLoadBalancingClient GetElbClient()
        {
            if (vm.Region == null)
            {
                throw new InvalidRegionException("No region defined when creating elastic load balancing client");
            }

            AmazonElasticLoadBalancingConfig config = new AmazonElasticLoadBalancingConfig();
            config.ServiceURL = vm.Region.ElbUrl;

            AmazonElasticLoadBalancingClient client = new AmazonElasticLoadBalancingClient(config);

            return client;
        }

        #endregion

        #region "Regional Load"

        /// <summary>
        /// Event handler for cboRegion combo box selection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            initializeRegion();

        }

        /// <summary>
        /// Initializes contents of console based on region selected and EC2 classic/vpc
        /// </summary>
        private void initializeRegion()
        {
            try
            {

                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Verifying AWS Credentials, setting region and loading data...";


                if (vm.IsVpc)
                {
                    vpcworker = new BackgroundWorker();
                    vpcworker.DoWork += new DoWorkEventHandler(LoadAwsVpcDataContext);
                    vpcworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadAwsVpcDataContextDone);
                    vpcworker.RunWorkerAsync();
                }
                else
                {
                    vm.SelectedVpc = null;
                    InitiateAwsDataLoadBackgroundworker();
                }

                if (monitorTimer != null && monitorTimer.IsEnabled)
                    monitorTimer.Stop();

                monitorTimer = new System.Windows.Threading.DispatcherTimer();
                monitorTimer.Tick += new EventHandler(monitorTimer_Tick);
                monitorTimer.Interval = new TimeSpan(0, 0, 1);
                monitorTimer.Start();
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);

                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while initializing the region.", "Error", MessageBoxButton.OK);
            }

        }

        #endregion

        #region "Load AWS data"

        /// <summary>
        /// loads view model with AWS data based on region selected and EC2 classic/vpc in background
        /// </summary>
        private void InitiateAwsDataLoadBackgroundworker()
        {
            try
            {
                loadworker = new BackgroundWorker();
                loadworker.DoWork += new DoWorkEventHandler(LoadAwsData);
                loadworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadAwsDataDone);
                loadworker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                MessageBox.Show(Window.GetWindow(this), "Error occured while loading AWS data for specified region and ec2 enivronment. Please review error log for more details.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// loadworker DoWork event handler to load view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadAwsData(object sender, DoWorkEventArgs e)
        {
            try
            {
                AmazonEC2Client ec2Client = GetEc2Client();
                AmazonAutoScalingClient asClient = GetAutoScaleClient();
                AmazonElasticLoadBalancingClient elbClient = GetElbClient();

                LoadSecurityGroups(ec2Client);
                LoadLaunchConfigurations(asClient);
                LoadAvailabilityZones(ec2Client);
                LoadKeyPairs(ec2Client);
                LoadElasticLoadBalancers(elbClient);
                LoadAutoScalingGroups(asClient);
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                MessageBox.Show(Window.GetWindow(this), "Error occured while loading AWS data for specified region and ec2 enivronment. Please review error log for more details.", "Error", MessageBoxButton.OK);
            }

        }

        /// <summary>
        /// Load security groups to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadSecurityGroups(AmazonEC2Client ec2Client)
        {
            try
            {
                DescribeSecurityGroupsRequest sgreq = new DescribeSecurityGroupsRequest();
                DescribeSecurityGroupsResponse sgresp = ec2Client.DescribeSecurityGroups(sgreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.SecurityGroups.Clear();
                }));

                foreach (SecurityGroup sg in sgresp.DescribeSecurityGroupsResult.SecurityGroup)
                {
                    if (vm.IsVpc)
                    {
                        if (sg.VpcId != null && vm.SelectedVpc != null)
                        {
                            if (sg.VpcId == vm.SelectedVpc.VPC.VpcId)
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    vm.SecurityGroups.Add(new Models.ConsoleSG() { SecurityGroup = sg, DisplayName = string.Concat(sg.GroupName, " ( VPC: ", sg.VpcId, " )") });
                                }));
                            }
                        }
                    }
                    else
                    {
                        if (!(sg.VpcId != null && sg.VpcId != string.Empty && !vm.IsVpc))
                        {
                            //vm.SecurityGroups.Add(new Models.LcSecurityGroup() { SecurityGroup = sg, DisplayName = sg.GroupName });
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                vm.SecurityGroups.Add(new Models.ConsoleSG() { SecurityGroup = sg, DisplayName = sg.GroupName });
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading security groups for region and environment type");
            }
        }

        /// <summary>
        /// Load launch configurations to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadLaunchConfigurations(AmazonAutoScalingClient asClient)
        {
            try
            {
                DescribeLaunchConfigurationsRequest lcreq = new DescribeLaunchConfigurationsRequest();
                DescribeLaunchConfigurationsResponse lcresp = asClient.DescribeLaunchConfigurations(lcreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.LaunchConfigurations.Clear();
                }));
                foreach (LaunchConfiguration lcg in lcresp.DescribeLaunchConfigurationsResult.LaunchConfigurations)
                {
                    if (lcg.SecurityGroups.Count() == 0)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vm.LaunchConfigurations.Add(new Models.ConsoleLC() { LaunchConfiguration = lcg });
                        }));
                    }
                    else
                    {
                        foreach (Models.ConsoleSG lcsg in vm.SecurityGroups)
                        {
                            if (lcg.SecurityGroups.Contains(lcsg.SecurityGroup.GroupId))
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    vm.LaunchConfigurations.Add(new Models.ConsoleLC() { LaunchConfiguration = lcg });
                                }));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading launch configurations for region and environment type");
            }
        }

        /// <summary>
        /// Load availability zones to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadAvailabilityZones(AmazonEC2Client ec2Client)
        {
            try
            {

                DescribeAvailabilityZonesRequest azreq = new DescribeAvailabilityZonesRequest();
                DescribeAvailabilityZonesResponse azresp = ec2Client.DescribeAvailabilityZones(azreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.Zones.Clear();
                }));
                foreach (AvailabilityZone az in azresp.DescribeAvailabilityZonesResult.AvailabilityZone)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        vm.Zones.Add(az);
                    }));
                }
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading availability zones for region and environment type");
            }
        }

        /// <summary>
        /// Load key pairs to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadKeyPairs(AmazonEC2Client ec2Client)
        {
            try
            {
                DescribeKeyPairsRequest keyreq = new DescribeKeyPairsRequest();
                DescribeKeyPairsResponse keyresp = ec2Client.DescribeKeyPairs(keyreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.KeyPairs.Clear();
                }));
                foreach (KeyPair kp in keyresp.DescribeKeyPairsResult.KeyPair)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        vm.KeyPairs.Add(kp.KeyName);
                    }));
                }
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading key pairs for region and environment type");
            }
        }

        /// <summary>
        /// Load elastic load balancers to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadElasticLoadBalancers(AmazonElasticLoadBalancingClient elbClient)
        {
            try
            {
                DescribeLoadBalancersRequest elbreq = new DescribeLoadBalancersRequest();
                DescribeLoadBalancersResponse elbresp = elbClient.DescribeLoadBalancers(elbreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.LoadBalancers.Clear();
                }));
                foreach (LoadBalancerDescription lbd in elbresp.DescribeLoadBalancersResult.LoadBalancerDescriptions)
                {
                    if (vm.IsVpc)
                    {
                        if (lbd.VPCId != null && vm.SelectedVpc != null && lbd.VPCId == vm.SelectedVpc.VPC.VpcId)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                vm.LoadBalancers.Add(lbd);
                            }));
                        }
                    }
                    else
                    {
                        if (!(lbd.VPCId != null && lbd.VPCId != string.Empty && !vm.IsVpc))
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                vm.LoadBalancers.Add(lbd);
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading elastic load balancers for region and environment type");
            }
        }

        /// <summary>
        /// Load auto scaling groups to view model with AWS data based on region selected and EC2 classic/vpc
        /// </summary>
        private void LoadAutoScalingGroups(AmazonAutoScalingClient asClient)
        {
            try
            {
                DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                DescribeAutoScalingGroupsResponse asresp = asClient.DescribeAutoScalingGroups(asreq);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.AutoScalingGroups.Clear();
                }));
                foreach (AutoScalingGroup asg in asresp.DescribeAutoScalingGroupsResult.AutoScalingGroups)
                {
                    if (vm.IsVpc)
                    {
                        if (!string.IsNullOrEmpty(asg.VPCZoneIdentifier) && vm.SelectedVpc != null)
                        {
                            foreach (Models.ConsoleSubnet subnet in vm.SelectedVpc.Subnets)
                            {
                                if (asg.VPCZoneIdentifier.Contains(subnet.Subnet.SubnetId))
                                {
                                    //vm.AutoScalingGroups.Add(asg);
                                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                    {
                                        vm.AutoScalingGroups.Add(new Models.ConsoleASG() { AutoScalingGroup = asg });
                                    }));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(asg.VPCZoneIdentifier))
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                vm.AutoScalingGroups.Add(new Models.ConsoleASG() { AutoScalingGroup = asg });
                            }));
                        }
                    }
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    for (int i = 0; i < vm.AutoScalingGroups.Count(); i++)
                    {
                        DescribeNotificationConfigurationsRequest nreq = new DescribeNotificationConfigurationsRequest();
                        nreq.WithAutoScalingGroupNames(vm.AutoScalingGroups[i].AutoScalingGroup.AutoScalingGroupName);
                        DescribeNotificationConfigurationsResponse nresp = asClient.DescribeNotificationConfigurations(nreq);
                        vm.AutoScalingGroups[i].NotificationConfigurations = new List<NotificationConfiguration>();
                        foreach (NotificationConfiguration nc in nresp.DescribeNotificationConfigurationsResult.NotificationConfigurations)
                        {
                            vm.AutoScalingGroups[i].NotificationConfigurations.Add(nc);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);
                throw new DataLoadingException("Error occurred loading auto scaling groups for region and environment type");
            }
        }

        /// <summary>
        /// loadworker RunWorkerCompleted event handler to disable busy mode of console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadAwsDataDone(object sender, RunWorkerCompletedEventArgs e)
        {
            ((ViewModel)this.DataContext).IsBusy = false;
        }

        #endregion

        #region "Auto Scaling Group Informaiton Polling"

        /// <summary>
        /// Tick event handler for monitorTimer to poll AWS for updated information on instance information for auto scaling groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monitorTimer_Tick(object sender, EventArgs e)
        {
            if (vm.RefreshCountdown == 0)
            {
                vm.RefreshCountdown = vm.SelectedRefreshPeriod.Period; //vm.RefreshRate;
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                asreq.AutoScalingGroupNames.Add(((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName);
                IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "instances");
            }
            else
            {
                vm.RefreshCountdown -= 1;
            }
        }

        /// <summary>
        /// Callback function for DescribeAutoScalingGroups async call
        /// </summary>
        /// <param name="result"></param>
        private void AutoScalingGroupInfoCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DescribeAutoScalingGroupsResponse resp = client.EndDescribeAutoScalingGroups(result);
                List<AutoScalingGroup> asgroups = resp.DescribeAutoScalingGroupsResult.AutoScalingGroups;

                foreach (AutoScalingGroup asg in asgroups)
                {
                    Models.ConsoleASG _asg = vm.AutoScalingGroups.Where(o => o.AutoScalingGroup.AutoScalingGroupName == asg.AutoScalingGroupName).FirstOrDefault();
                    if (_asg != null)
                    {
                        if (result.AsyncState != null)
                        {
                            if (result.AsyncState.ToString() == "instances")
                            {
                                vm.AutoScalingGroups.Where(o => o.AutoScalingGroup.AutoScalingGroupName == asg.AutoScalingGroupName).First().AutoScalingGroup = asg;
                            }
                            else if (result.AsyncState.ToString().Contains("full"))
                            {
                                ((ViewModel)this.DataContext).SelectedAutoScalingGroup = new Models.ConsoleASG() { AutoScalingGroup = asg };
                            }
                        }
                    }

                }

                if (result.AsyncState != null)
                {
                    if (result.AsyncState.ToString().Contains("full"))
                    {
                        string cursel = vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                        vm.AutoScalingGroups.Clear();
                        foreach (AutoScalingGroup asg in asgroups)
                        {
                            if (result.AsyncState.ToString().Contains("delete") &&
                                asg.AutoScalingGroupName == cursel)
                            {
                                continue;
                            }

                            if (vm.IsVpc && vm.SelectedVpc != null)
                            {
                                if (!string.IsNullOrEmpty(asg.VPCZoneIdentifier))
                                {
                                    foreach (Models.ConsoleSubnet subnet in vm.SelectedVpc.Subnets)
                                    {
                                        if (asg.VPCZoneIdentifier.Contains(subnet.Subnet.SubnetId))
                                        {
                                            vm.AutoScalingGroups.Add(new Models.ConsoleASG() { AutoScalingGroup = asg });
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(asg.VPCZoneIdentifier))
                                {
                                    vm.AutoScalingGroups.Add(new Models.ConsoleASG() { AutoScalingGroup = asg });
                                }
                            }
                        }

                        if (result.AsyncState.ToString().Contains("new"))
                        {
                            vm.ASGroup = null;
                        }

                        if (result.AsyncState.ToString().Contains("delete"))
                        {
                            vm.SelectedAutoScalingGroup = null;
                        }

                        InitiateAwsDataLoadBackgroundworker();
                    }
                }

            }));
        }

        #endregion

        #region "Environment Selection"

        /// <summary>
        /// Event handler for rbEnvironment radio button checked. Sets whether console should read EC2 classic or a VPC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbEnvironment_Checked(object sender, RoutedEventArgs e)
        {
            vm.IsVpc = (sender as RadioButton).Tag.ToString() == "1";
            spVpc.Visibility = vm.IsVpc ? Visibility.Visible : Visibility.Collapsed;
            RadioButton rb = sender as RadioButton;
            ((ViewModel)this.DataContext).SelectedAutoScalingGroup = null;

            ((ViewModel)this.DataContext).SelectedLaunchConfiguration = null;

            vm.IsBusy = true;
            vm.BusyContent = "Loading environmental information for Classic EC2...";

            if (vm.IsVpc)
            {
                vpcworker = new BackgroundWorker();
                vpcworker.DoWork += new DoWorkEventHandler(LoadAwsVpcDataContext);
                vpcworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadAwsVpcDataContextDone);
                vpcworker.RunWorkerAsync();
            }
            else
            {
                vm.SelectedVpc = null;
                InitiateAwsDataLoadBackgroundworker();
            }


        }

        /// <summary>
        /// vpcworker DoWork event handler to load view model with selected VPC data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadAwsVpcDataContext(object sender, DoWorkEventArgs e)
        {
            AmazonEC2Client ec2Client = GetEc2Client();
            DescribeVpcsRequest vpcreq = new DescribeVpcsRequest();
            DescribeVpcsResponse vpcresp = ec2Client.DescribeVpcs(vpcreq);
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                vm.Vpcs.Clear();
            }));
            foreach (Vpc vpc in vpcresp.DescribeVpcsResult.Vpc)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    vm.Vpcs.Add(new Models.ConsoleVPC() { VPC = vpc });
                }));
            }

            DescribeSubnetsRequest sncreq = new DescribeSubnetsRequest();
            DescribeSubnetsResponse cnresp = ec2Client.DescribeSubnets(sncreq);
            foreach (Subnet subnet in cnresp.DescribeSubnetsResult.Subnet)
            {
                Models.ConsoleVPC vpc = vm.Vpcs.Where(o => o.VPC.VpcId == subnet.VpcId).FirstOrDefault();
                if (vpc != null)
                {
                    Models.ConsoleSubnet cs = vpc.Subnets.Where(o => o.Subnet == subnet).FirstOrDefault();
                    if (cs == null)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vpc.Subnets.Add(new Models.ConsoleSubnet() { DisplayName = string.Concat(subnet.SubnetId, " (", subnet.AvailabilityZone, ")"), Subnet = subnet });
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// loadworker Run Worker Completed event handler to refresh view model data in console based on VPC loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadAwsVpcDataContextDone(object sender, RunWorkerCompletedEventArgs e)
        {
            InitiateAwsDataLoadBackgroundworker();
            this.cboVPC.SelectedIndex = 0;
        }

        /// <summary>
        /// Event handler for cboVPC combobox selection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboVPC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedVpc != null)
            {
                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Loading data for the selected VPC...";

                ((ViewModel)this.DataContext).SelectedAutoScalingGroup = null;
                ((ViewModel)this.DataContext).SelectedLaunchConfiguration = null;

                InitiateAwsDataLoadBackgroundworker();
            }
        }

        #endregion

        #region "LC and ASG selection handlers"

        /// <summary>
        /// Event handler when launch configuration selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchConfigurationSelectionChanged(object sender, EventArgs e)
        {
            bool validConfig = ((ViewModel)this.DataContext).SelectedLaunchConfiguration != null && !string.IsNullOrEmpty((((ViewModel)this.DataContext).SelectedLaunchConfiguration.LaunchConfiguration.LaunchConfigurationName));
            gEmptySelectedLC.Visibility = !validConfig ? Visibility.Visible : Visibility.Hidden;
            gSelectedLC.Visibility = validConfig ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Event handler when auto scaling group selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoScaleGroupSelectionChanged(object sender, EventArgs e)
        {
            bool validConfig = vm.SelectedAutoScalingGroup != null && !string.IsNullOrEmpty(vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName);       
            gEmptySelectedASG.Visibility = !validConfig ? Visibility.Visible : Visibility.Hidden;
            gSelectedASG.Visibility = validConfig ? Visibility.Visible : Visibility.Hidden;
        }

        #endregion

        #region "Shutdown / Delete Auto Scaling Group"

        /// <summary>
        /// Event handler for "View Instances" context menu click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuViewAsgInstances(object sender, EventArgs e)
        {
            Window win = new InstancesWindow((ViewModel)this.DataContext);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        /// <summary>
        /// Event handler for "Shutdown Auto Scaling Group" context menu click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuShutdownAsg(object sender, EventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedAutoScalingGroup == null || ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName == null)
            {
                MessageBox.Show(Window.GetWindow(this), "Please select a Auto Scaling Group.", "Make Selection", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show(Window.GetWindow(this), string.Concat("Do you want to shutdown the Auto Scaling Group: ", ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName, "?"), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ((ViewModel)this.DataContext).IsBusy = true;
                ((ViewModel)this.DataContext).BusyContent = "Shuting down group. Waiting for all activities to complete.";
                AmazonAutoScalingClient client = GetAutoScaleClient();
                UpdateAutoScalingGroupRequest req = new UpdateAutoScalingGroupRequest();
                req.AutoScalingGroupName = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                req.MinSize = 0;
                req.MaxSize = 0;
                IAsyncResult result = client.BeginUpdateAutoScalingGroup(req, ShutdownAutoScaleGroupCallback, null);
            }
        }

        /// <summary>
        /// Event handler for "Delete Auto Scaling Group" context menu click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuDeleteAsg(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedAutoScalingGroup == null || vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName == null)
            {
                MessageBox.Show(Window.GetWindow(this), "Please select a Auto Scaling Group.", "Make Selection", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show(Window.GetWindow(this), string.Concat("Do you want to delete the Auto Scaling Group: ", vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName, "?"), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                vm.IsBusy = true;
                vm.BusyContent = "Shuting down group. Waiting for activities to complete.";
                AmazonAutoScalingClient client = GetAutoScaleClient();
                UpdateAutoScalingGroupRequest req = new UpdateAutoScalingGroupRequest();
                req.AutoScalingGroupName = vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                req.MinSize = 0;
                req.MaxSize = 0;
                IAsyncResult result = client.BeginUpdateAutoScalingGroup(req, ShutdownAutoScaleGroupCallback, "Delete");
            }
        }

        /// <summary>
        /// Callback function for BeginUpdateAutoScalinGroup async call to set min/max size of ASG to 0 "shutting down" the ASG
        /// </summary>
        /// <param name="result"></param>
        private void ShutdownAutoScaleGroupCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                UpdateAutoScalingGroupResponse resp = client.EndUpdateAutoScalingGroup(result);
                shutdownTimer = new System.Windows.Threading.DispatcherTimer();
                shutdownTimer.Tick += new EventHandler(shutdownTimer_Tick);
                shutdownTimer.Interval = new TimeSpan(0, 0, 5);
                shutdownTimer.Tag = result.AsyncState != null ? result.AsyncState.ToString() : null;
                shutdownTimer.Start();
            }));
        }

        /// <summary>
        /// Tick event handler for shutdownTimer to poll AWS for updated information on auto scaling group shutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shutdownTimer_Tick(object sender, EventArgs e)
        {
            AmazonAutoScalingClient client = GetAutoScaleClient();
            if (shutdownTimer.Tag != null)
            {
                if (shutdownTimer.Tag.ToString() == "activities")
                {
                    DescribeScalingActivitiesRequest req = new DescribeScalingActivitiesRequest();
                    req.AutoScalingGroupName = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                    IAsyncResult dasgresult = client.BeginDescribeScalingActivities(req, AutoScalingGroupScalingActivityInfoCallback, null);
                    return;
                }
            }

            DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
            IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupShutdownInfoCallback, shutdownTimer.Tag);
        }

        /// <summary>
        /// Callback function for BeginDescribeScalingActivities async call to monitor the activities of an ASG during shutdown
        /// </summary>
        /// <param name="result"></param>
        private void AutoScalingGroupScalingActivityInfoCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DescribeScalingActivitiesResponse resp = client.EndDescribeScalingActivities(result);

                foreach (Activity activity in resp.DescribeScalingActivitiesResult.Activities)
                {
                    if (activity.StatusCode.ToLower() != "successful" && activity.StatusCode.ToLower() != "failed")
                    {
                        return;
                    }
                }

                shutdownTimer.Stop();
                DeleteAutoScalingGroupRequest req = new DeleteAutoScalingGroupRequest();
                ((ViewModel)this.DataContext).BusyContent = "Deleting auto scaling group";
                req.AutoScalingGroupName = ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                IAsyncResult dasgresult = client.BeginDeleteAutoScalingGroup(req, DeleteAutoScalingGroupCallback, null);
            }));
        }


        /// <summary>
        /// Callback function for BeginDescribeAutoScalingGroups async call to monitor the shutdown of a ASG
        /// </summary>
        /// <param name="result"></param>
        private void AutoScalingGroupShutdownInfoCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DescribeAutoScalingGroupsResponse resp = client.EndDescribeAutoScalingGroups(result);
                List<AutoScalingGroup> asgroups = resp.DescribeAutoScalingGroupsResult.AutoScalingGroups;
                foreach (AutoScalingGroup asg in asgroups)
                {
                    if (asg.AutoScalingGroupName == ((ViewModel)this.DataContext).SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName)
                    {
                        if (asg.Instances.Count() == 0)
                        {
                            shutdownTimer.Stop();
                            if (result.AsyncState != null)
                            {
                                ((ViewModel)this.DataContext).BusyContent = "Scanning Auto Scaling Activities";
                                shutdownTimer.Tag = "activities";
                                shutdownTimer.Start();
                            }
                            else
                            {
                                DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                                IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "full");
                            }
                        }
                    }
                }

            }));
        }

        /// <summary>
        /// Callback function for BeginDeleteAutoScalingGroup async call to delete an ASG
        /// </summary>
        /// <param name="result"></param>
        private void DeleteAutoScalingGroupCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DeleteAutoScalingGroupResponse resp = client.EndDeleteAutoScalingGroup(result);
                DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "full,delete");

            }));
        }

        #endregion

        #region "Add Availability Group"

        /// <summary>
        /// Add auto scaling group click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddASG_Click(object sender, RoutedEventArgs e)
        {
            if (this.vm.IsVpc && this.vm.SelectedVpc == null)
            {
                MessageBox.Show("Currently there is no VPC selected. Please select a VPC and then try again.", "No Selected VPC", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Window win = new AsgWindow((ViewModel)this.DataContext, false);
            win.Owner = Window.GetWindow(this);
            ((AsgWindow)win).AsgAdded += new EventHandler(asgitem_added);
            win.ShowDialog();

        }

        /// <summary>
        /// Event handler when an Auto Scaling Group is added from the AsgWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void asgitem_added(object sender, EventArgs e)
        {
            try
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                CreateAutoScalingGroupRequest req = ((AWS.AutoScale.Console.AsgWindow)sender).ASGRequest;
                IAsyncResult result = client.BeginCreateAutoScalingGroup(req, CreateAutoScaleGroupCallback, null);
            }
            catch
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while creating auto scaling group. Please ensure your parameters are correct.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Callback function for BeginCreateAutoScalingGroup async call to create an ASG
        /// </summary>
        /// <param name="result"></param>
        private void CreateAutoScaleGroupCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    AmazonAutoScalingClient client = GetAutoScaleClient();
                    CreateAutoScalingGroupResponse resp = client.EndCreateAutoScalingGroup(result);
                    DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                    IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "full,new");
                }
                catch (Exception ex)
                {
                    ((ViewModel)this.DataContext).IsBusy = false;
                    MessageBox.Show(Window.GetWindow(this), "Error occured while creating auto scaling group.\n" + ex.Message, "Error", MessageBoxButton.OK);
                }

            }));
        }

        #endregion

        #region "Update Availability Group"

        /// <summary>
        /// Event handler for "Update Auto Scaling Group" context menu click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuUpdateAsg(object sender, RoutedEventArgs e)
        {
            Window win = new AsgWindow((ViewModel)this.DataContext, true);
            win.Owner = Window.GetWindow(this);
            ((AsgWindow)win).AsgUpdated += new EventHandler(asgitem_updated);
            win.ShowDialog();
        }

        /// <summary>
        /// Event handler when an Auto Scaling Group is updated from the AsgWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void asgitem_updated(object sender, EventArgs e)
        {
            try
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                UpdateAutoScalingGroupRequest req = ((AWS.AutoScale.Console.AsgWindow)sender).ASGUpdateRequest;
                IAsyncResult result = client.BeginUpdateAutoScalingGroup(req, UpdateAutoScaleGroupCallback, null);
            }
            catch (Exception ex)
            {
                LogManager.LogEntry(ex.Message);
                LogManager.LogEntry(ex.StackTrace);

                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while updating auto scaling group. Please ensure your parmaters are correct.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Callback function for BeginUpdateAutoScalingGroup async call to update an ASG based on user changes in AsgWindow
        /// </summary>
        /// <param name="result"></param>
        private void UpdateAutoScaleGroupCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                UpdateAutoScalingGroupResponse resp = client.EndUpdateAutoScalingGroup(result);

                if (vm.ASGroup.DesiredCapacity != vm.SelectedAutoScalingGroup.AutoScalingGroup.DesiredCapacity)
                {
                    if (vm.ASGroup.DesiredCapacity > vm.ASGroup.MaxSize)
                    {
                        vm.ASGroup.DesiredCapacity = vm.ASGroup.MaxSize;
                    }

                    if (vm.ASGroup.DesiredCapacity < vm.ASGroup.MinSize)
                    {
                        vm.ASGroup.DesiredCapacity = vm.ASGroup.MinSize;
                    }


                    if (((ViewModel)this.DataContext).ASGroup.DesiredCapacity != 0)
                    {
                        vm.IsBusy = true;
                        SetDesiredCapacityRequest sdcreq = new SetDesiredCapacityRequest();
                        sdcreq.AutoScalingGroupName = vm.SelectedAutoScalingGroup.AutoScalingGroup.AutoScalingGroupName;
                        sdcreq.DesiredCapacity = vm.ASGroup.DesiredCapacity;
                        IAsyncResult sdcresult = client.BeginSetDesiredCapacity(sdcreq, SetDesiredCapacityCallback, null);
                        return;
                    }

                    vm.ASGroup = new Models.AsgShell();

                    DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                    IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "full");

                }
                else
                {
                    vm.IsBusy = false;
                }

                vm.ASGroup = null;
            }));
        }

        /// <summary>
        /// Callback function for BeginSetDesiredCapacity async call to update an ASG desired capacity based on user changes in AsgWindow
        /// </summary>
        /// <param name="result"></param>
        private void SetDesiredCapacityCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                SetDesiredCapacityResponse resp = client.EndSetDesiredCapacity(result);
                DescribeAutoScalingGroupsRequest asreq = new DescribeAutoScalingGroupsRequest();
                IAsyncResult asresult = client.BeginDescribeAutoScalingGroups(asreq, AutoScalingGroupInfoCallback, "full");
                vm.IsBusy = false;
            }));
        }

        #endregion

        #region "Create Launch Config"

        /// <summary>
        /// Event handler for Add launch configuration button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddLC_Click(object sender, RoutedEventArgs e)
        {
            if (this.vm.IsVpc && this.vm.SelectedVpc == null)
            {
                MessageBox.Show("Currently there is no VPC selected. Please select a VPC and then try again.", "No Selected VPC", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Window win = new LcWindow((ViewModel)this.DataContext);
            win.Owner = Window.GetWindow(this);
            ((LcWindow)win).LcAdded += new EventHandler(lcitem_added);
            win.ShowDialog();
        }

        /// <summary>
        /// Event handler when a launch configuration is created from the LcWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lcitem_added(object sender, EventArgs e)
        {
            try
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                CreateLaunchConfigurationRequest req = ((AWS.AutoScale.Console.LcWindow)sender).LcRequest;
                IAsyncResult result = client.BeginCreateLaunchConfiguration(req, CreateLaunchConfigCallback, null);
            }
            catch
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while creating auto scaling group. Please ensure your parameters are correct.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Callback function for BeginCreateLaunchConfiguration async call to create a Launch configuration based on user input from LcWindow
        /// </summary>
        /// <param name="result"></param>
        private void CreateLaunchConfigCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                InitiateAwsDataLoadBackgroundworker();
            }));
        }

        #endregion

        #region "Delete Launch Config"

        /// <summary>
        /// Event handler for "Delete Launch Configuration" context menu click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuDeleteLc(object sender, EventArgs e)
        {
            if (vm.SelectedLaunchConfiguration == null || vm.SelectedLaunchConfiguration.LaunchConfiguration.LaunchConfigurationName == null)
            {
                MessageBox.Show(Window.GetWindow(this), "Please select a Launch Configuration.", "Make Selection", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show(Window.GetWindow(this), string.Concat("Do you want to delete the Launch Configuration: ", vm.SelectedLaunchConfiguration.LaunchConfiguration.LaunchConfigurationName, "?"), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                vm.IsBusy = true;
                vm.BusyContent = "Deleting Launch Configuration";
                AmazonAutoScalingClient client = GetAutoScaleClient();
                DeleteLaunchConfigurationRequest req = new DeleteLaunchConfigurationRequest();
                req.LaunchConfigurationName = vm.SelectedLaunchConfiguration.LaunchConfiguration.LaunchConfigurationName;
                IAsyncResult result = client.BeginDeleteLaunchConfiguration(req, DeleteLaunchConfigCallback, vm.SelectedLaunchConfiguration.LaunchConfiguration.LaunchConfigurationName);
            }

        }

        /// <summary>
        /// Callback function for DeleteLaunchConfigurationRequest async call to delete a Launch configuration
        /// </summary>
        /// <param name="result"></param>
        private void DeleteLaunchConfigCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                InitiateAwsDataLoadBackgroundworker();
            }));
        }

        #endregion

        #region "Create Notification"

        /// <summary>
        /// Event handler for "Add Notification" context menu click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuAddNotification(object sender, RoutedEventArgs e)
        {
            Window win = new NcWindow((ViewModel)this.DataContext);
            win.Owner = Window.GetWindow(this);
            ((NcWindow)win).NcAdded += new EventHandler(ncitem_added);
            win.ShowDialog();
        }

        /// <summary>
        /// Event handler when a notification configuration is created from the NcWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ncitem_added(object sender, EventArgs e)
        {
            try
            {
                AmazonAutoScalingClient client = GetAutoScaleClient();
                PutNotificationConfigurationRequest req = ((NcWindow)sender).PutNcRequest;
                IAsyncResult result = client.BeginPutNotificationConfiguration(req, PutNotificationConfigCallback, null);
            }
            catch
            {
                ((ViewModel)this.DataContext).IsBusy = false;
                MessageBox.Show(Window.GetWindow(this), "Error occured while adding notification configuration.", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Callback function for BeginPutNotificationConfiguration async call to put a notification configuration with a ASG
        /// </summary>
        /// <param name="result"></param>
        private void PutNotificationConfigCallback(IAsyncResult result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                InitiateAwsDataLoadBackgroundworker();
            }));
        }

        #endregion

        /// <summary>
        /// Close button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseConfig_Click(object sender, RoutedEventArgs e)
        {
            reConfiguration.IsExpanded = false;
        }

    }
}
