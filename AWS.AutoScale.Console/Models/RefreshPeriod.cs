/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model for refresh period
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class RefreshPeriod
    {
        private string name;
        private int period;

        /// <summary>
        /// constructor
        /// </summary>
        public RefreshPeriod()
		{
		}

        /// <summary>
        /// constructor with name and period
        /// </summary>
        /// <param name="name">Name of refresh period</param>
        /// <param name="period">Number of seconds for refresh period</param>
        public RefreshPeriod(string name, int period)
		{
			Name = name;
            Period = period;
		}

        /// <summary>
        /// Display name of the refresh period
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
        /// Number of seconds for refresh period
        /// </summary>
        public int Period
        {
            get
            {
                return this.period;
            }

            set
            {
                this.period = value;
            }
        }
    }
}
