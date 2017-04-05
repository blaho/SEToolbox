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

    public class StructurePlayerViewModel : StructureBaseViewModel<StructurePlayerModel>
    {
        #region fields

        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;

        #endregion

        #region ctor

        public StructurePlayerViewModel(BaseViewModel parentViewModel, StructurePlayerModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
        }

        public StructurePlayerViewModel(BaseViewModel parentViewModel, StructurePlayerModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
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

        protected new StructurePlayerModel DataModel
        {
            get { return base.DataModel as StructurePlayerModel; }
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

        public string BlockCountDetails
        {
            get { return DataModel.BlockCountDetails; }
        }

        public int AssemblerCount
        {
            get { return DataModel.AssemblerCount; }
        }

        public string AssemblerDetails
        {
            get { return DataModel.AssemblerDetails; }
        }

        public int RefineryCount
        {
            get { return DataModel.RefineryCount; }
        }

        public string RefineryDetails
        {
            get { return DataModel.RefineryDetails; }
        }

        public int ShipToolCount
        {
            get { return DataModel.ShipToolCount; }
        }

        public string ShipToolDetails
        {
            get { return DataModel.ShipToolDetails; }
        }

        public int PowerBlockCount
        {
            get { return DataModel.PowerBlockCount; }
        }

        public string PowerBlockDetails
        {
            get { return DataModel.PowerBlockDetails; }
        }

        public int ThrusterCount
        {
            get { return DataModel.ThrusterCount; }
        }

        public string ThrusterDetails
        {
            get { return DataModel.ThrusterDetails; }
        }

        public int TurretCount
        {
            get { return DataModel.TurretCount; }
        }

        public string TurretDetails
        {
            get { return DataModel.TurretDetails; }
        }

        #endregion

        #region command methods

        #endregion

        #region methods

        #endregion
    }
}
