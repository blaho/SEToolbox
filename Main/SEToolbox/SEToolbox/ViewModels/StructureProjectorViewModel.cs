namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using VRage.Game;

    public class StructureProjectorViewModel : StructureBaseViewModel<StructureProjectorModel>
    {
        #region fields

        private Tuple<long, string> _selectedProgrammableBlock;
        private string _programmableBlockSourceCode;

        #endregion

        #region ctor

        public StructureProjectorViewModel(BaseViewModel parentViewModel, StructureProjectorModel dataModel)
            : base(parentViewModel, dataModel)
        {
            DataModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        #endregion

        #region Properties

        protected new StructureProjectorModel DataModel
        {
            get { return base.DataModel as StructureProjectorModel; }
        }

        public string GridName
        {
            get { return DataModel.GridName; }
        }

        public string OwnerName
        {
            get { return DataModel.OwnerName; }
        }

        public string BuilderName
        {
            get { return DataModel.BuilderName; }
        }

        public string GridBuilderName
        {
            get { return DataModel.GridBuilderName; }
        }

        public bool Enabled
        {
            get { return DataModel.Enabled; }
        }

        public string BlockCountStr
        {
            get { return DataModel.BlockCountStr; }
        }

        #endregion

        #region command methods

        #endregion

        #region methods

        #endregion
    }
}
