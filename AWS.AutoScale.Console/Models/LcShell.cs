/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Data model shell for creating an Auto Scaling Launch Configuration in the console
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.AutoScale.Console.Models
{
    public class LcShell : NotifyPropertyChangeBase, IDataErrorInfo
    {
        private string name;
        private string ami;
        private string key;
        private InstanceType instanceType;
        public Dictionary<string, string> Errors = new Dictionary<string, string>();
        public bool IsValidating = false;

        /// <summary>
        /// Error Property required for data error info
        /// </summary>
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Error message retrieval based on property name
        /// </summary>
        /// <param name="columnName">Name of property to retrieve error message for</param>
        /// <returns>Error message string</returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                if (!IsValidating) return result;
                Errors.Remove(columnName);
                switch (columnName)
                {
                    case "Name": if (string.IsNullOrEmpty(Name)) result = "Name is required"; break;
                    case "Ami": if (string.IsNullOrEmpty(Ami)) result = "Ami is required"; break;
                    case "InstanceType": if (instanceType == null) result = "Instance Type is required"; break;
                }

                if (result != string.Empty) Errors.Add(columnName, result);
                return result;
            }
        }

        /// <summary>
        /// Determines validity of data properties
        /// </summary>
        /// <returns>true/false based on property validity</returns>
        public bool IsValid()
        {
            IsValidating = true;
            try
            {
                base.OnPropertyChanged("Name");
                base.OnPropertyChanged("Ami");
                base.OnPropertyChanged("InstanceType");
            }
            finally
            {
                IsValidating = false;
            }
            return (Errors.Count() == 0);
        }

        /// <summary>
        /// Display name of the Launch Configuration
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
        /// Unique ID of the Amazon Machine Image (AMI) which is assigned during registration
        /// </summary>
        public string Ami
        {
            get
            {
                return this.ami;
            }

            set
            {
                this.ami = value;
            }
        }

        /// <summary>
        /// The name of the Amazon EC2 key pair.
        /// </summary>
        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// The instance type of the Amazon EC2 instance
        /// </summary>
        public InstanceType InstanceType
        {
            get
            {
                return this.instanceType;
            }

            set
            {
                this.instanceType = value;
            }
        }

    }
}
