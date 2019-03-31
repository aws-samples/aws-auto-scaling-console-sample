/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model for regions
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class Region
    {
        private string name;

        /// <summary>
        /// constructor
        /// </summary>
		public Region()
		{
		}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Display name of the region</param>
        public Region(string name)
		{
			Name = name;
		}

        /// <summary>
        /// Display name of the region
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
			}
		}

        /// <summary>
        /// Auto Scaling regional SSL endpoint URL
        /// </summary>
		public string Url 
		{
			get
			{
                return string.Concat("https://autoscaling.", this.name, ".amazonaws.com");
			}
		}

        /// <summary>
        /// EC2 regional SSL endpoint url
        /// </summary>
        public string Ec2Url
        {
            get
            {
                return string.Concat("https://ec2.", this.name, ".amazonaws.com");
            }
        }

        /// <summary>
        /// ELB regional SSL endpoint url
        /// </summary>
        public string ElbUrl
        {
            get
            {
                return string.Concat("https://elasticloadbalancing.", this.name, ".amazonaws.com");
            }
        }

    }
}
