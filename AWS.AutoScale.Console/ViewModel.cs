/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: View Model for application providing data available for application views
*/

using Amazon.AutoScaling.Model;
using AWS.AutoScale.Console.Models;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.ElasticLoadBalancing.Model;
using EC2 = Amazon.EC2.Model;
using AS = Amazon.AutoScaling.Model;

namespace AWS.AutoScale.Console.DataBinding
{
    public class ViewModel : NotifyPropertyChangeBase
    {
        private Region region;
        private ConsoleLC selectedLaunchConfiguration;
        private LcShell launchConfiguration;
        private EC2.AvailabilityZone selectedZone;
        private ConsoleASG selectedAutoScalingGroup;
        private RefreshPeriod selectedRefreshPeriod;
        private AsgShell asGroup;
        private ConsoleVPC selectedVpc;
        private int refreshRate = 5;
        private int refreshCountdown = 0;
        private bool isVpc;
        private ObservableCollection<Region> regions;
        private ObservableCollection<EC2.AvailabilityZone> zones;
        private ObservableCollection<InstanceType> instanceTypes;
        private ObservableCollection<RefreshPeriod> refreshPeriods;
        private ObservableCollection<ConsoleLC> launchConfigurations;
        private ObservableCollection<ConsoleASG> autoScalingGroups;
        private ObservableCollection<ConsoleSG> securityGroups;
        private ObservableCollection<ConsoleVPC> vpcs;
        private ObservableCollection<string> keyPairs;
        private ObservableCollection<LoadBalancerDescription> loadBalancers;
        private ObservableCollection<AS.Instance> runningInstances;
        private bool isBusy;
        private string busyContent;

        public event EventHandler SelectedAutoScaleGroupChanged;
        public event EventHandler SelectedLaunchConfigurationChanged;


        public Region Region
        {
            get
            {
                return this.region;
            }

            set
            {
                this.region = value;
                this.OnPropertyChanged("Region");
                this.OnPropertyChanged("EnvironmentDescription");
            }
        }

        public string EnvironmentDescription
        {
            get
            {
                string desc = string.Empty;

                if (this.region != null)
                {
                    desc = string.Concat(this.region.Name, " | ", this.Environment);
                }

                return desc;
            }
        }

        public string Environment
        {
            get
            {
                string env = string.Empty;
                if (this.isVpc)
                {
                    env = string.Concat("VPC: ", this.selectedVpc != null ? this.selectedVpc.VPC.VpcId : "Not Selected");
                }
                else
                {
                    env = "Classic EC2";
                }

                return env;
            }
        }

        public ObservableCollection<Region> Regions
        {
            get
            {
                if (regions == null)
                {
                    regions = new ObservableCollection<Region>();
                }
                return regions;
            }

            set
            {
                if (this.regions != value)
                {
                    this.regions = value;
                    this.OnPropertyChanged("Regions");
                }
            }
        }

        public ObservableCollection<ConsoleVPC> Vpcs
        {
            get
            {
                if (vpcs == null)
                {
                    vpcs = new ObservableCollection<ConsoleVPC>();
                }
                return vpcs;
            }

            set
            {
                if (this.vpcs != value)
                {
                    this.vpcs = value;
                    this.OnPropertyChanged("Vpcs");
                }
            }
        }

        public ObservableCollection<EC2.AvailabilityZone> Zones
        {
            get
            {
                if (this.zones == null)
                {
                    this.zones = new ObservableCollection<EC2.AvailabilityZone>();
                }
                return this.zones;
            }

            set
            {
                if (this.zones != value)
                {
                    this.zones = value;
                    this.OnPropertyChanged("Zones");
                }
            }
        }

        public ObservableCollection<ConsoleASG> AutoScalingGroups
        {
            get
            {
                if (this.autoScalingGroups == null)
                {
                    this.autoScalingGroups = new ObservableCollection<ConsoleASG>();
                }
                return this.autoScalingGroups;
            }

            set
            {
                if (this.autoScalingGroups != value)
                {
                    this.autoScalingGroups = value;
                    this.OnPropertyChanged("AutoScalingGroups");
                }
            }
        }

