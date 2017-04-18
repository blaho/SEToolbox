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

    public class StructureTimerModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.


        [NonSerialized]
        private static readonly object Locker = new object();

        [NonSerialized]
        private string _gridName;

        [NonSerialized]
        private string _ownerName;

        [NonSerialized]
        private IEnumerable<string> _toolbarButtons;

        [NonSerialized]
        private string _toolbarSummary;

        [NonSerialized]
        private decimal _delay;

        [NonSerialized]
        private bool _enabled;

        [NonSerialized]
        private string _selfTriggerType;

        [NonSerialized]
        IEnumerable<Tuple<long, string>> _programmableBlocks;

        [NonSerialized]
        private string _pbNames;

        [NonSerialized]
        private string _pbSourceCodePreview;

        [NonSerialized]
        private IEnumerable<Tuple<long, string>> _pbSourceCodes;

        #endregion

        #region ctor

        public StructureTimerModel(IEnumerable<Tuple<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeBlock>> blocks, MyObjectBuilder_CubeGrid grid, MyObjectBuilder_TimerBlock timer)
            : base(null)
        {
            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == timer.Owner);
            if (identity != null)
                _ownerName = identity.DisplayName;
            else
                _ownerName = "Unknown";
            _gridName = grid.DisplayName;
            DisplayName = GetBlockName(grid, timer);
            _delay = timer.Delay / 1000;
            _enabled = timer.Enabled && timer.IsCountingDown;
            if (timer.Toolbar.Slots.Count > 0)
            {
                var toolbarItems = timer.Toolbar.Slots.OrderBy(s => s.Index).Select(s => s.Data).OfType<MyObjectBuilder_ToolbarItemTerminalBlock>();
                _toolbarButtons = toolbarItems.Select(ti => $"{GetBlockName(blocks.SingleOrDefault(cb => cb.Item2.EntityId == ti.BlockEntityId))} - {ti._Action}");
                _toolbarSummary = String.Join(" | ", _toolbarButtons);
                var selfRefSlots = timer.Toolbar.Slots.Select(s => s.Data).OfType<MyObjectBuilder_ToolbarItemTerminalBlock>().Where(s => s.BlockEntityId == timer.EntityId);
                if (selfRefSlots.Count() == 0)
                    _selfTriggerType = "None";
                else
                {
                    if (selfRefSlots.Count(s => s._Action == "TriggerNow") > 0)
                        _selfTriggerType = "Trigger Now";
                    else
                    {
                        if (selfRefSlots.Count(s => s._Action == "Start") > 0)
                            _selfTriggerType = "Start";
                        else
                            _selfTriggerType = selfRefSlots.First()._Action;
                    }
                }
                var pbSlots = toolbarItems.Select(s => blocks.SingleOrDefault(cb => cb.Item2.EntityId == s.BlockEntityId)).Where(cb => cb?.Item2 is MyObjectBuilder_MyProgrammableBlock);
                _programmableBlocks = pbSlots.Select(pb => new Tuple<long, string>(pb.Item2.EntityId, GetBlockName(pb)));
                _pbNames = String.Join("\n", _programmableBlocks);
                _pbSourceCodePreview = String.Join("\n", pbSlots.Select(pb =>
                    {
                        var prg = ((MyObjectBuilder_MyProgrammableBlock)pb.Item2).Program;
                        return prg?.Substring(0, Math.Min(prg.Length, 1000)).Replace('\n', ' ');
                    }));
                _pbSourceCodes = pbSlots.Select(pb => new Tuple<long, string>(pb.Item2.EntityId, ((MyObjectBuilder_MyProgrammableBlock)pb.Item2).Program));
            }
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

        public string ToolbarSummary
        {
            get { return _toolbarSummary; }
        }

        public IEnumerable<string> ToolbarButtons
        {
            get { return _toolbarButtons; }
        }

        public decimal Delay
        {
            get { return _delay; }
        }

        public bool Enabled
        {
            get { return _enabled; }
        }

        public string SelfTriggerType
        {
            get { return _selfTriggerType; }
        }

        public IEnumerable<Tuple<long, string>> ProgrammableBlocks
        {
            get { return _programmableBlocks; }
        }

        public string ProgrammableBlockNames
        {
            get { return _pbNames; }
        }

        public string ProgrammableBlockSourceCodePreview
        {
            get { return _pbSourceCodePreview; }
        }

        public IEnumerable<Tuple<long, string>> ProgrammableBlockSourceCodes
        {
            get { return _pbSourceCodes; }
        }

        #endregion

        #region methods

        #endregion
    }
}
