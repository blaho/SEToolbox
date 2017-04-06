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
        private string _gridName;

        [NonSerialized]
        private string _ownerName;

        [NonSerialized]
        private string _toolbarSummary;

        [NonSerialized]
        private decimal _delay;

        [NonSerialized]
        private bool _enabled;

        [NonSerialized]
        private string _selfTriggerType;

        [NonSerialized]
        private string _pbName;

        [NonSerialized]
        private string _pbSourceCodePreview;

        [NonSerialized]
        private string _pbSourceCode;

        #endregion

        #region ctor

        public StructureTimerModel(IEnumerable<Tuple<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeBlock>> blocks, MyObjectBuilder_CubeGrid grid, MyObjectBuilder_TimerBlock timer)
            : base(null)
        {
            IsSubsSystemNotReady = true;
            IsConstructionNotReady = true;
            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == timer.Owner);
            if (identity != null)
                _ownerName = identity.DisplayName;
            else
                _ownerName = "Nobody";
            _gridName = grid.DisplayName;
            DisplayName = GetBlockName(grid, timer);
            _delay = timer.Delay / 1000;
            _enabled = timer.Enabled && timer.IsCountingDown;
            if (timer.Toolbar.Slots.Count > 0)
            {
                var toolbarItems = timer.Toolbar.Slots.OrderBy(s => s.Index).Select(s => s.Data).OfType<MyObjectBuilder_ToolbarItemTerminalBlock>();
                _toolbarSummary = String.Join(" | ", toolbarItems.Select(ti => $"{GetBlockName(blocks.SingleOrDefault(cb => cb.Item2.EntityId == ti.BlockEntityId))} - {ti._Action}"));
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
                _pbName = String.Join("\n", pbSlots.Select(pb => GetBlockName(pb)));
                _pbSourceCodePreview = String.Join("\n", pbSlots.Select(pb =>
                    {
                        var prg = ((MyObjectBuilder_MyProgrammableBlock)pb.Item2).Program;
                        return prg.Substring(0, Math.Min(prg.Length, 1000)).Replace('\n', ' ');
                    }));
                _pbSourceCode = String.Join("\n\n-----------------\n\n", pbSlots.Select(pb => ((MyObjectBuilder_MyProgrammableBlock)pb.Item2).Program));

                ;
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

        public string ProgrammableBlockName
        {
            get { return _pbName; }
        }

        public string ProgrammableBlockSourceCodePreview
        {
            get { return _pbSourceCodePreview; }
        }

        public string ProgrammableBlockSourceCode
        {
            get { return _pbSourceCode; }
        }

        #endregion

        #region methods

        #endregion
    }
}