        public ObservableCollection<RefreshPeriod> RefreshPeriods
        {
            get
            {
                if (refreshPeriods == null)
                {
                    refreshPeriods = new ObservableCollection<RefreshPeriod>();
                    refreshPeriods.Add(new RefreshPeriod("5 seconds", 5));
                    refreshPeriods.Add(new RefreshPeriod("10 seconds", 10));
                    refreshPeriods.Add(new RefreshPeriod("15 seconds", 15));
                    refreshPeriods.Add(new RefreshPeriod("30 seconds", 30));
                    refreshPeriods.Add(new RefreshPeriod("45 seconds", 45));
                    refreshPeriods.Add(new RefreshPeriod("60 seconds", 60));
                }
                return refreshPeriods;
            }
        }

        public ObservableCollection<InstanceType> InstanceTypes
        {
            get
            {
                if (instanceTypes == null)
                {
                    instanceTypes = new ObservableCollection<InstanceType>();
                    instanceTypes.Add(new InstanceType("t1.micro"));
                    instanceTypes.Add(new InstanceType("m1.small"));
                    instanceTypes.Add(new InstanceType("m1.medium"));
                    instanceTypes.Add(new InstanceType("m1.large"));
                    instanceTypes.Add(new InstanceType("m1.xlarge"));
                    instanceTypes.Add(new InstanceType("m3.xlarge"));
                    instanceTypes.Add(new InstanceType("m3.2xlarge"));
                    instanceTypes.Add(new InstanceType("m2.xlarge"));
                    instanceTypes.Add(new InstanceType("m2.2xlarge"));
                    instanceTypes.Add(new InstanceType("m2.4xlarge"));
                    instanceTypes.Add(new InstanceType("c1.medium"));
                    instanceTypes.Add(new InstanceType("c1.xlarge"));
                    instanceTypes.Add(new InstanceType("hs1.8xlarge"));
                }
                return instanceTypes;
            }
        }

        public ObservableCollection<ConsoleLC> LaunchConfigurations
        {
            get
            {
                if (this.launchConfigurations == null)
                {
                    this.launchConfigurations = new ObservableCollection<ConsoleLC>();
                }
                return this.launchConfigurations;
            }

            set
            {
                if (this.launchConfigurations != value)
                {
                    this.launchConfigurations = value;
                    this.OnPropertyChanged("LaunchConfigurations");
                }
            }
        }

        public ObservableCollection<ConsoleSG> SecurityGroups
        {
            get
            {
                if (this.securityGroups == null)
                {
                    this.securityGroups = new ObservableCollection<ConsoleSG>();
                }
                return this.securityGroups;
            }

            set
            {
                if (this.securityGroups != value)
                {
                    this.securityGroups = value;
                    this.OnPropertyChanged("SecurityGroups");
                }
            }
        }

        public ObservableCollection<string> KeyPairs
        {
            get
            {
                if (this.keyPairs == null)
                {
                    this.keyPairs = new ObservableCollection<string>();
                }
                return this.keyPairs;
            }

            set
            {
                if (this.keyPairs != value)
                {
                    this.keyPairs = value;
                    this.OnPropertyChanged("KeyPairs");
                }
            }
        }

        public ObservableCollection<LoadBalancerDescription> LoadBalancers
        {
            get
            {
                if (this.loadBalancers == null)
                {
                    this.loadBalancers = new ObservableCollection<LoadBalancerDescription>();
                }
                return this.loadBalancers;
            }

            set
            {
                if (this.loadBalancers != value)
                {
                    this.loadBalancers = value;
                    this.OnPropertyChanged("LoadBalancers");
                }
            }
        }

        public ObservableCollection<AS.Instance> RunningInstances
        {
            get
            {
                if (this.runningInstances == null)
                {
                    this.runningInstances = new ObservableCollection<AS.Instance>();
                }
                return this.runningInstances;
            }

            set
            {
                if (this.runningInstances != value)
                {
                    this.runningInstances = value;
                    this.OnPropertyChanged("RunningInstances");
                }
            }
        }

