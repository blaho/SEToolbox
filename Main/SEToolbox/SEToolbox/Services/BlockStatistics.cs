using SEToolbox.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace SEToolbox.Services
{
    public class BlockStatistics
    {
        public int BlockCount { get; set; }
        public string BlockCountDetails
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var sc in cubeCategories)
                    sb.AppendLine($"{sc.Category}: {sc.Count}");
                return sb.ToString();
            }
        }

        public List<BlockStatsBySubcategory> cubeSubcategories { get; private set; }
        public List<BlockStatsByCategory> cubeCategories { get; private set; }

        public BlockStatistics(IEnumerable<MyObjectBuilder_CubeBlock> cubes)
        {
            BlockCount = cubes.Count();
            foreach (var c in cubes.Where(c => c.SubtypeName == ""))
                c.SubtypeName = c.GetType().Name.Remove(0, c.GetType().Name.IndexOf('_') + 1);
            cubeSubcategories = cubes.GroupBy(k => GetBlockSubcategory(k.SubtypeName), (subtypeName, blocks) => new BlockStatsBySubcategory { Subcategory = subtypeName, Blocks = blocks, Count = blocks.Count() }).OrderBy(cc => cc.Count).ToList();
            cubeCategories = cubeSubcategories.GroupBy(k => GetBlockCategory(k.Subcategory), (cat, subcats) => new BlockStatsByCategory() { Category = cat, Subcategories = subcats, Count = subcats.Select(sc => sc.Count).Aggregate((prev, curr) => prev + curr) }).ToList();
        }

        public enum BlockCategory
        {
            None,

            // interesting
            Assembler,
            Refinery,
            ShipTool,
            Power,
            ThrusterGyro,
            WeaponTurret,

            // not interesting
            Armor,
            Conveyor,
            Gases,
            Container,
            JumpDrive,
            Weapon,

            Misc
        }

        public enum BlockSubcategory
        {
            None,

            Assembler,

            Refinery,
            ArcFurnace,

            ShipDrill,
            ShipWelder,
            ShipGrinder,

            SmallReactor,
            LargeReactor,
            Battery,
            Solar,

            SmallThruster,
            LargeThruster,
            Gyro,
            JumpDrive,

            InteriorTurret,
            GatlingTurret,
            MissileTurret,

            GatlingGun,
            MissileLauncher,

            LightArmor,
            HeavyArmor,
            InteriorWall,
            SteelCatwalk,
            StairRamp,
            Window,

            OxygenTank,
            HydrogenTank,
            OxygenGenerator,
            AirVent,

            SmallContainer,
            MediumContainer,
            LargeContainer,

            Conveyor,
            Suspension,
            Lights,
            Cockpit,
            Automation,
            Door,
            Modules,
            Communication,
            Environment,
            RotorPiston,
            Projector,
            Medical,
            MergeBlock,

            Misc
        }

        private BlockSubcategory GetBlockSubcategory(string subtypeName)
        {
            if (subtypeName.Contains("HeavyBlockArmor"))
                return BlockSubcategory.HeavyArmor;
            if (subtypeName.Contains("BlockArmor") || subtypeName.StartsWith("SmallArmor") || subtypeName.StartsWith("Armor"))
                return BlockSubcategory.LightArmor;
            if ((subtypeName == SubtypeId.LargeBlockInteriorWall.ToString()) || (subtypeName == SubtypeId.LargeInteriorPillar.ToString()) || (subtypeName == "Passage"))
                return BlockSubcategory.InteriorWall;
            if (subtypeName.Contains("Conveyor") || subtypeName.Contains("Connector") || subtypeName.Contains("Collector"))
                return BlockSubcategory.Conveyor;
            if (subtypeName.Contains("Thrust"))
                if (subtypeName.Contains("BlockSmall"))
                    return BlockSubcategory.SmallThruster;
                else
                    return BlockSubcategory.LargeThruster;
            if (subtypeName == SubtypeId.LargeJumpDrive.ToString())
                return BlockSubcategory.JumpDrive; 
            if (subtypeName.Contains("OxygenTank"))
                return BlockSubcategory.OxygenTank;
            if (subtypeName.Contains("OxygenGenerator") || (subtypeName == SubtypeId.LargeBlockOxygenFarm.ToString()))
                return BlockSubcategory.OxygenGenerator;
            if ((subtypeName == SubtypeId.SmallHydrogenTank.ToString()) || (subtypeName == SubtypeId.LargeHydrogenTank.ToString()))
                return BlockSubcategory.HydrogenTank;
            if (subtypeName == SubtypeId.LargeAssembler.ToString())
                return BlockSubcategory.Assembler;
            if (subtypeName == "Blast Furnace")
                return BlockSubcategory.ArcFurnace;
            if (subtypeName == SubtypeId.LargeRefinery.ToString())
                return BlockSubcategory.Refinery;
            if ((subtypeName == SubtypeId.SmallBlockDrill.ToString()) || (subtypeName == SubtypeId.LargeBlockDrill.ToString()))
                return BlockSubcategory.ShipDrill;
            if ((subtypeName == SubtypeId.SmallShipWelder.ToString()) || (subtypeName == SubtypeId.LargeShipWelder.ToString()))
                return BlockSubcategory.ShipWelder;
            if ((subtypeName == SubtypeId.SmallShipGrinder.ToString()) || (subtypeName == SubtypeId.LargeShipGrinder.ToString()))
                return BlockSubcategory.ShipGrinder;
            if ((subtypeName == SubtypeId.SmallBlockSmallGenerator.ToString()) || (subtypeName == SubtypeId.LargeBlockSmallGenerator.ToString()))
                return BlockSubcategory.SmallReactor;
            if ((subtypeName == SubtypeId.SmallBlockLargeGenerator.ToString()) || (subtypeName == SubtypeId.LargeBlockLargeGenerator.ToString()))
                return BlockSubcategory.LargeReactor;
            if ((subtypeName == SubtypeId.SmallBlockBatteryBlock.ToString()) || (subtypeName == SubtypeId.LargeBlockBatteryBlock.ToString()))
                return BlockSubcategory.Battery;
            if (subtypeName.Contains("Solar"))
                return BlockSubcategory.Solar;
            if (subtypeName.Contains("GatlingTurret"))
                return BlockSubcategory.GatlingTurret;
            if (subtypeName.Contains("MissileTurret"))
                return BlockSubcategory.MissileTurret;
            if (subtypeName == SubtypeId.LargeInteriorTurret.ToString())
                return BlockSubcategory.InteriorTurret;
            if ((subtypeName == SubtypeId.SmallBlockSmallContainer.ToString()) || (subtypeName == SubtypeId.LargeBlockSmallContainer.ToString()))
                return BlockSubcategory.SmallContainer; 
            if (subtypeName == SubtypeId.SmallBlockMediumContainer.ToString())
                return BlockSubcategory.MediumContainer;
            if ((subtypeName == SubtypeId.SmallBlockLargeContainer.ToString()) || (subtypeName == SubtypeId.LargeBlockLargeContainer.ToString()))
                return BlockSubcategory.LargeContainer;
            if (subtypeName.Contains("LandingGear") || subtypeName.Contains("Suspension"))
                return BlockSubcategory.Suspension;
            if (subtypeName.Contains("Antenna") || subtypeName.Contains("Beacon") || subtypeName.Contains("OreDetector"))
                return BlockSubcategory.Communication;
            if (subtypeName.Contains("Light"))
                return BlockSubcategory.Lights;
            if (subtypeName.Contains("Cockpit") || subtypeName.Contains("Seat"))
                return BlockSubcategory.Cockpit;
            if (subtypeName.Contains("Camera") || subtypeName.Contains("Programmable") || subtypeName.Contains("Sensor") || subtypeName.Contains("Timer") || subtypeName.Contains("Remote")
                || subtypeName.Contains("LCD") || subtypeName.Contains("TextPanel") || subtypeName.Contains("Sound") || subtypeName.Contains("ButtonPanel") || subtypeName.Contains("ControlPanel"))
                return BlockSubcategory.Automation;
            if (subtypeName.Contains("Gyro"))
                return BlockSubcategory.Gyro;
            if (subtypeName.EndsWith("Gun"))
                return BlockSubcategory.GatlingGun;
            if (subtypeName.Contains("Launcher"))
                return BlockSubcategory.MissileLauncher;
            if (subtypeName.Contains("Gravity") || subtypeName.Contains("Vent"))
                return BlockSubcategory.Environment;
            if (subtypeName.Contains("Projector"))
                return BlockSubcategory.Projector;
            if (subtypeName.EndsWith("Stator") || subtypeName.Contains("PistonBase"))
                return BlockSubcategory.RotorPiston;
            if (subtypeName.Contains("PistonTop") || subtypeName.EndsWith("Rotor") || subtypeName.Contains("Wheel"))
                return BlockSubcategory.None;
            if (subtypeName.Contains("Door"))
                return BlockSubcategory.Door;
            if (subtypeName.Contains("Catwalk") || subtypeName.Contains("CoverWall"))
                return BlockSubcategory.SteelCatwalk;
            if (subtypeName.Contains("Module"))
                return BlockSubcategory.Modules;
            if (subtypeName.Contains("Window"))
                return BlockSubcategory.Window;
            if ((subtypeName == SubtypeId.LargeMedicalRoom.ToString()) || subtypeName == SubtypeId.LargeBlockCryoChamber.ToString())
                return BlockSubcategory.Medical;
            if ((subtypeName == SubtypeId.LargeStairs.ToString()) || subtypeName == SubtypeId.LargeRamp.ToString())
                return BlockSubcategory.StairRamp;
            if (subtypeName.Contains("MergeBlock"))
                return BlockSubcategory.MergeBlock;
            return BlockSubcategory.Misc;
        }

        private BlockCategory GetBlockCategory(BlockSubcategory subcategory)
        {
            switch (subcategory)
            {
                case BlockSubcategory.None:
                    return BlockCategory.None;

                case BlockSubcategory.Assembler:
                    return BlockCategory.Assembler;

                case BlockSubcategory.Refinery:
                case BlockSubcategory.ArcFurnace:
                    return BlockCategory.Refinery;

                case BlockSubcategory.ShipDrill:
                case BlockSubcategory.ShipWelder:
                case BlockSubcategory.ShipGrinder:
                    return BlockCategory.ShipTool;

                case BlockSubcategory.SmallReactor:
                case BlockSubcategory.LargeReactor:
                case BlockSubcategory.Battery:
                case BlockSubcategory.Solar:
                    return BlockCategory.Power;

                case BlockSubcategory.SmallThruster:
                case BlockSubcategory.LargeThruster:
                case BlockSubcategory.Gyro:
                    return BlockCategory.ThrusterGyro;

                case BlockSubcategory.JumpDrive:
                    return BlockCategory.JumpDrive;

                case BlockSubcategory.SmallContainer:
                case BlockSubcategory.MediumContainer:
                case BlockSubcategory.LargeContainer:
                    return BlockCategory.Container;

                case BlockSubcategory.InteriorTurret:
                case BlockSubcategory.GatlingTurret:
                case BlockSubcategory.MissileTurret:
                    return BlockCategory.WeaponTurret;

                case BlockSubcategory.GatlingGun:
                case BlockSubcategory.MissileLauncher:
                    return BlockCategory.Weapon;

                case BlockSubcategory.LightArmor:
                case BlockSubcategory.HeavyArmor:
                case BlockSubcategory.InteriorWall:
                case BlockSubcategory.SteelCatwalk:
                case BlockSubcategory.StairRamp:
                    return BlockCategory.Armor;

                default: return BlockCategory.Misc;
            }
        }

        public class BlockStatsBySubcategory
        {
            public BlockSubcategory Subcategory;
            public IEnumerable<MyObjectBuilder_CubeBlock> Blocks;
            public int Count;
        }

        public class BlockStatsByCategory
        {
            public BlockCategory Category;
            public IEnumerable<BlockStatsBySubcategory> Subcategories;
            public int Count;

            public string GetDetails()
            {
                var sb = new StringBuilder();
                foreach (var sc in Subcategories)
                    sb.AppendLine($"{sc.Subcategory}: {sc.Count}");
                return sb.ToString();
            }
        }
    }

}


