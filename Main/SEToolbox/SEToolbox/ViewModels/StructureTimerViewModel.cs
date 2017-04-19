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

        private Tuple<long, string> _selectedProgrammableBlock;
        private string _programmableBlockSourceCode;

        #endregion

        #region ctor

        public StructureTimerViewModel(BaseViewModel parentViewModel, StructureTimerModel dataModel)
            : base(parentViewModel, dataModel)
        {
            SelectedProgrammableBlock = DataModel.ProgrammableBlocks?.FirstOrDefault();
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

        public string ToolbarSummary
        {
            get { return DataModel.ToolbarSummary; }
        }

        public IEnumerable<string> ToolbarButtons
        {
            get { return DataModel.ToolbarButtons; }
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

        public IEnumerable<Tuple<long, string>> ProgrammableBlocks
        {
            get { return DataModel.ProgrammableBlocks; }
        }

        public Tuple<long, string> SelectedProgrammableBlock
        {
            get { return _selectedProgrammableBlock; }
            set
            {
                if (value != _selectedProgrammableBlock)
                {
                    _selectedProgrammableBlock = value;
                    _programmableBlockSourceCode = DataModel.ProgrammableBlockSourceCodes.SingleOrDefault(pb => pb.Item1 == _selectedProgrammableBlock.Item1).Item2;
                    OnPropertyChanged(nameof(ProgrammableBlockSourceCode));
                }
            }
        }

        public string ProgrammableBlockNames
        {
            get { return DataModel.ProgrammableBlockNames; }
        }

        public string ProgrammableBlockSourceCodePreview
        {
            get { return DataModel.ProgrammableBlockSourceCodePreview; }
        }

        public string ProgrammableBlockSourceCode
        {
            get { return _programmableBlockSourceCode; }
        }

        #endregion

        #region command methods

        #endregion

        #region methods

        #endregion
    }
}
