using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ImportControl.xaml
    /// </summary>
    public partial class ImportControl : UserControl
    {
        public event EventHandler PolygonSelectionChanged
        {
            add => Context.PolygonSelectionChanged += value;
            remove => Context.PolygonSelectionChanged -= value;
        }

        public ImportControlModel Context { get; }

        public ImportControl(IReadOnlyTtDataLayer dal, bool sortByName, bool hasMetadata = false, bool hasGroups = false, bool hasNmea = false)
        {
            InitializeComponent();

            this.DataContext = Context = new ImportControlModel(dal, sortByName, hasMetadata, hasGroups, hasNmea);
        }
    }
}
