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
    using Res = SEToolbox.Properties.Resources;

    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
    {
        #region fields

        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;
        private Lazy<ObservableCollection<CubeItemViewModel>> _cubeList;
        private ObservableCollection<CubeItemViewModel> _selections;
        private CubeItemViewModel _selectedCubeItem;
        private string[] _filerView;

        #endregion

        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
            Selections = new ObservableCollection<CubeItemViewModel>();
        }

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            _dialogService = dialogService;
            _colorDialogFactory = colorDialogFactory;

            Func<CubeItemModel, CubeItemViewModel> viewModelCreator = model => new CubeItemViewModel(this, model);
            Func<ObservableCollection<CubeItemViewModel>> collectionCreator =
                () => new ObservableViewModelCollection<CubeItemViewModel, CubeItemModel>(dataModel.CubeList, viewModelCreator);
            _cubeList = new Lazy<ObservableCollection<CubeItemViewModel>>(collectionCreator);

            DataModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "CubeList")
                {
                    collectionCreator.Invoke();
                    _cubeList = new Lazy<ObservableCollection<CubeItemViewModel>>(collectionCreator);
                }
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand OptimizeObjectCommand => new DelegateCommand(OptimizeObjectExecuted, OptimizeObjectCanExecute);

        public ICommand RepairObjectCommand => new DelegateCommand(RepairObjectExecuted, RepairObjectCanExecute);

        public ICommand ResetLinearVelocityCommand => new DelegateCommand(ResetLinearVelocityExecuted, ResetLinearVelocityCanExecute);

        public ICommand ResetRotationVelocityCommand => new DelegateCommand(ResetRotationVelocityExecuted, ResetRotationVelocityCanExecute);

        public ICommand ResetVelocityCommand => new DelegateCommand(ResetVelocityExecuted, ResetVelocityCanExecute);

        public ICommand ReverseVelocityCommand => new DelegateCommand(ReverseVelocityExecuted, ReverseVelocityCanExecute);

        public ICommand MaxVelocityAtPlayerCommand => new DelegateCommand(MaxVelocityAtPlayerExecuted, MaxVelocityAtPlayerCanExecute);

        public ICommand ConvertCommand => new DelegateCommand(ConvertExecuted, ConvertCanExecute);

        public ICommand ConvertToHeavyArmorCommand => new DelegateCommand(ConvertToHeavyArmorExecuted, ConvertToHeavyArmorCanExecute);

        public ICommand ConvertToLightArmorCommand => new DelegateCommand(ConvertToLightArmorExecuted, ConvertToLightArmorCanExecute);

        public ICommand FrameworkCommand => new DelegateCommand(FrameworkExecuted, FrameworkCanExecute);

        public ICommand ConvertToFrameworkCommand => new DelegateCommand<double>(ConvertToFrameworkExecuted, ConvertToFrameworkCanExecute);

        public ICommand ConvertToStationCommand => new DelegateCommand(ConvertToStationExecuted, ConvertToStationCanExecute);

        public ICommand ReorientStationCommand => new DelegateCommand(ReorientStationExecuted, ReorientStationCanExecute);

        public ICommand RotateStructureYawPositiveCommand => new DelegateCommand(RotateStructureYawPositiveExecuted, RotateStructureYawPositiveCanExecute);

        public ICommand RotateStructureYawNegativeCommand => new DelegateCommand(RotateStructureYawNegativeExecuted, RotateStructureYawNegativeCanExecute);

        public ICommand RotateStructurePitchPositiveCommand => new DelegateCommand(RotateStructurePitchPositiveExecuted, RotateStructurePitchPositiveCanExecute);

        public ICommand RotateStructurePitchNegativeCommand => new DelegateCommand(RotateStructurePitchNegativeExecuted, RotateStructurePitchNegativeCanExecute);

        public ICommand RotateStructureRollPositiveCommand => new DelegateCommand(RotateStructureRollPositiveExecuted, RotateStructureRollPositiveCanExecute);

        public ICommand RotateStructureRollNegativeCommand => new DelegateCommand(RotateStructureRollNegativeExecuted, RotateStructureRollNegativeCanExecute);

        public ICommand RotateCubesYawPositiveCommand => new DelegateCommand(RotateCubesYawPositiveExecuted, RotateCubesYawPositiveCanExecute);

        public ICommand RotateCubesYawNegativeCommand => new DelegateCommand(RotateCubesYawNegativeExecuted, RotateCubesYawNegativeCanExecute);

        public ICommand RotateCubesPitchPositiveCommand => new DelegateCommand(RotateCubesPitchPositiveExecuted, RotateCubesPitchPositiveCanExecute);

        public ICommand RotateCubesPitchNegativeCommand => new DelegateCommand(RotateCubesPitchNegativeExecuted, RotateCubesPitchNegativeCanExecute);

        public ICommand RotateCubesRollPositiveCommand => new DelegateCommand(RotateCubesRollPositiveExecuted, RotateCubesRollPositiveCanExecute);

        public ICommand RotateCubesRollNegativeCommand => new DelegateCommand(RotateCubesRollNegativeExecuted, RotateCubesRollNegativeCanExecute);

        public ICommand ConvertToShipCommand => new DelegateCommand(ConvertToShipExecuted, ConvertToShipCanExecute);

        public ICommand ConvertToCornerArmorCommand => new DelegateCommand(ConvertToCornerArmorExecuted, ConvertToCornerArmorCanExecute);

        public ICommand ConvertToRoundArmorCommand => new DelegateCommand(ConvertToRoundArmorExecuted, ConvertToRoundArmorCanExecute);

        public ICommand MirrorStructureByPlaneCommand => new DelegateCommand(MirrorStructureByPlaneExecuted, MirrorStructureByPlaneCanExecute);

        public ICommand MirrorStructureGuessOddCommand => new DelegateCommand(MirrorStructureGuessOddExecuted, MirrorStructureGuessOddCanExecute);

        public ICommand MirrorStructureGuessEvenCommand => new DelegateCommand(MirrorStructureGuessEvenExecuted, MirrorStructureGuessEvenCanExecute);

        public ICommand CopyDetailCommand => new DelegateCommand(CopyDetailExecuted, CopyDetailCanExecute);

        public ICommand FilterStartCommand => new DelegateCommand(FilterStartExecuted, FilterStartCanExecute);

        public ICommand FilterTabStartCommand => new DelegateCommand(FilterTabStartExecuted, FilterTabStartCanExecute);

        public ICommand FilterClearCommand => new DelegateCommand(FilterClearExecuted, FilterClearCanExecute);

        public ICommand DeleteCubesCommand => new DelegateCommand(DeleteCubesExecuted, DeleteCubesCanExecute);

        public ICommand ReplaceCubesCommand => new DelegateCommand(ReplaceCubesExecuted, ReplaceCubesCanExecute);

        public ICommand ColorCubesCommand => new DelegateCommand(ColorCubesExecuted, ColorCubesCanExecute);

        public ICommand FrameworkCubesCommand => new DelegateCommand(FrameworkCubesExecuted, FrameworkCubesCanExecute);

        public ICommand SetOwnerCommand => new DelegateCommand(SetOwnerExecuted, SetOwnerCanExecute);

        public ICommand SetBuiltByCommand => new DelegateCommand(SetBuiltByExecuted, SetBuiltByCanExecute);

        public ICommand CopyCubeGpsCommand
        {
            get { return new DelegateCommand(CopyCubeGpsExecuted, CopyCubeGpsCanExecute); }
        }

        public ICommand UseAsPivotCommand
        {
            get { return new DelegateCommand(UseAsPivotExecuted, UseAsPivotCanExecute); }
        }

        #endregion

        #region Properties

        public ObservableCollection<CubeItemViewModel> CubeList
        {
            get { return _cubeList.Value; }
        }

        public ObservableCollection<CubeItemViewModel> Selections
        {
            get { return _selections; }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    RaisePropertyChanged(() => Selections);
                }
            }
        }

        public CubeItemViewModel SelectedCubeItem
        {
            get { return _selectedCubeItem; }

            set
            {
                if (value != _selectedCubeItem)
                {
                    _selectedCubeItem = value;
                    RaisePropertyChanged(() => SelectedCubeItem);
                }
            }
        }

        protected new StructureCubeGridModel DataModel
        {
            get { return base.DataModel as StructureCubeGridModel; }
        }

        public bool IsDamaged
        {
            get { return DataModel.IsDamaged; }
        }

        public int DamageCount
        {
            get { return DataModel.DamageCount; }
        }

        public MyCubeSize GridSize
        {
            get { return DataModel.GridSize; }
            set { DataModel.GridSize = value; }
        }

        public bool IsStatic
        {
            get { return DataModel.IsStatic; }
            set { DataModel.IsStatic = value; }
        }

        public bool Dampeners
        {
            get { return DataModel.Dampeners; }

            set
            {
                DataModel.Dampeners = value;
                MainViewModel.IsModified = true;
            }
        }

        public bool Destructible
        {
            get { return DataModel.Destructible; }

            set
            {
                DataModel.Destructible = value;
                MainViewModel.IsModified = true;
            }
        }

        public Point3D Min
        {
            get { return DataModel.Min; }
            set { DataModel.Min = value; }
        }

        public Point3D Max
        {
            get { return DataModel.Max; }
            set { DataModel.Max = value; }
        }

        public Vector3D Scale
        {
            get { return DataModel.Scale; }
            set { DataModel.Scale = value; }
        }

        public BindableSize3DModel Size
        {
            get { return new BindableSize3DModel(DataModel.Size); }
        }

        public BindableVector3DModel Center
        {
            get { return new BindableVector3DModel(DataModel.Center); }
            set { DataModel.Center = value.ToVector3(); }
        }

        public bool IsPiloted
        {
            get { return DataModel.IsPiloted; }
        }

        public override double LinearVelocity
        {
            get { return DataModel.LinearVelocity; }
        }

        public double AngularVelocity
        {
            get { return DataModel.AngularVelocity; }
        }

        public TimeSpan TimeToProduce
        {
            get { return DataModel.TimeToProduce; }
            set { DataModel.TimeToProduce = value; }
        }

        public string CockpitOrientation
        {
            get { return DataModel.CockpitOrientation; }
        }

        public List<CubeAssetModel> CubeAssets
        {
            get { return DataModel.CubeAssets; }
            set { DataModel.CubeAssets = value; }
        }

        public List<CubeAssetModel> ComponentAssets
        {
            get { return DataModel.ComponentAssets; }
            set { DataModel.ComponentAssets = value; }
        }

        public List<OreAssetModel> IngotAssets
        {
            get { return DataModel.IngotAssets; }
            set { DataModel.IngotAssets = value; }
        }

        public List<OreAssetModel> OreAssets
        {
            get { return DataModel.OreAssets; }
            set { DataModel.OreAssets = value; }
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

        public string BuilderName
        {
            get
            {
                return DataModel.BuilderName;
            }
        }
        #endregion

        #region command methods

        public bool OptimizeObjectCanExecute()
        {
            return true;
        }

        public void OptimizeObjectExecuted()
        {
            MainViewModel.OptimizeModel(this);
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RepairObjectCanExecute()
        {
            return IsDamaged;
        }

        public void RepairObjectExecuted()
        {
            DataModel.RepairAllDamage();
            MainViewModel.IsModified = true;
        }

        public bool ResetLinearVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f;
        }

        public void ResetLinearVelocityExecuted()
        {
            DataModel.ResetLinearVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ResetRotationVelocityCanExecute()
        {
            return DataModel.AngularVelocity != 0f;
        }

        public void ResetRotationVelocityExecuted()
        {
            DataModel.ResetRotationVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ResetVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularVelocity != 0f;
        }

        public void ResetVelocityExecuted()
        {
            DataModel.ResetVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularVelocity != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            DataModel.ReverseVelocity();
            MainViewModel.IsModified = true;
        }

        public bool MaxVelocityAtPlayerCanExecute()
        {
            return MainViewModel.ThePlayerCharacter != null;
        }

        public void MaxVelocityAtPlayerExecuted()
        {

            var position = MainViewModel.ThePlayerCharacter.PositionAndOrientation.Value.Position;
            DataModel.MaxVelocityAtPlayer(position);
            MainViewModel.IsModified = true;
        }

        public bool ConvertCanExecute()
        {
            return true;
        }

        public void ConvertExecuted()
        {
        }

        public bool ConvertToHeavyArmorCanExecute()
        {
            return true;
        }

        public void ConvertToHeavyArmorExecuted()
        {
            if (DataModel.ConvertFromLightToHeavyArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToLightArmorCanExecute()
        {
            return true;
        }

        public void ConvertToLightArmorExecuted()
        {
            if (DataModel.ConvertFromHeavyToLightArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool FrameworkCanExecute()
        {
            return true;
        }

        public void FrameworkExecuted()
        {
            // placeholder for menu only.
        }

        public bool ConvertToFrameworkCanExecute(double value)
        {
            return true;
        }

        public void ConvertToFrameworkExecuted(double value)
        {
            DataModel.ConvertToFramework((float)value);
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool ConvertToStationCanExecute()
        {
            return !DataModel.IsStatic;
        }

        public void ConvertToStationExecuted()
        {
            DataModel.ConvertToStation();
            MainViewModel.IsModified = true;
        }

        public bool ReorientStationCanExecute()
        {
            return true;
        }

        public void ReorientStationExecuted()
        {
            DataModel.ReorientStation();
            MainViewModel.IsModified = true;
        }

        public bool RotateStructureYawPositiveCanExecute()
        {
            return true;
        }

        public void RotateStructurePitchPositiveExecuted()
        {
            // +90 around X
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(0, VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateStructurePitchNegativeCanExecute()
        {
            return true;
        }

        public void RotateStructurePitchNegativeExecuted()
        {
            // -90 around X
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(0, -VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateStructureRollPositiveCanExecute()
        {
            return true;
        }

        public void RotateStructureYawPositiveExecuted()
        {
            // +90 around Y
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateStructureYawNegativeCanExecute()
        {
            return true;
        }

        public void RotateStructureYawNegativeExecuted()
        {
            // -90 around Y
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(-VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateStructurePitchPositiveCanExecute()
        {
            return true;
        }

        public void RotateStructureRollPositiveExecuted()
        {
            // +90 around Z
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateStructureRollNegativeCanExecute()
        {
            return true;
        }

        public void RotateStructureRollNegativeExecuted()
        {
            // -90 around Z
            DataModel.RotateStructure(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesYawPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchPositiveExecuted()
        {
            // +90 around X
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchNegativeExecuted()
        {
            // -90 around X
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, -VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesRollPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesYawPositiveExecuted()
        {
            // +90 around Y
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesYawNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesYawNegativeExecuted()
        {
            // -90 around Y
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(-VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesRollPositiveExecuted()
        {
            // +90 around Z
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesRollNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesRollNegativeExecuted()
        {
            // -90 around Z
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool ConvertToShipCanExecute()
        {
            return DataModel.IsStatic;
        }

        public void ConvertToShipExecuted()
        {
            DataModel.ConvertToShip();
            MainViewModel.IsModified = true;
        }

        public bool ConvertToCornerArmorCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToCornerArmorExecuted()
        {
            if (DataModel.ConvertToCornerArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToRoundArmorCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToRoundArmorExecuted()
        {
            if (DataModel.ConvertToRoundArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool MirrorStructureByPlaneCanExecute()
        {
            return true;
        }

        public void MirrorStructureByPlaneExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(true, false))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessOddCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessOddExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(false, true))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessEvenCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessEvenExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(false, false))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool CopyDetailCanExecute()
        {
            return true;
        }

        public void CopyDetailExecuted()
        {
            var cubes = new StringBuilder();
            if (CubeAssets != null)
            {
                foreach (var mat in CubeAssets)
                {
                    cubes.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} {3}\t{4:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, Res.GlobalSIMassKilogram, mat.Time);
                }
            }

            var components = new StringBuilder();
            if (ComponentAssets != null)
            {
                foreach (var mat in ComponentAssets)
                {
                    components.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0} {3}\t{4:#,##0.00} {5}\t{6:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, Res.GlobalSIMassKilogram, mat.Volume, Res.GlobalSIMassKilogram,  mat.Time);
                }
            }

            var ingots = new StringBuilder();
            if (IngotAssets != null)
            {
                foreach (var mat in IngotAssets)
                {
                    ingots.AppendFormat("{0}\t{1:#,##0.00}\t{2:#,##0.00} {3}\t{4:#,##0.00} {5}\t{6:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Amount, mat.Mass, Res.GlobalSIMassKilogram, mat.Volume, Res.GlobalSIMassKilogram, mat.Time);
                }
            }

            var ores = new StringBuilder();
            if (OreAssets != null)
            {
                foreach (var mat in OreAssets)
                {
                    ores.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} {3}\t{4:#,##0.00} {5}\r\n", mat.FriendlyName, mat.Amount, mat.Mass, Res.GlobalSIMassKilogram, mat.Volume, Res.GlobalSIMassKilogram);
                }
            }

            var detail = string.Format(Properties.Resources.CtlCubeDetail,
                DisplayName,
                ClassType,
                IsPiloted,
                DamageCount,
                LinearVelocity,
                PlayerDistance,
                Scale.X, Scale.Y, Scale.Z,
                Size.Width, Size.Height, Size.Depth,
                Mass,
                BlockCount,
                PositionAndOrientation.Value.Position.X, PositionAndOrientation.Value.Position.Y, PositionAndOrientation.Value.Position.Z,
                TimeToProduce,
                cubes.ToString(),
                components.ToString(),
                ingots.ToString(),
                ores.ToString());

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(detail);
            }
            catch
            {
                // Ignore exception which may be generated by a Remote desktop session where Clipboard access has not been granted.
            }
        }

        public bool FilterStartCanExecute()
        {
            return ActiveComponentFilter != ComponentFilter;
        }

        public void FilterStartExecuted()
        {
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
        }

        public bool FilterTabStartCanExecute()
        {
            return true;
        }

        public void FilterTabStartExecuted()
        {
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
            FrameworkExtension.FocusedElementMoveFocus();
        }

        public bool FilterClearCanExecute()
        {
            return !string.IsNullOrEmpty(ComponentFilter);
        }

        public void FilterClearExecuted()
        {
            ComponentFilter = string.Empty;
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
        }

        public bool DeleteCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void DeleteCubesExecuted()
        {
            IsBusy = true;

            MainViewModel.ResetProgress(0, Selections.Count);

            while (Selections.Count > 0)
            {
                MainViewModel.Progress++;
                var cube = Selections[0];
                if (DataModel.CubeGrid.CubeBlocks.Remove(cube.Cube))
                    DataModel.CubeList.Remove(cube.DataModel);
            }

            MainViewModel.ClearProgress();
            IsBusy = false;
        }

        public bool ReplaceCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void ReplaceCubesExecuted()
        {
            var model = new SelectCubeModel();
            var loadVm = new SelectCubeViewModel(this, model);
            model.Load(GridSize, SelectedCubeItem.Cube.TypeId, SelectedCubeItem.SubtypeId);
            var result = _dialogService.ShowDialog<WindowSelectCube>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                var contentPath = ToolboxUpdater.GetApplicationContentPath();
                var change = false;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    if (cube.TypeId != model.CubeItem.TypeId || cube.SubtypeId != model.CubeItem.SubtypeId)
                    {
                        var idx = DataModel.CubeGrid.CubeBlocks.IndexOf(cube.Cube);
                        DataModel.CubeGrid.CubeBlocks.RemoveAt(idx);

                        var cubeDefinition = SpaceEngineersApi.GetCubeDefinition(model.CubeItem.TypeId, GridSize, model.CubeItem.SubtypeId);
                        var newCube = cube.CreateCube(model.CubeItem.TypeId, model.CubeItem.SubtypeId, cubeDefinition);
                        cube.TextureFile = (cubeDefinition.Icons == null || cubeDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icons.First(), Path.Combine(contentPath, cubeDefinition.Icons.First()));
                        DataModel.CubeGrid.CubeBlocks.Insert(idx, newCube);

                        change = true;
                    }
                }

                MainViewModel.ClearProgress();
                if (change)
                {
                    MainViewModel.IsModified = true;
                }
                MainViewModel.IsBusy = false;
            }
        }

        public bool ColorCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void ColorCubesExecuted()
        {
            var colorDialog = _colorDialogFactory();
            colorDialog.FullOpen = true;
            colorDialog.BrushColor = SelectedCubeItem.Color as System.Windows.Media.SolidColorBrush;
            colorDialog.CustomColors = MainViewModel.CreativeModeColors;

            if (_dialogService.ShowColorDialog(OwnerViewModel, colorDialog) == System.Windows.Forms.DialogResult.OK)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    if (colorDialog.DrawingColor.HasValue)
                        cube.UpdateColor(colorDialog.DrawingColor.Value.ToSandboxHsvColor());
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }

            MainViewModel.CreativeModeColors = colorDialog.CustomColors;
        }

        public bool FrameworkCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void FrameworkCubesExecuted()
        {
            var model = new FrameworkBuildModel { BuildPercent = SelectedCubeItem.BuildPercent * 100 };
            var loadVm = new FrameworkBuildViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowFrameworkBuild>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    cube.UpdateBuildPercent(model.BuildPercent.Value / 100);
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }
        }

        public bool SetOwnerCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void SetOwnerExecuted()
        {
            var model = new ChangeOwnerModel();
            model.Title = Res.WnChangeOwnerTitle;
            model.Load(SelectedCubeItem.Owner);
            var loadVm = new ChangeOwnerViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowChangeOwner>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    cube.ChangeOwner(model.SelectedPlayer.PlayerId);
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }
        }

        public bool UseAsPivotCanExecute()
        {
            return Selections.Count == 1;
        }

        public void UseAsPivotExecuted()
        {
            MainViewModel.IsBusy = true;
            MainViewModel.ResetProgress(0, CubeList.Count + DataModel.CubeGrid.BlockGroups.SelectMany(x => x.Blocks).Count());
            // rotate the grid's cubes towards the reference block's forward and up, compensate by turning the grid inversely
            // the grid will stay in the same position and orientation in the world
            // the blocks will be in the same position and orientation in reference to each other
            // basically we turned only the pivot point towads the reference blocks' forward and up
            DataModel.RotateCubes(VRageMath.Quaternion.Inverse(SelectedCubeItem.Cube.BlockOrientation.ToQuaternion()));
            // double-check that the reference is pointing towards forward and up, it's crucial for the next steps
            if (SelectedCubeItem.Cube.BlockOrientation.Forward != VRageMath.Base6Directions.Direction.Forward || SelectedCubeItem.Cube.BlockOrientation.Up != VRageMath.Base6Directions.Direction.Up)
                throw new InvalidOperationException("Forward must point to Forward and Up must point Up.");
            // reposition the blocks so the reference block is at the 0,0,0 grid coordinate
            var pivotPos = SelectedCubeItem.Cube.Min;
            foreach (var cube in CubeList)
            {
                MainViewModel.Progress++;
                // reposition block
                cube.RepositionAround(pivotPos);
            }
            // adjust blockgroups
            foreach (var bg in DataModel.CubeGrid.BlockGroups)
            {
                var newList = new List<VRageMath.Vector3I>();
                for (var i = 0; i < bg.Blocks.Count; i++)
                {
                    MainViewModel.Progress++;
                    newList.Add(new VRageMath.Vector3I(bg.Blocks[i].X - pivotPos.X, bg.Blocks[i].Y - pivotPos.Y, bg.Blocks[i].Z - pivotPos.Z));
                }
                bg.Blocks = newList;
            }
            // move the reference block to the very beginning of the cubelist
            // (since 01.142, the blueprint's first block is placed on the projector at 0-0-0 setting)
            DataModel.CubeGrid.CubeBlocks.Move(DataModel.CubeGrid.CubeBlocks.IndexOf(SelectedCubeItem.Cube), 0);
            // move the grid so it's at the same world position as before           
            DataModel.MoveGridToCubePos(pivotPos);
            //
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
            MainViewModel.ClearProgress();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        private bool CopyCubeGpsCanExecute()
        {
            return Selections.Count == 1;
        }

        private void CopyCubeGpsExecuted()
        {
            var grid = Selections[0].OwnerViewModel as StructureCubeGridViewModel;
            var pos1 = (VRageMath.Vector3D)grid.PositionAndOrientation.Value.Position;
            var orient1 = grid.PositionAndOrientation.Value.ToQuaternion();
            var multi1 = grid.GridSize.ToLength();
            pos1 += VRageMath.Vector3.Transform(Selections[0].Position.ToVector3I() * multi1, orient1);
            var cubeCoord = new VRage.MyPositionAndOrientation(pos1, orient1.Forward, orient1.Up);
            string caption;
            var fb = Selections[0].Cube as MyObjectBuilder_FunctionalBlock;
            if (fb == null)
                caption = $"{Selections[0].FriendlyName} {Selections[0].Position}";
            else
                caption = fb.CustomName ?? Selections[0].FriendlyName;
            var text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "GPS:{0}:{1:0.00}:{2:0.00}:{3:0.00}:", caption, cubeCoord.Position.X, cubeCoord.Position.Y, cubeCoord.Position.Z);
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

        public bool SetBuiltByCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void SetBuiltByExecuted()
        {
            var model = new ChangeOwnerModel();
            model.Title = Res.WnChangeBuiltByTitle;
            model.Load(SelectedCubeItem.BuiltBy);
            var loadVm = new ChangeOwnerViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowChangeOwner>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    cube.ChangeBuiltBy(model.SelectedPlayer.PlayerId);
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }
        }

        #endregion

        #region methods

        private void ApplyCubeFilter()
        {
            // Prepare filter beforehand.
            if (string.IsNullOrEmpty(ActiveComponentFilter))
                _filerView = new string[0];
            else
                _filerView = ActiveComponentFilter.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(CubeList);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (_filerView.Length == 0)
                return true;

            var cube = (CubeItemViewModel)item;
            return _filerView.All(s => (cube.FriendlyName != null && cube.FriendlyName.ToLowerInvariant().Contains(s)) || cube.ColorText.ToLowerInvariant().Contains(s));
        }

        #endregion
    }
}
