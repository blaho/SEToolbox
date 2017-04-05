namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using System.Text;
    using Services;

    public class StructurePlayerModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private string _activeComponentFilter;

        [NonSerialized]
        private string _componentFilter;

        [NonSerialized]
        private static readonly object Locker = new object();

        [NonSerialized]
        private bool _isSubsSystemNotReady;

        [NonSerialized]
        private bool _isConstructionNotReady;

        [NonSerialized]
        private string _blockCountDetails;

        [NonSerialized]
        private int _assemblerCount;

        [NonSerialized]
        private string _assemblerDetails;

        [NonSerialized]
        private int _refineryCount;

        [NonSerialized]
        private string _refineryDetails;

        [NonSerialized]
        private int _shipToolCount;

        [NonSerialized]
        private string _shipToolDetails;

        [NonSerialized]
        private int _powerBlockCount;

        [NonSerialized]
        private string _powerBlockDetails;

        [NonSerialized]
        private int _thrusterCount;

        [NonSerialized]
        private string _thrusterDetails;

        [NonSerialized]
        private int _turretCount;

        [NonSerialized]
        private string _turretDetails;

        #endregion

        #region ctor

        public StructurePlayerModel(long builtBy, IEnumerable<MyObjectBuilder_CubeBlock> cubes)
            : base(null)
        {
            IsSubsSystemNotReady = true;
            IsConstructionNotReady = true;
            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == builtBy);
            if (identity != null)
                DisplayName = identity.DisplayName;
            else
                DisplayName = "Nobody";
            CountBlocks(cubes);
        }

        private void CountBlocks(IEnumerable<MyObjectBuilder_CubeBlock> cubes)
        {
            var bs = new BlockStatistics(cubes);
            BlockCount = bs.BlockCount;
            _blockCountDetails = bs.BlockCountDetails;
            _assemblerCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Assembler)?.Count ?? 0;
            _assemblerDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Assembler)?.GetDetails();
            _refineryCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Refinery)?.Count ?? 0;
            _refineryDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Refinery)?.GetDetails();
            _shipToolCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.ShipTool)?.Count ?? 0;
            _shipToolDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.ShipTool)?.GetDetails();
            _powerBlockCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Power)?.Count ?? 0;
            _powerBlockDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.Power)?.GetDetails();
            _thrusterCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.ThrusterGyro)?.Count ?? 0;
            _thrusterDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.ThrusterGyro)?.GetDetails();
            _turretCount = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.WeaponTurret)?.Count ?? 0;
            _turretDetails = bs.cubeCategories.FirstOrDefault(c => c.Category == BlockStatistics.BlockCategory.WeaponTurret)?.GetDetails();
        }

        #endregion

        #region Properties

        public override string DisplayName
        {
            get
            {
                return base.DisplayName;
            }
            set
            {
                base.DisplayName = value;
            }
        }

        public string ActiveComponentFilter
        {
            get
            {
                return _activeComponentFilter;
            }

            set
            {
                if (value != _activeComponentFilter)
                {
                    _activeComponentFilter = value;
                    RaisePropertyChanged(() => ActiveComponentFilter);
                }
            }
        }

        public string ComponentFilter
        {
            get
            {
                return _componentFilter;
            }

            set
            {
                if (value != _componentFilter)
                {
                    _componentFilter = value;
                    RaisePropertyChanged(() => ComponentFilter);
                }
            }
        }

        public bool IsSubsSystemNotReady
        {
            get { return _isSubsSystemNotReady; }

            set
            {
                if (value != _isSubsSystemNotReady)
                {
                    _isSubsSystemNotReady = value;
                    RaisePropertyChanged(() => IsSubsSystemNotReady);
                }
            }
        }

        public bool IsConstructionNotReady
        {
            get { return _isConstructionNotReady; }

            set
            {
                if (value != _isConstructionNotReady)
                {
                    _isConstructionNotReady = value;
                    RaisePropertyChanged(() => IsConstructionNotReady);
                }
            }
        }

        public string BlockCountDetails
        {
            get
            {
                return _blockCountDetails;
            }
        }

        public int AssemblerCount
        {
            get
            {
                return _assemblerCount;
            }
        }

        public string AssemblerDetails
        {
            get
            {
                return _assemblerDetails;
            }
        }

        public int RefineryCount
        {
            get
            {
                return _refineryCount;
            }
        }

        public string RefineryDetails
        {
            get
            {
                return _refineryDetails;
            }
        }

        public int ShipToolCount
        {
            get
            {
                return _shipToolCount;
            }
        }

        public string ShipToolDetails
        {
            get
            {
                return _shipToolDetails;
            }
        }

        public int PowerBlockCount
        {
            get
            {
                return _powerBlockCount;
            }
        }

        public string PowerBlockDetails
        {
            get
            {
                return _powerBlockDetails;
            }
        }

        public int TurretCount
        {
            get
            {
                return _turretCount;
            }
        }

        public string TurretDetails
        {
            get
            {
                return _turretDetails;
            }
        }

        public int ThrusterCount
        {
            get
            {
                return _thrusterCount;
            }
        }

        public string ThrusterDetails
        {
            get
            {
                return _thrusterDetails;
            }
        }
        #endregion

        #region methods

        #endregion
    }
}
