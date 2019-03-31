/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *  
 * Description: Window to view instances associated with an auto scaling group
*/

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
    public partial class InstancesWindow : Window
    {
        public ViewModel Model;

        /// <summary>
        /// InstancesWindow constructor
        /// </summary>
        public InstancesWindow()
        {
            InitializeComponent();
            //this.DataContext = this.Model;
        }

        /// <summary>
        /// InstancesWindow constructor
        /// </summary>
        /// <param name="model">Master View Model for ASG console</param>
        public InstancesWindow(ViewModel model)
            : this()
        {
            this.DataContext = model;
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

    }
}
