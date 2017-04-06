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

    public class StructureTimerViewModel : StructureBaseViewModel<StructureTimerModel>
    {
        #region fields

        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;

        #endregion

        #region ctor

        public StructureTimerViewModel(BaseViewModel parentViewModel, StructureTimerModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
        }

        public StructureTimerViewModel(BaseViewModel parentViewModel, StructureTimerModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            _dialogService = dialogService;
            _colorDialogFactory = colorDialogFactory;

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

        protected new StructureTimerModel DataModel
        {
            get { return base.DataModel as StructureTimerModel; }
        }

        public BindableVector3DModel Center
        {
            get { return new BindableVector3DModel(DataModel.Center); }
            set { DataModel.Center = value.ToVector3(); }
        }

        public string ActiveComponentFilter
        {
            get { return DataModel.ActiveComponentFilter; }
            set { DataModel.ActiveComponentFilter = value; }
        }

        public string ComponentFilter
        {
            get { return DataModel.ComponentFilter; }
            set { DataModel.ComponentFilter = value; }
        }

        public bool IsConstructionNotReady
        {
            get { return DataModel.IsConstructionNotReady; }
            set { DataModel.IsConstructionNotReady = value; }
        }

        public bool IsSubsSystemNotReady
        {
            get { return DataModel.IsSubsSystemNotReady; }
            set { DataModel.IsSubsSystemNotReady = value; }
        }

        public string GridName
        {
            get { return DataModel.GridName; }
        }

        public string OwnerName
        {
            get { return DataModel.OwnerName; }
        }

        public string ToolbarSummary
        {
            get { return DataModel.ToolbarSummary; }
        }

        public decimal Delay
        {
            get { return DataModel.Delay; }
        }

        public bool Enabled
        {
            get { return DataModel.Enabled; }
        }

        public string SelfTriggerType
        {
            get { return DataModel.SelfTriggerType; }
        }

        public string ProgrammableBlockName
        {
            get { return DataModel.ProgrammableBlockName; }
        }

        public string ProgrammableBlockSourceCodePreview
        {
            get { return DataModel.ProgrammableBlockSourceCodePreview; }
        }

        public string ProgrammableBlockSourceCode
        {
            get { return DataModel.ProgrammableBlockSourceCode; }
        }

        #endregion

        #region command methods

        #endregion

        #region methods

        #endregion
    }
}
