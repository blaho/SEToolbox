﻿namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Shell;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using Res = SEToolbox.Properties.Resources;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using WPFLocalizeExtension.Engine;
    using System.Windows.Threading;

    public class ExplorerViewModel : BaseViewModel, IDropable, IMainView
    {
        #region Fields

        private static readonly object Locker = new object();
        private readonly ExplorerModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;
        private bool? _closeResult;

        private bool _ignoreUpdateSelection;

        private IStructureViewBase _preSelectedStructure;
        private IStructureViewBase _selectedStructure;
        private ObservableCollection<IStructureViewBase> _selections;
        private ObservableCollection<IStructureViewBase> _structures;

        private ObservableCollection<StructurePlayerViewModel> _players;
        private ObservableCollection<StructureTimerViewModel> _timers;
        private ObservableCollection<StructureProjectorViewModel> _projectors;

        private ObservableCollection<LanguageModel> _languages;

        // If true, when adding new models to the collection, the new models will be highlighted as selected in the UI.
        private bool _selectNewStructure;

        private int _selectedTabIndex;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public ExplorerViewModel(ExplorerModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>, ServiceLocator.Resolve<ISaveFileDialog>)
        {
        }

        public ExplorerViewModel(ExplorerModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory, Func<ISaveFileDialog> saveFileDialogFactory)
            : base(null)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);
            Contract.Requires(saveFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _saveFileDialogFactory = saveFileDialogFactory;
            _dataModel = dataModel;

            Selections = new ObservableCollection<IStructureViewBase>();
            Selections.CollectionChanged += (sender, e) => RaisePropertyChanged(() => IsMultipleSelections);

            Structures = new ObservableCollection<IStructureViewBase>();
            foreach (var item in _dataModel.Structures)
            {
                AddViewModel(item);
            }

            Players = new ObservableCollection<StructurePlayerViewModel>();
            Timers = new ObservableCollection<StructureTimerViewModel>();
            Projectors = new ObservableCollection<StructureProjectorViewModel>();
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Structures, Locker);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Players, Locker);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Timers, Locker);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Projectors, Locker);

            UpdateLanguages();

            _dataModel.Structures.CollectionChanged += Structures_CollectionChanged;
            _dataModel.Players.CollectionChanged += Players_CollectionChanged;
            _dataModel.Timers.CollectionChanged += Timers_CollectionChanged;
            _dataModel.Projectors.CollectionChanged += Projectors_CollectionChanged;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand ClosingCommand => new DelegateCommand<CancelEventArgs>(ClosingExecuted, ClosingCanExecute);

        public ICommand OpenCommand => new DelegateCommand(OpenExecuted, OpenCanExecute);

        public ICommand SaveCommand => new DelegateCommand(SaveExecuted, SaveCanExecute);

        public ICommand SaveAsCommand => new DelegateCommand(SaveAsExecuted, SaveAsCanExecute);

        public ICommand ClearCommand => new DelegateCommand(ClearExecuted, ClearCanExecute);

        public ICommand ReloadCommand => new DelegateCommand(ReloadExecuted, ReloadCanExecute);

        public ICommand IsActiveCommand => new DelegateCommand(new Func<bool>(IsActiveCanExecute));

        public ICommand ImportVoxelCommand => new DelegateCommand(ImportVoxelExecuted, ImportVoxelCanExecute);

        public ICommand ImportImageCommand => new DelegateCommand(ImportImageExecuted, ImportImageCanExecute);

        public ICommand ImportModelCommand => new DelegateCommand(ImportModelExecuted, ImportModelCanExecute);

        public ICommand ImportAsteroidModelCommand => new DelegateCommand(ImportAsteroidModelExecuted, ImportAsteroidModelCanExecute);

        public ICommand ImportSandboxObjectCommand => new DelegateCommand(ImportSandboxObjectExecuted, ImportSandboxObjectCanExecute);

        public ICommand OpenComponentListCommand => new DelegateCommand(OpenComponentListExecuted, OpenComponentListCanExecute);

        public ICommand WorldReportCommand => new DelegateCommand(WorldReportExecuted, WorldReportCanExecute);

        public ICommand OpenFolderCommand => new DelegateCommand(OpenFolderExecuted, OpenFolderCanExecute);

        public ICommand ViewSandboxCommand => new DelegateCommand(ViewSandboxExecuted, ViewSandboxCanExecute);

        public ICommand OpenWorkshopCommand => new DelegateCommand(OpenWorkshopExecuted, OpenWorkshopCanExecute);

        public ICommand ExportSandboxObjectCommand => new DelegateCommand(ExportSandboxObjectExecuted, ExportSandboxObjectCanExecute);

        public ICommand ExportBasicSandboxObjectCommand => new DelegateCommand(ExportBasicSandboxObjectExecuted, ExportBasicSandboxObjectCanExecute);

        public ICommand ExportPrefabObjectCommand => new DelegateCommand(ExportPrefabObjectExecuted, ExportPrefabObjectCanExecute);

        public ICommand CreateFloatingItemCommand => new DelegateCommand(CreateFloatingItemExecuted, CreateFloatingItemCanExecute);

        public ICommand GenerateVoxelFieldCommand => new DelegateCommand(GenerateVoxelFieldExecuted, GenerateVoxelFieldCanExecute);

        public ICommand Test1Command => new DelegateCommand(Test1Executed, Test1CanExecute);

        public ICommand Test2Command => new DelegateCommand(Test2Executed, Test2CanExecute);

        public ICommand Test3Command => new DelegateCommand(Test3Executed, Test3CanExecute);

        public ICommand Test4Command => new DelegateCommand(Test4Executed, Test4CanExecute);

        public ICommand Test5Command => new DelegateCommand(Test5Executed, Test5CanExecute);

        public ICommand Test6Command => new DelegateCommand(Test6Executed, Test6CanExecute);

        public ICommand OpenSettingsCommand => new DelegateCommand(OpenSettingsExecuted, OpenSettingsCanExecute);

        public ICommand OpenUpdatesLinkCommand => new DelegateCommand(OpenUpdatesLinkExecuted, OpenUpdatesLinkCanExecute);

        public ICommand OpenDocumentationLinkCommand => new DelegateCommand(OpenDocumentationLinkExecuted, OpenDocumentationLinkCanExecute);

        public ICommand OpenSupportLinkCommand => new DelegateCommand(OpenSupportLinkExecuted, OpenSupportLinkCanExecute);

        public ICommand AboutCommand => new DelegateCommand(AboutExecuted, AboutCanExecute);

        public ICommand LanguageCommand => new DelegateCommand(new Func<bool>(LanguageCanExecute));

        public ICommand SetLanguageCommand => new DelegateCommand<string>(SetLanguageExecuted, SetLanguageCanExecute);

        public ICommand DeleteObjectCommand => new DelegateCommand(DeleteObjectExecuted, DeleteObjectCanExecute);

        public ICommand CopyObjectGpsCommand => new DelegateCommand(CopyObjectGpsExecuted, CopyObjectGpsCanExecute);

        public ICommand GroupMoveCommand => new DelegateCommand(GroupMoveExecuted, GroupMoveCanExecute);

        public ICommand RejoinShipCommand => new DelegateCommand(RejoinShipExecuted, RejoinShipCanExecute);

        public ICommand JoinShipPartsCommand => new DelegateCommand(JoinShipPartsExecuted, JoinShipPartsCanExecute);

        public ICommand VoxelMergeCommand => new DelegateCommand(VoxelMergeExecuted, VoxelMergeCanExecute);

        public ICommand RepairShipsCommand => new DelegateCommand(RepairShipsExecuted, RepairShipsCanExecute);

        public ICommand ResetVelocityCommand => new DelegateCommand(ResetVelocityExecuted, ResetVelocityCanExecute);

        public ICommand ConvertToShipCommand => new DelegateCommand(ConvertToShipExecuted, ConvertToShipCanExecute);

        public ICommand ConvertToStationCommand => new DelegateCommand(ConvertToStationExecuted, ConvertToStationCanExecute);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return _closeResult;
            }

            set
            {
                _closeResult = value;
                RaisePropertyChanged(() => CloseResult);
            }
        }

        public ObservableCollection<IStructureViewBase> Structures
        {
            get
            {
                return _structures;
            }

            private set
            {
                if (value != _structures)
                {
                    _structures = value;
                    RaisePropertyChanged(() => Structures);
                }
            }
        }

        public IStructureViewBase SelectedStructure
        {
            get
            {
                return _selectedStructure;
            }

            set
            {
                if (value != _selectedStructure)
                {
                    _selectedStructure = value;
                    if (_selectedStructure != null && !_ignoreUpdateSelection)
                        _selectedStructure.DataModel.InitializeAsync();
                    RaisePropertyChanged(() => SelectedStructure);
                }
            }
        }

        public ObservableCollection<IStructureViewBase> Selections
        {
            get
            {
                return _selections;
            }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    RaisePropertyChanged(() => Selections);
                }
            }
        }

        public bool IsMultipleSelections
        {
            get
            {
                return _selections.Count > 1;
            }
        }

        public ObservableCollection<StructurePlayerViewModel> Players
        {
            get
            {
                return _players;
            }

            private set
            {
                if (value != _players)
                {
                    _players = value;
                    RaisePropertyChanged(() => Players);
                }
            }
        }

        public ObservableCollection<StructureTimerViewModel> Timers
        {
            get
            {
                return _timers;
            }

            private set
            {
                if (value != _timers)
                {
                    _timers = value;
                    RaisePropertyChanged(() => Timers);
                }
            }
        }

        public ObservableCollection<StructureProjectorViewModel> Projectors
        {
            get
            {
                return _projectors;
            }

            private set
            {
                if (value != _projectors)
                {
                    _projectors = value;
                    RaisePropertyChanged(() => Projectors);
                }
            }
        }

        public WorldResource ActiveWorld
        {
            get
            {
                return _dataModel.ActiveWorld;
            }
            set
            {
                _dataModel.ActiveWorld = value;
            }
        }

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return _dataModel.ThePlayerCharacter;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _dataModel.IsActive;
            }

            set
            {
                _dataModel.IsActive = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _dataModel.IsBusy;
            }

            set
            {
                _dataModel.IsBusy = value;
            }
        }

        public bool IsDebugger
        {
            get
            {
                return Debugger.IsAttached;
            }
        }

        public bool IsModified
        {
            get
            {
                return _dataModel.IsModified;
            }

            set
            {
                _dataModel.IsModified = value;
            }
        }

        public bool IsBaseSaveChanged
        {
            get
            {
                return _dataModel.IsBaseSaveChanged;
            }

            set
            {
                _dataModel.IsBaseSaveChanged = value;
            }
        }

        public ObservableCollection<LanguageModel> Languages
        {
            get
            {
                return _languages;
            }

            private set
            {
                if (value != _languages)
                {
                    _languages = value;
                    RaisePropertyChanged(() => Languages);
                }
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }

            set
            {
                if (value != _selectedTabIndex)
                {
                    SelectedStructure = null;
                    Selections.Clear();
                    _selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
        }

        #endregion

        #region Command Methods

        public bool ClosingCanExecute(CancelEventArgs e)
        {
            return true;
        }

        public void ClosingExecuted(CancelEventArgs e)
        {
            // TODO: dialog on outstanding changes.


            //if (CheckCloseWindow() == false)
            //{
            //e.Cancel = true;
            //CloseResult = null;
            //}
            //else
            {
                if (CloseRequested != null)
                {
                    CloseRequested(this, EventArgs.Empty);
                }
            }
        }

        public bool OpenCanExecute()
        {
            return true;
        }

        public void OpenExecuted()
        {
            var model = new SelectWorldModel();
            model.Load(SpaceEngineersConsts.BaseLocalPath, SpaceEngineersConsts.BaseDedicatedServerHostPath, SpaceEngineersConsts.BaseDedicatedServerServicePath);
            var loadVm = new SelectWorldViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowLoad>(this, loadVm);
            if (result == true)
            {
                SelectedTabIndex = 0;
                _dataModel.BeginLoad();
                _dataModel.ActiveWorld = model.SelectedWorld;
                ActiveWorld.LoadCheckpoint();
                ActiveWorld.LoadDefinitionsAndMods();
                ActiveWorld.LoadSector();
                _dataModel.ParseSandBox();
                _dataModel.EndLoad();
            }
        }

        public bool SaveCanExecute()
        {
            return _dataModel.ActiveWorld != null &&
                ((_dataModel.ActiveWorld.SaveType != SaveWorldType.DedicatedServerService && _dataModel.ActiveWorld.SaveType != SaveWorldType.CustomAdminRequired)
                || ((_dataModel.ActiveWorld.SaveType == SaveWorldType.DedicatedServerService || _dataModel.ActiveWorld.SaveType == SaveWorldType.CustomAdminRequired)
                    && ToolboxUpdater.IsRuningElevated()));
        }

        public void SaveExecuted()
        {
            if (_dataModel != null)
            {
                _dataModel.SaveCheckPointAndSandBox();
            }
        }

        public bool SaveAsCanExecute()
        {
            return _dataModel.ActiveWorld != null &&
                _dataModel.ActiveWorld.SaveType != SaveWorldType.Custom &&
                ((_dataModel.ActiveWorld.SaveType != SaveWorldType.DedicatedServerService && _dataModel.ActiveWorld.SaveType != SaveWorldType.CustomAdminRequired)
                || ((_dataModel.ActiveWorld.SaveType == SaveWorldType.DedicatedServerService || _dataModel.ActiveWorld.SaveType == SaveWorldType.CustomAdminRequired)
                    && ToolboxUpdater.IsRuningElevated()));
        }

        public void SaveAsExecuted()
        {
            if (_dataModel != null)
            {
                // TODO: dialog for save name.
                // TODO: create new directory.
                // TODO: copy all files across from old to new.
                // TODO: update _dataModel.ActiveWorld. paths.
                //_dataModel.SaveCheckPointAndSandBox();
            }
        }

        public bool ClearCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ClearExecuted()
        {
            // TODO: clear loaded data.
        }

        public bool ReloadCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ReloadExecuted()
        {
            // TODO: check is save directory is still valid.

            _dataModel.BeginLoad();

            // Reload Checkpoint file.
            ActiveWorld.LoadCheckpoint();

            // Reload Definitions, Mods, and clear out Materials, Textures.
            ActiveWorld.LoadDefinitionsAndMods();
            Converters.DDSConverter.ClearCache();

            // Load Sector file.
            ActiveWorld.LoadSector();

            _dataModel.ParseSandBox();
            _dataModel.EndLoad();
        }

        public bool IsActiveCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public bool ImportVoxelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportVoxelExecuted()
        {
            var model = new ImportVoxelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new ImportVoxelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportVoxel>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                var structure = _dataModel.AddEntity(newEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (_preSelectedStructure != null)
                    SelectedStructure = _preSelectedStructure;
                IsBusy = false;
            }
        }

        public bool ImportImageCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportImageExecuted()
        {
            var model = new ImportImageModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new ImportImageViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportImage>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                // make sure resultant object has cubes.
                if (newEntity.CubeBlocks.Count != 0)
                {
                    _selectNewStructure = true;
                    _dataModel.CollisionCorrectEntity(newEntity);
                    _dataModel.AddEntity(newEntity);
                    _selectNewStructure = false;
                }
                IsBusy = false;
            }
        }

        public bool ImportModelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportModelExecuted()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                if (loadVm.IsValidModel)
                {
                    _dataModel.CollisionCorrectEntity(newEntity);
                    var structure = _dataModel.AddEntity(newEntity);

                    if (structure is StructureVoxelModel)
                    {
                        ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    }

                    if (_preSelectedStructure != null)
                        SelectedStructure = _preSelectedStructure;
                }
                IsBusy = false;
            }
        }

        public bool ImportAsteroidModelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportAsteroidModelExecuted()
        {
            var model = new Import3DAsteroidModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DAsteroidViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportAsteroidModel>(this, loadVm);
            if (result == true && loadVm.IsValidEntity)
            {
                IsBusy = true;
                _dataModel.CollisionCorrectEntity(loadVm.NewEntity);
                var structure = _dataModel.AddEntity(loadVm.NewEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (_preSelectedStructure != null)
                    SelectedStructure = _preSelectedStructure;

                if (loadVm.SaveWhenFinsihed)
                {
                    _dataModel.SaveCheckPointAndSandBox();
                }

                IsBusy = false;
            }
        }

        public bool ImportSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportSandboxObjectExecuted()
        {
            ImportSandboxObjectFromFile();
        }

        public bool OpenComponentListCanExecute()
        {
            return true;
        }

        public void OpenComponentListExecuted()
        {
            var model = new ComponentListModel();
            model.Load();
            var loadVm = new ComponentListViewModel(this, model);
            _dialogService.Show<WindowComponentList>(this, loadVm);
        }

        public bool WorldReportCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void WorldReportExecuted()
        {
            var model = new ResourceReportModel();
            model.Load(_dataModel.ActiveWorld.Savename, _dataModel.Structures);
            var loadVm = new ResourceReportViewModel(this, model);
            _dialogService.ShowDialog<WindowResourceReport>(this, loadVm);
        }

        public bool OpenFolderCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void OpenFolderExecuted()
        {
            Process.Start("Explorer", string.Format("\"{0}\"", _dataModel.ActiveWorld.Savepath));
        }

        public bool ViewSandboxCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ViewSandboxExecuted()
        {
            if (_dataModel != null)
            {
                var filename = _dataModel.SaveTemporarySandbox();
                Process.Start(string.Format("\"{0}\"", filename));
            }
        }
        public bool OpenWorkshopCanExecute()
        {
            return _dataModel.ActiveWorld != null && _dataModel.ActiveWorld.WorkshopId.HasValue &&
                   _dataModel.ActiveWorld.WorkshopId.Value != 0;
        }

        public void OpenWorkshopExecuted()
        {
            Process.Start(string.Format("http://steamcommunity.com/sharedfiles/filedetails/?id={0}", _dataModel.ActiveWorld.WorkshopId.Value), null);
        }

        public bool ExportSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ExportSandboxObjectExecuted()
        {
            ExportSandboxObjectToFile(false, Selections.ToArray());
        }

        public bool ExportBasicSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ExportBasicSandboxObjectExecuted()
        {
            ExportSandboxObjectToFile(true, Selections.ToArray());
        }

        public bool ExportPrefabObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ExportPrefabObjectExecuted()
        {
            ExportPrefabObjectToFile(true, Selections.ToArray());
        }

        public bool CreateFloatingItemCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void CreateFloatingItemExecuted()
        {
            var model = new GenerateFloatingObjectModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position, _dataModel.ActiveWorld.Checkpoint.MaxFloatingObjects);
            var loadVm = new GenerateFloatingObjectViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowGenerateFloatingObject>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntities = loadVm.BuildEntities();
                if (loadVm.IsValidItemToImport)
                {
                    _selectNewStructure = true;
                    for (var i = 0; i < newEntities.Length; i++)
                    {
                        _dataModel.AddEntity(newEntities[i]);
                    }
                    _selectNewStructure = false;
                }
                IsBusy = false;
            }
        }

        public bool GenerateVoxelFieldCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void GenerateVoxelFieldExecuted()
        {
            var model = new GenerateVoxelFieldModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new GenerateVoxelFieldViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowGenerateVoxelField>(this, loadVm);
            model.Unload();
            if (result == true)
            {
                IsBusy = true;
                string[] sourceVoxelFiles;
                MyObjectBuilder_EntityBase[] newEntities;
                loadVm.BuildEntities(out sourceVoxelFiles, out newEntities);
                _selectNewStructure = true;

                ResetProgress(0, newEntities.Length);

                for (var i = 0; i < newEntities.Length; i++)
                {
                    var structure = _dataModel.AddEntity(newEntities[i]);
                    ((StructureVoxelModel)structure).SourceVoxelFilepath = sourceVoxelFiles[i]; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    Progress++;
                }
                _selectNewStructure = false;
                IsBusy = false;
                ClearProgress();
            }
        }

        public bool OpenSettingsCanExecute()
        {
            return true;
        }

        public void OpenSettingsExecuted()
        {
            var model = new SettingsModel();
            model.Load(GlobalSettings.Default.SEBinPath, GlobalSettings.Default.CustomVoxelPath, GlobalSettings.Default.AlwaysCheckForUpdates, GlobalSettings.Default.UseCustomResource);
            var loadVm = new SettingsViewModel(this, model);
            if (_dialogService.ShowDialog<WindowSettings>(this, loadVm) == true)
            {
                bool reloadMods = GlobalSettings.Default.SEBinPath != model.SEBinPath;
                GlobalSettings.Default.SEBinPath = model.SEBinPath;
                GlobalSettings.Default.CustomVoxelPath = model.CustomVoxelPath;
                GlobalSettings.Default.AlwaysCheckForUpdates = model.AlwaysCheckForUpdates;
                bool resetLocalization = GlobalSettings.Default.UseCustomResource != model.UseCustomResource;
                GlobalSettings.Default.UseCustomResource = model.UseCustomResource;
                GlobalSettings.Default.Save();

                if (reloadMods)
                {
                    IsBusy = true;

                    if (ActiveWorld == null)
                    {
                        SpaceEngineersCore.Resources.LoadDefinitions();
                    }
                    else
                    {
                        // Reload the Mods.
                        ActiveWorld.LoadDefinitionsAndMods();
                    }

                    IsBusy = false;
                }

                if (resetLocalization)
                {
                    SpaceEngineersApi.LoadLocalization();
                    UpdateLanguages();
                }
            }
        }

        public bool OpenUpdatesLinkCanExecute()
        {
            return true;
        }

        public void OpenUpdatesLinkExecuted()
        {
            Process.Start(SEToolbox.Properties.Resources.GlobalUpdatesUrl);
        }

        public bool OpenDocumentationLinkCanExecute()
        {
            return true;
        }

        public void OpenDocumentationLinkExecuted()
        {
            Process.Start(SEToolbox.Properties.Resources.GlobalDocumentationUrl);
        }

        public bool OpenSupportLinkCanExecute()
        {
            return true;
        }

        public void OpenSupportLinkExecuted()
        {
            Process.Start(SEToolbox.Properties.Resources.GlobalSupportUrl);
        }

        public bool LanguageCanExecute()
        {
            return true;
        }

        public bool SetLanguageCanExecute(string code)
        {
            return true;
        }

        public void SetLanguageExecuted(string code)
        {
            GlobalSettings.Default.LanguageCode = code;
            LocalizeDictionary.Instance.SetCurrentThreadCulture = false;
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(code);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            SpaceEngineersApi.LoadLocalization();
            UpdateLanguages();

            // Causes all bindings to update.
            OnPropertyChanged("");
        }

        public bool AboutCanExecute()
        {
            return true;
        }

        public void AboutExecuted()
        {
            var loadVm = new AboutViewModel(this);
            _dialogService.ShowDialog<WindowAbout>(this, loadVm);
        }

        public bool DeleteObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void DeleteObjectExecuted()
        {
            DeleteModel(Selections.ToArray());
        }

        public bool CopyObjectGpsCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 1;
        }

        public void CopyObjectGpsExecuted()
        {
            var text = String.Format(CultureInfo.InvariantCulture, "GPS:{0}:{1:0.00}:{2:0.00}:{3:0.00}:", Selections[0].DataModel.DisplayName.Replace(":", "_").Replace("&", "_"), Selections[0].DataModel.PositionX, Selections[0].DataModel.PositionY, Selections[0].DataModel.PositionZ);
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(text);
            }
            catch
            {
                // Ignore exception which may be generated by a Remote desktop session where Clipboard access has not been granted.
            }
        }

        public bool GroupMoveCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 1;
        }

        public void GroupMoveExecuted()
        {
            var model = new GroupMoveModel();
            var position = ThePlayerCharacter != null ? (Vector3D)ThePlayerCharacter.PositionAndOrientation.Value.Position : Vector3D.Zero;
            model.Load(Selections, position);
            var loadVm = new GroupMoveViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowGroupMove>(this, loadVm);
            if (result == true)
            {
                model.ApplyNewPositions();
                _dataModel.CalcDistances();
                IsModified = true;
            }
        }

        public bool RejoinShipCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 2 &&
                ((Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void RejoinShipExecuted()
        {
            IsBusy = true;
            RejoinShipModels(Selections[0], Selections[1]);
            IsBusy = false;
        }

        public bool JoinShipPartsCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 2 &&
                ((Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void JoinShipPartsExecuted()
        {
            IsBusy = true;
            MergeShipPartModels(Selections[0], Selections[1]);
            IsBusy = false;
        }

        public bool VoxelMergeCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 2 &&
                ((Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.Voxel && Selections[0].DataModel.IsValid) ||
                (Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.Voxel && Selections[0].DataModel.IsValid));
        }

        public void VoxelMergeExecuted()
        {
            var model = new MergeVoxelModel();
            var item1 = Selections[0];
            var item2 = Selections[1];
            model.Load(item1.DataModel, item2.DataModel);
            var loadVm = new MergeVoxelViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowVoxelMerge>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                var structure = _dataModel.AddEntity(newEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (_preSelectedStructure != null)
                    SelectedStructure = _preSelectedStructure;

                if (loadVm.RemoveOriginalAsteroids)
                {
                    DeleteModel(item1, item2);
                }

                IsBusy = false;
            }
        }

        public bool RepairShipsCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void RepairShipsExecuted()
        {
            IsBusy = true;
            RepairShips(Selections);
            IsBusy = false;
        }

        public bool ResetVelocityCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ResetVelocityExecuted()
        {
            IsBusy = true;
            StopShips(Selections);
            IsBusy = false;
        }


        public bool ConvertToShipCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ConvertToShipExecuted()
        {
            IsBusy = true;
            ConvertToShips(Selections);
            IsBusy = false;
        }

        public bool ConvertToStationCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ConvertToStationExecuted()
        {
            IsBusy = true;
            ConvertToStations(Selections);
            IsBusy = false;
        }

        #endregion

        #region Test command methods

        public bool Test1CanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void Test1Executed()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

            IsBusy = true;
            var newEntity = loadVm.BuildTestEntity();

            // Split object where X=28|29.
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X <= 3);
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X > 4);

            _selectNewStructure = true;
            _dataModel.CollisionCorrectEntity(newEntity);
            _dataModel.AddEntity(newEntity);
            _selectNewStructure = false;
            IsBusy = false;
        }

        public bool Test2CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test2Executed()
        {
            TestCalcCubesModel(Selections.ToArray());
            //OptimizeModel(Selections.ToArray());
        }

        public bool Test3CanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void Test3Executed()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

            loadVm.ArmorType = ImportArmorType.Light;
            loadVm.BuildDistance = 10;
            loadVm.ClassType = ImportModelClassType.SmallShip;
            loadVm.Filename = @"D:\Development\SpaceEngineers\building 3D\models\algos.obj";
            loadVm.Forward = new BindableVector3DModel(Vector3.Forward);
            loadVm.IsMaxLengthScale = false;
            loadVm.IsMultipleScale = true;
            loadVm.IsValidModel = true;
            loadVm.MultipleScale = 1;
            loadVm.Up = new BindableVector3DModel(Vector3.Up);

            IsBusy = true;
            var newEntity = loadVm.BuildEntity();

            // Split object where X=28|29.
            ((MyObjectBuilder_CubeGrid)newEntity).CubeBlocks.RemoveAll(c => c.Min.X <= 28);

            _selectNewStructure = true;
            _dataModel.CollisionCorrectEntity(newEntity);
            _dataModel.AddEntity(newEntity);
            _selectNewStructure = false;
            IsBusy = false;
        }

        public bool Test4CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test4Executed()
        {
            MirrorModel(false, Selections.ToArray());
        }

        public bool Test5CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test5Executed()
        {
            _dataModel.TestDisplayRotation(Selections[0].DataModel as StructureCubeGridModel);
        }

        public bool Test6CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test6Executed()
        {
            _dataModel.TestConvert(Selections[0].DataModel as StructureCubeGridModel);
        }

        #endregion

        #region methods

        void Structures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: AddViewModel(e.NewItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Remove: RemoveViewModel(e.OldItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Reset: _structures.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: AddViewModel(e.NewItems[0] as StructurePlayerModel); break;
                case NotifyCollectionChangedAction.Remove: RemoveViewModel(e.OldItems[0] as StructurePlayerModel); break;
                case NotifyCollectionChangedAction.Reset: _players.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void Timers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: AddViewModel(e.NewItems[0] as StructureTimerModel); break;
                case NotifyCollectionChangedAction.Remove: RemoveViewModel(e.OldItems[0] as StructureTimerModel); break;
                case NotifyCollectionChangedAction.Reset: _timers.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void Projectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: AddViewModel(e.NewItems[0] as StructureProjectorModel); break;
                case NotifyCollectionChangedAction.Remove: RemoveViewModel(e.OldItems[0] as StructureProjectorModel); break;
                case NotifyCollectionChangedAction.Reset: _projectors.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void AddViewModel(IStructureBase structureBase)
        {
            IStructureViewBase item;

            if (structureBase is StructureCharacterModel)
                item = new StructureCharacterViewModel(this, structureBase as StructureCharacterModel);
            else if (structureBase is StructureCubeGridModel)
                item = new StructureCubeGridViewModel(this, structureBase as StructureCubeGridModel);
            else if (structureBase is StructurePlanetModel)
                item = new StructurePlanetViewModel(this, structureBase as StructurePlanetModel);
            else if (structureBase is StructureVoxelModel)
                item = new StructureVoxelViewModel(this, structureBase as StructureVoxelModel);
            else if (structureBase is StructureFloatingObjectModel)
                item = new StructureFloatingObjectViewModel(this, structureBase as StructureFloatingObjectModel);
            else if (structureBase is StructureMeteorModel)
                item = new StructureMeteorViewModel(this, structureBase as StructureMeteorModel);
            else if (structureBase is StructureInventoryBagModel)
                item = new StructureInventoryBagViewModel(this, structureBase as StructureInventoryBagModel);
            else if (structureBase is StructureUnknownModel)
                item = new StructureUnknownViewModel(this, structureBase as StructureUnknownModel);
            else
            {
                throw new NotImplementedException("As yet undefined ViewModel has been called.");
            }

            _structures.Add(item);
            _preSelectedStructure = item;

            if (_selectNewStructure)
            {
                SelectedStructure = item;
            }
        }

        private void AddViewModel(StructurePlayerModel structureBase)
        {
            var item = new StructurePlayerViewModel(this, structureBase);

            _players.Add(item);
        }

        private void AddViewModel(StructureTimerModel structureBase)
        {
            var item = new StructureTimerViewModel(this, structureBase);

            _timers.Add(item);
        }

        private void AddViewModel(StructureProjectorModel structureBase)
        {
            var item = new StructureProjectorViewModel(this, structureBase);

            _projectors.Add(item);
        }
        
        /// <summary>
        /// Find and remove ViewModel, with the specied Model.
        /// Remove the Entity also.
        /// </summary>
        /// <param name="model"></param>
        private void RemoveViewModel(IStructureBase model)
        {
            var viewModel = Structures.FirstOrDefault(s => s.DataModel == model);
            if (viewModel != null && _dataModel.RemoveEntity(model.EntityBase))
            {
                Structures.Remove(viewModel);
            }
        }

        // remove Model from collection, causing sync to happen.
        public void DeleteModel(params IStructureViewBase[] viewModels)
        {
            int index = -1;
            if (viewModels.Length > 0)
            {
                index = Structures.IndexOf(viewModels[0]);
            }

            _ignoreUpdateSelection = true;

            foreach (var viewModel in viewModels)
            {
                bool canDelete = true;

                if (viewModel == null)
                {
                    canDelete = false;
                }
                else if (viewModel is StructureCharacterViewModel)
                {
                    canDelete = !((StructureCharacterViewModel)viewModel).IsPlayer;
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    canDelete = !((StructureCubeGridViewModel)viewModel).IsPiloted;
                }

                if (canDelete)
                {
                    viewModel.DataModel.CancelAsync();
                    _dataModel.Structures.Remove(viewModel.DataModel);
                }
            }

            _ignoreUpdateSelection = false;

            // Find and select next object
            while (index >= Structures.Count)
            {
                index--;
            }

            if (index > -1)
            {
                SelectedStructure = Structures[index];
            }
        }

        public void OptimizeModel(params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels.OfType<StructureCubeGridViewModel>())
            {
                _dataModel.OptimizeModel(viewModel.DataModel as StructureCubeGridModel);
            }
        }

        public void MirrorModel(bool oddMirror, params IStructureViewBase[] viewModels)
        {
            foreach (var model in viewModels.OfType<StructureCubeGridViewModel>())
            {
                ((StructureCubeGridModel)model.DataModel).MirrorModel(true, false);
            }
        }

        private void RejoinShipModels(IStructureViewBase viewModel1, IStructureViewBase viewModel2)
        {
            var ship1 = (StructureCubeGridViewModel)viewModel1;
            var ship2 = (StructureCubeGridViewModel)viewModel2;

            _dataModel.RejoinBrokenShip((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel);

            // Delete ship2.
            DeleteModel(viewModel2);

            // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
            // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.
        }

        private void MergeShipPartModels(IStructureViewBase viewModel1, IStructureViewBase viewModel2)
        {
            var ship1 = (StructureCubeGridViewModel)viewModel1;
            var ship2 = (StructureCubeGridViewModel)viewModel2;

            if (_dataModel.MergeShipParts((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel))
            {
                // Delete ship2.
                DeleteModel(viewModel2);

                // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
                // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.

                viewModel1.DataModel.UpdateGeneralFromEntityBase();
            }
        }

        private void RepairShips(IEnumerable<IStructureViewBase> structures)
        {
            foreach (var structure in structures)
            {
                if (structure.DataModel.ClassType == ClassType.SmallShip
                    || structure.DataModel.ClassType == ClassType.LargeShip
                    || structure.DataModel.ClassType == ClassType.LargeStation
                    || structure.DataModel.ClassType == ClassType.SmallStation)
                {
                    ((StructureCubeGridViewModel)structure).RepairObjectExecuted();
                }
            }
        }

        private void StopShips(IEnumerable<IStructureViewBase> structures)
        {
            foreach (var structure in structures)
            {
                if (structure.DataModel.ClassType == ClassType.SmallShip
                    || structure.DataModel.ClassType == ClassType.LargeShip
                    || structure.DataModel.ClassType == ClassType.LargeStation
                    || structure.DataModel.ClassType == ClassType.SmallStation)
                {
                    ((StructureCubeGridViewModel)structure).ResetVelocityExecuted();
                }
            }
        }

        private void ConvertToShips(IEnumerable<IStructureViewBase> structures)
        {
            foreach (var structure in structures)
            {
                if (structure.DataModel.ClassType == ClassType.LargeStation
                    || structure.DataModel.ClassType == ClassType.SmallStation)
                {
                    ((StructureCubeGridViewModel)structure).ConvertToShipExecuted();
                }
            }
        }

        private void ConvertToStations(IEnumerable<IStructureViewBase> structures)
        {
            foreach (var structure in structures)
            {
                if (structure.DataModel.ClassType == ClassType.SmallShip
                    || structure.DataModel.ClassType == ClassType.LargeShip)
                {
                    ((StructureCubeGridViewModel)structure).ConvertToStationExecuted();
                }
            }
        }

        /// <inheritdoc />
        public string CreateUniqueVoxelStorageName(string originalFile)
        {
            return _dataModel.CreateUniqueVoxelStorageName(originalFile, null);
        }

        public string CreateUniqueVoxelStorageName(string originalFile, MyObjectBuilder_EntityBase[] additionalList)
        {
            return _dataModel.CreateUniqueVoxelStorageName(originalFile, additionalList);
        }

        public List<IStructureBase> GetIntersectingEntities(BoundingBoxD box)
        {
            return _dataModel.Structures.Where(item => item.WorldAABB.Intersects(box)).ToList();
        }

        public void ImportSandboxObjectFromFile()
        {
            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = AppConstants.SandboxObjectFilter;
            openFileDialog.Title = Res.DialogImportSandboxObjectTitle;
            openFileDialog.Multiselect = true;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                var badfiles = _dataModel.LoadEntities(openFileDialog.FileNames);

                foreach (var filename in badfiles)
                {
                    _dialogService.ShowMessageBox(this, string.Format(Res.ClsImportInvalid, Path.GetFileName(filename)), Res.ClsImportTitleFailed, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void ExportSandboxObjectToFile(bool blankOwnerAndMedBays, params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                if (viewModel is StructureCharacterViewModel)
                {
                    var structure = (StructureCharacterViewModel)viewModel;

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = AppConstants.SandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.Description);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureVoxelViewModel)
                {
                    var structure = (StructureVoxelViewModel)viewModel;
                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = AppConstants.VoxelFilter;
                    saveFileDialog.Title = Res.DialogExportVoxelTitle;
                    saveFileDialog.FileName = structure.Name + MyVoxelMap.V2FileExtension;
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        var asteroid = (StructureVoxelModel)structure.DataModel;
                        string sourceFile;

                        if (asteroid.SourceVoxelFilepath != null)
                            sourceFile = asteroid.SourceVoxelFilepath;  // Source Voxel file is temporary. Hasn't been saved yet.
                        else
                            sourceFile = asteroid.VoxelFilepath;  // Source Voxel file exists.
                        File.Copy(sourceFile, saveFileDialog.FileName, true);
                    }
                }
                else if (viewModel is StructureFloatingObjectViewModel)
                {
                    var structure = (StructureFloatingObjectViewModel)viewModel;

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = AppConstants.SandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureMeteorViewModel)
                {
                    var structure = (StructureMeteorViewModel)viewModel;

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = AppConstants.SandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    var structure = (StructureCubeGridViewModel)viewModel;

                    var partname = string.IsNullOrEmpty(structure.DisplayName) ? structure.EntityId.ToString() : structure.DisplayName.Replace("|", "_").Replace("\\", "_").Replace("/", "_");
                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = AppConstants.SandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, partname);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, partname);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        var cloneEntity = (MyObjectBuilder_CubeGrid)viewModel.DataModel.EntityBase.Clone();

                        if (blankOwnerAndMedBays)
                        {
                            // Call to ToArray() to force Linq to update the value.

                            // Clear Medical room SteamId.
                            cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersTypes.MedicalRoom).Select(c => { ((MyObjectBuilder_MedicalRoom)c).SteamUserId = 0; return c; }).ToArray();

                            // Clear Owners.
                            cloneEntity.CubeBlocks.Select(c => { c.Owner = 0; c.ShareMode = MyOwnershipShareModeEnum.None; return c; }).ToArray();
                        }

                        // Remove any pilots.
                        cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersTypes.Cockpit).Select(c =>
                        {
                            ((MyObjectBuilder_Cockpit)c).ClearPilotAndAutopilot();
                            ((MyObjectBuilder_Cockpit)c).PilotRelativeWorld = null;
                            return c;
                        }).ToArray();

                        _dataModel.SaveEntity(cloneEntity, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureInventoryBagViewModel)
                {
                    // Need to use the specific serializer when exporting to generate the correct XML, so Unknown should never be export.
                    _dialogService.ShowMessageBox(this, Res.ClsExportInventoryBag, Res.ClsExportTitleFailed, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (viewModel is StructurePlanetViewModel)
                {
                    // Too complex to export without work to package the data,
                    _dialogService.ShowMessageBox(this, Res.ClsExportPlanet, Res.ClsExportTitleFailed, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (viewModel is StructureUnknownViewModel)
                {
                    // Need to use the specific serializer when exporting to generate the correct XML, so Unknown should never be export.
                    _dialogService.ShowMessageBox(this, Res.ClsExportUnknown, Res.ClsExportTitleFailed, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
        }


        public void ExportPrefabObjectToFile(bool blankOwnerAndMedBays, params IStructureViewBase[] viewModels)
        {
            var saveFileDialog = _saveFileDialogFactory();
            saveFileDialog.Filter = AppConstants.PrefabObjectFilter;
            saveFileDialog.Title = Res.DialogExportSandboxObjectTitle;
            saveFileDialog.FileName = "export prefab.sbc";
            saveFileDialog.OverwritePrompt = true;

            if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
            {
                var definition = new MyObjectBuilder_Definitions();
                definition.Prefabs = new MyObjectBuilder_PrefabDefinition[1];
                MyObjectBuilder_PrefabDefinition prefab;
                prefab = new MyObjectBuilder_PrefabDefinition();
                prefab.Id.TypeId = new MyObjectBuilderType(typeof(MyObjectBuilder_PrefabDefinition));
                prefab.Id.SubtypeId = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                var grids = new List<MyObjectBuilder_CubeGrid>();

                foreach (var viewModel in viewModels)
                {
                    if (viewModel is StructureCubeGridViewModel)
                    {
                        var cloneEntity = (MyObjectBuilder_CubeGrid)viewModel.DataModel.EntityBase.Clone();

                        if (blankOwnerAndMedBays)
                        {
                            // Call to ToArray() to force Linq to update the value.

                            // Clear Medical room SteamId.
                            cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersTypes.MedicalRoom).Select(c => { ((MyObjectBuilder_MedicalRoom)c).SteamUserId = 0; return c; }).ToArray();

                            // Clear Owners.
                            cloneEntity.CubeBlocks.Select(c => { c.Owner = 0; c.ShareMode = MyOwnershipShareModeEnum.None; return c; }).ToArray();
                        }

                        // Remove any pilots.
                        cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersTypes.Cockpit).Select(c =>
                        {
                            ((MyObjectBuilder_Cockpit)c).ClearPilotAndAutopilot();
                            ((MyObjectBuilder_Cockpit)c).PilotRelativeWorld = null;
                            return c;
                        }).ToArray();

                        grids.Add(cloneEntity);
                    }
                }

                prefab.CubeGrids = grids.ToArray();
                definition.Prefabs[0] = prefab;

                SpaceEngineersApi.WriteSpaceEngineersFile(definition, saveFileDialog.FileName);
            }
        }

        public void TestCalcCubesModel(params IStructureViewBase[] viewModels)
        {
            var bld = new StringBuilder();

            foreach (var viewModel in viewModels.OfType<StructureCubeGridViewModel>())
            {
                var model = viewModel.DataModel as StructureCubeGridModel;
                //var list = model.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("Red") ||
                //    b.SubtypeName.Contains("Blue") ||
                //    b.SubtypeName.Contains("Green") ||
                //    b.SubtypeName.Contains("Yellow") ||
                //    b.SubtypeName.Contains("White") ||
                //    b.SubtypeName.Contains("Black")).ToArray();

                var list = model.CubeGrid.CubeBlocks.Where(b => b is MyObjectBuilder_Cockpit).ToArray();
                //var list = model.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("Conveyor")).ToArray();

                foreach (var b in list)
                {
                    CubeType cubeType = CubeType.Exterior;

                    //if (b.SubtypeName.Contains("ArmorSlope"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("Slope")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}
                    //else if (b.SubtypeName.Contains("ArmorCornerInv"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("InverseCorner")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}
                    //else if (b.SubtypeName.Contains("ArmorCorner"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("NormalCorner")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}

                    //SpaceEngineersAPI.CubeOrientations

                    // XYZ= (7, 15, 3)   Orientation = (0, 0, 0, 1)  SmallBlockArmorSlopeBlue               CubeType.SlopeCenterBackBottom
                    // XYZ= (8, 14, 3)   Orientation = (1, 0, 0, 0)  SmallBlockArmorCornerInvBlue           CubeType.InverseCornerRightFrontTop
                    // XYZ= (8, 15, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorCornerBlue     CubeType.NormalCornerLeftBackBottom

                    // XYZ= (13, 9, 3)   Orientation = (1, 0, 0, 0)  SmallBlockArmorCornerInvGreen          CubeType.InverseCornerRightFrontTop
                    // XYZ= (14, 8, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorSlopeGreen     CubeType.SlopeLeftCenterBottom
                    // XYZ= (14, 9, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorCornerGreen        CubeType.NormalCornerLeftBackBottom


                    bld.AppendFormat("// XYZ= ({0}, {1}, {2})   Orientation = ({3}, {4})  {5}    CubeType.{6}\r\n",
                        b.Min.X, b.Min.Y, b.Min.Z, b.BlockOrientation.Forward, b.BlockOrientation.Up, b.SubtypeName, cubeType);
                }
            }
            Debug.Write(bld.ToString());
        }

        public void CalcDistances()
        {
            _dataModel.CalcDistances();
        }

        private void UpdateLanguages()
        {
            var list = new List<LanguageModel>();

            foreach (var kvp in AppConstants.SupportedLanguages)
            {
                var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(kvp.Key);
                list.Add(new LanguageModel { IetfLanguageTag = culture.IetfLanguageTag, LanguageName = culture.DisplayName, NativeName = culture.NativeName, ImageName = kvp.Value });
            }

            Languages = new ObservableCollection<LanguageModel>(list);
        }

        public IStructureBase AddEntity(MyObjectBuilder_EntityBase entity)
        {
            return _dataModel.AddEntity(entity);
        }

        #endregion

        #region IDragable Interface

        Type IDropable.DataType
        {
            get { return typeof(DataBaseViewModel); }
        }

        void IDropable.Drop(object data, int index)
        {
            _dataModel.MergeData((IList<IStructureBase>)data);
        }

        #endregion

        #region IMainView Interface

        public bool ShowProgress
        {
            get { return _dataModel.ShowProgress; }
            set { _dataModel.ShowProgress = value; }
        }

        public double Progress
        {
            get { return _dataModel.Progress; }
            set { _dataModel.Progress = value; }
        }

        public TaskbarItemProgressState ProgressState
        {
            get { return _dataModel.ProgressState; }
            set { _dataModel.ProgressState = value; }
        }

        public double ProgressValue
        {
            get { return _dataModel.ProgressValue; }
            set { _dataModel.ProgressValue = value; }
        }

        public double MaximumProgress
        {
            get { return _dataModel.MaximumProgress; }
            set { _dataModel.MaximumProgress = value; }
        }

        public void ResetProgress(double initial, double maximumProgress)
        {
            _dataModel.ResetProgress(initial, maximumProgress);
        }

        public void ClearProgress()
        {
            _dataModel.ClearProgress();
        }

        public void IncrementProgress()
        {
            _dataModel.IncrementProgress();
        }

        public MyObjectBuilder_Checkpoint Checkpoint
        {
            get { return ActiveWorld.Checkpoint; }
        }

        /// <summary>
        /// Read in current 'world' color Palette.
        /// </summary>
        public int[] CreativeModeColors
        {
            get { return _dataModel.CreativeModeColors; }
            set { _dataModel.CreativeModeColors = value; }
        }

        #endregion
    }
}