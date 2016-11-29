﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for CsvImportControl.xaml
    /// </summary>
    public partial class CsvImportControl : UserControl
    {
        public CsvImportControl(string fileName, Action<IReadOnlyTtDataLayer, bool> onSetup)
        {
            DataContext = new CsvImportModel(fileName, onSetup);

            InitializeComponent();
        }
        
    }
}
