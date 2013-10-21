/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * Licensed under the Amazon Software License (the "License"). You may not use this file except 
 * in compliance with the License. A copy of the License is located at http://aws.amazon.com/asl/
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *
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
