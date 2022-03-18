using OrbisNeighborHood.MVVM.View.SubView;
using OrbisSuite.Common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbisNeighborHood.MVVM.ViewModel.SubView
{
    internal class EditTargetViewModel
    {
        public EditTargetViewModel()
        {

        }

        #region Events

        public event EventHandler<RoutedEventArgs>? TargetChanged;

        public void DoTargetChanged()
        {
            TargetChanged?.Invoke(this, new RoutedEventArgs());
        }

        #endregion

        #region Dependency Properties

        public TargetInfo? CurrentTarget { get; set; }

        #endregion
    }
}
