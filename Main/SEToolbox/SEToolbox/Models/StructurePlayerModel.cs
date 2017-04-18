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
        private static readonly object Locker = new object();

        [NonSerialized]
        private BlockStatistics _blockStatistics;

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
            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == builtBy);
            if (identity != null)
                DisplayName = identity.DisplayName;
            else
                DisplayName = "Unknown";
            _blockStatistics = new BlockStatistics(cubes);
            CountBlocks(cubes);
        }

        private void CountBlocks(IEnumerable<MyObjectBuilder_CubeBlock> cubes)
        {
            _assemblerCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Assembler)?.Count ?? 0;
            _assemblerDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Assembler)?.GetDetails();
            _refineryCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Refinery)?.Count ?? 0;
            _refineryDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Refinery)?.GetDetails();
            _shipToolCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.ShipTool)?.Count ?? 0;
            _shipToolDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.ShipTool)?.GetDetails();
            _powerBlockCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Power)?.Count ?? 0;
            _powerBlockDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.Power)?.GetDetails();
            _thrusterCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.ThrusterGyro)?.Count ?? 0;
            _thrusterDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.ThrusterGyro)?.GetDetails();
            _turretCount = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.WeaponTurret)?.Count ?? 0;
            _turretDetails = _blockStatistics.CubeCategories.FirstOrDefault(c => c.Category == Services.BlockCategory.WeaponTurret)?.GetDetails();
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

        public override int BlockCount
        {
            get { return _blockStatistics.Count; }
        }

        public IEnumerable<BlockStatistics> BlockStatistics
        {
            get { return new List<BlockStatistics>(new BlockStatistics[] { _blockStatistics }); }
        }

        public string BlockCountDetails
        {
            get { return _blockStatistics.BlockCountDetails; }
        }

        public int AssemblerCount
        {
            get { return _assemblerCount; }
        }

        public string AssemblerDetails
        {
            get { return _assemblerDetails; }
        }

        public int RefineryCount
        {
            get { return _refineryCount; }
        }

        public string RefineryDetails
        {
            get { return _refineryDetails; }
        }

        public int ShipToolCount
        {
            get { return _shipToolCount; }
        }

        public string ShipToolDetails
        {
            get { return _shipToolDetails; }
        }

        public int PowerBlockCount
        {
            get { return _powerBlockCount; }
        }

        public string PowerBlockDetails
        {
            get { return _powerBlockDetails; }
        }

        public int TurretCount
        {
            get { return _turretCount; }
        }

        public string TurretDetails
        {
            get { return _turretDetails; }
        }

        public int ThrusterCount
        {
            get { return _thrusterCount; }
        }

        public string ThrusterDetails
        {
            get { return _thrusterDetails; }
        }
        #endregion

        #region methods

        #endregion
    }
}