        public ConsoleLC SelectedLaunchConfiguration
        {
            get
            {
                if (this.selectedLaunchConfiguration == null)
                    this.selectedLaunchConfiguration = new ConsoleLC();
                return this.selectedLaunchConfiguration;
            }

            set
            {
                if (this.selectedLaunchConfiguration != value)
                {
                    this.selectedLaunchConfiguration = value;
                    this.OnPropertyChanged("SelectedLaunchConfiguration");
                    this.SelectedLaunchConfigurationChanged(this, new EventArgs());
                }
            }
        }

        public Amazon.EC2.Model.AvailabilityZone SelectedZone
        {
            get
            {
                if (this.selectedZone == null)
                    this.selectedZone = new Amazon.EC2.Model.AvailabilityZone();
                return this.selectedZone;
            }

            set
            {
                if (this.selectedZone != value)
                {
                    this.selectedZone = value;
                }
            }
        }

        public LcShell LaunchConfiguration
        {
            get
            {
                if (this.launchConfiguration == null)
                    this.launchConfiguration = new LcShell();
                return this.launchConfiguration;
            }

            set
            {
                if (this.launchConfiguration != value)
                {
                    this.launchConfiguration = value;
                    this.OnPropertyChanged("LaunchConfiguration");
                }
            }
        }

        public ConsoleVPC SelectedVpc
        {
            get
            {
                return this.selectedVpc;
            }

            set
            {
                if (this.selectedVpc != value)
                {
                    this.selectedVpc = value;
                    this.OnPropertyChanged("SelectedVpc");
                    this.OnPropertyChanged("Environment");
                    this.OnPropertyChanged("EnvironmentDescription");
                }
            }
        }

        public ConsoleASG SelectedAutoScalingGroup
        {
            get
            {
                if (this.selectedAutoScalingGroup == null)
                    this.selectedAutoScalingGroup = new ConsoleASG();

                return this.selectedAutoScalingGroup;
            }

            set
            {
                if (this.selectedAutoScalingGroup != value)
                {
                    this.selectedAutoScalingGroup = value;
                      
                    this.OnPropertyChanged("SelectedAutoScalingGroup");
                    this.SelectedAutoScaleGroupChanged(this, new EventArgs());
                }
            }
        }

        public RefreshPeriod SelectedRefreshPeriod
        {
            get
            {
                if (this.selectedRefreshPeriod == null)
                    this.selectedRefreshPeriod = this.refreshPeriods[0];

                return this.selectedRefreshPeriod;
            }

            set
            {
                if (this.selectedRefreshPeriod != value)
                {
                    this.selectedRefreshPeriod = value;

                    this.OnPropertyChanged("SelectedRefreshPeriod");
                }
            }
        }

        public AsgShell ASGroup
        {
            get
            {
                if (this.asGroup == null)
                    this.asGroup = new AsgShell();
                return this.asGroup;
            }

            set
            {
                if (this.asGroup != value)
                {
                    this.asGroup = value;
                    this.OnPropertyChanged("ASGroup");
                }
            }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    this.OnPropertyChanged("IsBusy");
                }
            }
        }

        public string BusyContent
        {
            get { return this.busyContent; }
            set
            {
                if (this.busyContent != value)
                {
                    this.busyContent = value;
                    this.OnPropertyChanged("BusyContent");
                }
            }
        }

        public bool IsVpc
        {
            get { return this.isVpc; }
            set
            {
                if (this.isVpc != value)
                {
                    this.isVpc = value;
                    this.OnPropertyChanged("IsVpc");
                    this.OnPropertyChanged("EnvironmentDescription");
                }
            }
        }

        public int RefreshRate
        {
            get { return this.refreshRate; }
            set
            {
                if (this.refreshRate != value)
                {
                    this.refreshRate = value;
                    this.OnPropertyChanged("RefreshRate");
                }
            }
        }

        public int RefreshCountdown
        {
            get { return this.refreshCountdown; }
            set
            {
                if (this.refreshCountdown != value)
                {
                    this.refreshCountdown = value;
                    this.OnPropertyChanged("RefreshCountdown");
                }
            }
        }

    }
}
