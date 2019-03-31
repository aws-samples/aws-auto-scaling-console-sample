/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model for instance types
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class InstanceType
    {
        private string name;

        /// <summary>
        /// base constructor
        /// </summary>
        public InstanceType()
		{
		}

        /// <summary>
        /// constructor with name
        /// </summary>
        /// <param name="name">Name of instance type</param>
        public InstanceType(string name)
		{
			Name = name;
		}

        /// <summary>
        /// Display name of instance type
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
    }
}
