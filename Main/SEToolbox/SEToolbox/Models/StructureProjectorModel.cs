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

    public class StructureProjectorModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.


        [NonSerialized]
        private static readonly object Locker = new object();

        [NonSerialized]
        private string _gridName;

        [NonSerialized]
        private string _builderName;

        [NonSerialized]
        private string _gridBuilderName;

        [NonSerialized]
        private string _ownerName;

        [NonSerialized]
        private bool _enabled;

        [NonSerialized]
        private BlockStatistics _blockStatistics;

        [NonSerialized]
        private string _blockCountStr;

        #endregion

        #region ctor

        public StructureProjectorModel(MyObjectBuilder_CubeGrid grid, MyObjectBuilder_Projector proj)
            : base(null)
        {
            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == proj.Owner);
            if (identity != null)
                _ownerName = identity.DisplayName;
            else
                _ownerName = "Nobody";
            _gridName = grid.DisplayName;
            identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == proj.BuiltBy);
            if (identity != null)
                _builderName = identity.DisplayName;
            else
                _builderName = "Nobody";
            _builderName = $"(by {_builderName})";
            identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == grid.GetTopBuilderId());
            if (identity != null)
                _gridBuilderName = identity.DisplayName;
            else
                _gridBuilderName = "Nobody";
            _gridBuilderName = $"(by {_gridBuilderName})";
            DisplayName = GetBlockName(grid, proj);
            _enabled = proj.Enabled && proj.ProjectedGrid != null;
            if (proj.ProjectedGrid == null)
                return;
            _blockStatistics = new BlockStatistics(proj.ProjectedGrid.CubeBlocks);
            BlockCount = proj.ProjectedGrid.CubeBlocks.Count;
            _blockCountStr = $"{proj.ProjectedGrid.CubeBlocks.Count} ({((decimal)proj.ProjectedGrid.CubeBlocks.Count / grid.CubeBlocks.Count * 100):F1}% of self)";
        }

        private string GetBlockName(Tuple<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeBlock> blockWithParent)
        {
            if (blockWithParent == null)
                return "?";
            return GetBlockName(blockWithParent.Item1, blockWithParent.Item2);
        }

        private string GetBlockName(MyObjectBuilder_CubeGrid grid, MyObjectBuilder_CubeBlock block)
        {
            var res = (block as MyObjectBuilder_TerminalBlock)?.CustomName;
            if (res != null)
                return res;
            var cubeDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, grid.GridSizeEnum, block.SubtypeName);
            return cubeDefinition.DisplayNameText;
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

        public string GridName
        {
            get { return _gridName; }
        }

        public string OwnerName
        {
            get { return _ownerName; }
        }

        public string BuilderName
        {
            get { return _builderName; }
        }

        public string GridBuilderName
        {
            get { return _gridBuilderName; }
        }

        public bool Enabled
        {
            get { return _enabled; }
        }

        public string BlockCountStr
        {
            get { return _blockCountStr; }
        }

        #endregion

        #region methods

        #endregion
    }
}
