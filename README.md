# AWS Auto Scaling Console Sample C# Project

This C# project is a sample Auto Scaling Console application utilizing the Windows Presentation Foundation (WPF) to provide a Graphical User Interface for managing the AWS Auto Scaling feature. This sample utilizes the EC2, Auto Scaling, Elastic Load Balancing and SNS .NET APIs provided in the [AWS SDK for .NET](http://aws.amazon.com/sdkfornet/ "AWS SDK for .NET")

## About this sample
Being elastic is a fundamental principle of AWS. The key is to provision just the right amount of compute and storage resources to serve your workload. Compute nodes are the most common resource to scale, to ensure the right number of instances are running to meet the current application demand. Auto Scaling is well suited for applications that experience hourly, daily, or weekly variability in usage and need to automatically scale horizontally to keep up with usage variability. Auto Scaling frees you from having to accurately predict huge traffic spikes and plan for provisioning resources in advance of them. With Auto Scaling you can build a fully scalable and affordable infrastructure on the cloud.

As an example, mywebsite.com is running multiple copies of its web application hosted on identical Amazon EC2 instances, each handling customer requests. These EC2 instances are categorized into an Auto Scaling group and fronted by an Elastic Load Balancer that facilitates load balancing resources across Availability Zones.

![start](https://raw.github.com/awslabs/aws-auto-scaling-console-sample/master/images/start.png)

As time moves along, the application load increases and the web server fleet needs to horizontally scale for additional capacity. This can be triggered in a number of ways, such as, by using a schedule you define because you know when the peak is coming or a more likely scenario is the EC2 instance will generate an aggregate metric across the auto scaling fleet that is used to trigger the scale out....let’s say CPU load. So when the fleet average exceeds 80% the group will scale out.

![scale out](https://raw.github.com/awslabs/aws-auto-scaling-console-sample/master/images/scaleout.png)

Then when CPU average across the fleet reaches less than 30%, the auto scaling group will scale down and the additional instances are removed. 

![scale down](https://raw.github.com/awslabs/aws-auto-scaling-console-sample/master/images/scalein.png)

This helps mywebsite.com make efficient use of its compute resources by automatically scaling in and out based on key metrics for its web server fleet.

The auto scaling console sample allows you to setup and manage your own [Launch Configurations](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/AS_Concepts.html#LaunchConfiguration "Launch Configurations") and [Auto Scaling Groups](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/AS_Concepts.html#AutoScalingGroup "Auto Scaling Groups") through a Graphical User Interface.

## Requirements
In order to use the auto scaling console sample application, you will need an AWS account. To sign up for AWS, go to [http://aws.amazon.com](http://aws.amazon.com).

Additionally, you will need to install the [AWS SDK for .NET](http://aws.amazon.com/sdkfornet/ "AWS SDK for .NET") which provides .NET APIs for many AWS services including those used in this sample application.

## Basic Configuration
You need to set your AWS security credentials before the sample is able to connect to AWS. The sample will automatically pick up the credentials provided in the  appSettings section of the application configuration file (App.config):

	<appSettings>
	  <add key="AWSAccessKey" value="YOUR ACCESS KEY ID" />
	  <add key="AWSSecretKey" value="YOUR SECRET KEY" />
	</appSettings>

See the [Your Security Credentials](https://console.aws.amazon.com/iam/home?#security_credential "Your Security Credentials") page for more information on getting your keys. For more information on configuring credentials for applications, see the AWS SDK for .NET [Developer Guide](http://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/net-dg-config-creds.html).

## Running the Sample
To run the application, just compile and launch the Auto Scaling Console executable.

- Open the **AWS.AutoScale.Demo.sln** file with Visual Studio or your desired .NET IDE
- Open the **App.config** from the Visual Studio Solution Explorer or your desired .NET IDE
- Enter in your AWS Access Key and AWS Secret Key in the value attribute of the **AWSAccessKey** and **AWSSecretKey** application settings
- Compile and launch the executable

## Resources
- [AWS SDK for .NET](http://aws.amazon.com/sdkfornet/ "AWS SDK for .NET")
- [AWS Developer Guide for .NET](http://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/welcome.html "AWS Developer Guide for .NET")
- [Auto Scaling Concepts and Terminology](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/AS_Concepts.html#AutoScalingGroup "Auto Scaling Concepts and Terminology")
- [Basic Auto Scaling Configuration](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/US_BasicSetup.html "Basic Auto Scaling Configuration")
- [Using Auto Scaling](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/ProgrammingGuide.html "Using Auto Scaling")
- [Auto Scaling Developer Guide](http://docs.aws.amazon.com/AutoScaling/latest/DeveloperGuide/Welcome.html "Auto Scaling Developer Guide")

## License

This sample application is distributed under the [Amazon Software License](http://aws.amazon.com/asl/ "Amazon Software License").
