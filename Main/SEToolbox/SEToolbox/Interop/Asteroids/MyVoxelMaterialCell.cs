﻿//===================================================================================
// Voxel Methods/Classes for Space Engineers.
// Based on the Miner-Wars-2081 Mod Kit.
// Copyright (c) Keen Software House a. s.
// This code is expressly licenced, and should not be used in any application without 
// the permission of Keen Software House.
// See http://www.keenswh.com/about.html
// All rights reserved.
//===================================================================================

namespace SEToolbox.Interop.Asteroids
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VRageMath;

    class MyVoxelMaterialCell
    {
        #region fields

        private const int VoxelsInCell = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;
        private const int XStep = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;
        private const int YStep = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;
        private const int ZStep = 1;

        //  If whole cell contains only one material, it will be written in this member and 3D arrays won't be used.
        private bool _singleMaterialForWholeCell;
        private string _singleMaterial;
        private byte _singleIndestructibleContent;

        //  Used only if individual materials aren't same - are mixed.
        private string[] _materials;  // TODO: change back into a byte/int array, to preserve memory. Link to Material Dictionary.
        private byte[] _indestructibleContent;
        private string _averageCellMaterial;
        private static readonly Dictionary<string, int> CellMaterialCounts = new Dictionary<string, int>();

        #endregion

        #region ctor

        public MyVoxelMaterialCell(string defaultMaterial, byte defaultIndestructibleContents)
        {
            //  By default cell contains only one single material
            this.Reset(defaultMaterial, defaultIndestructibleContents);
        }

        #endregion

        #region methods

        //  Use when you want to change whole cell to one single material
        public void Reset(string defaultMaterial, byte defaultIndestructibleContents)
        {
            this._singleMaterialForWholeCell = true;
            this._singleMaterial = defaultMaterial;
            this._singleIndestructibleContent = defaultIndestructibleContents;
            this._averageCellMaterial = this._singleMaterial;
            this._materials = null;
            this._indestructibleContent = null;
        }


        //  Change material for specified voxel
        //  If this material is single material for whole cell, we do nothing. Otherwise we allocate 3D arrays and start using them.
        public void SetMaterialAndIndestructibleContent(string material, byte indestructibleContent, ref Vector3I voxelCoordInCell)
        {
            this.CheckInitArrays(material);

            if (this._singleMaterialForWholeCell == false)
            {
                var xyz = voxelCoordInCell.X * XStep + voxelCoordInCell.Y * YStep + voxelCoordInCell.Z * ZStep;
                this._materials[xyz] = material;
                this._indestructibleContent[xyz] = indestructibleContent;
            }
        }

        public void RepalceMaterial(string material)
        {
            this._singleMaterial = material;
            this._averageCellMaterial = material;
        }

        public bool IsSingleMaterialForWholeCell()
        {
            return this._singleMaterialForWholeCell;
        }

        //  Check if we new material differs from one main material and if yes, we need to start using 3D arrays
        void CheckInitArrays(string material)
        {
            if (this._singleMaterialForWholeCell && (this._singleMaterial != material))
            {
                this._materials = new string[VoxelsInCell];
                this._indestructibleContent = new byte[VoxelsInCell];
                //  Fill with present cell values
                for (var xyz = 0; xyz < VoxelsInCell; xyz++)
                {
                    this._materials[xyz] = _singleMaterial;
                    this._indestructibleContent[xyz] = this._singleIndestructibleContent;
                }

                //  From now, this cell contains more than one material
                this._singleMaterialForWholeCell = false;
            }
        }

        //  Calculate and then remember average material in this cell. It isn't single material, but average.
        public void CalcAverageCellMaterial()
        {
            if (this._singleMaterialForWholeCell == true)
            {
                //  For single material it's easy
                this._averageCellMaterial = this._singleMaterial;
            }
            else
            {
                //  If materials are stored in 3D array, we need to really calculate average material
                //  Iterate materials in this data cell
                for (var xyz = 0; xyz < VoxelsInCell; xyz++)
                {
                    var material = this._materials[xyz];

                    if (!CellMaterialCounts.ContainsKey(material))
                        CellMaterialCounts.Add(material, 1);
                    else
                        CellMaterialCounts[material]++;
                }

                var maxNum = 0;

                var keys = CellMaterialCounts.Keys.ToArray();
                foreach (var key in keys)
                {
                    var val = CellMaterialCounts[key];

                    if (val > maxNum)
                    {
                        maxNum = val;
                        this._averageCellMaterial = key;
                    }
                    CellMaterialCounts[key] = 0; // Erase for next operation
                }
            }

            Debug.Assert(this._averageCellMaterial != null);
        }

        //  Return material for specified voxel. If whole cell contain one single material, this one is returned. Otherwise material from 3D array is returned.
        public string GetMaterial(ref Vector3I voxelCoordInCell)
        {
            if (_singleMaterialForWholeCell == true)
            {
                return _singleMaterial;
            }
            else
            {
                return _materials[voxelCoordInCell.X * XStep + voxelCoordInCell.Y * YStep + voxelCoordInCell.Z * ZStep];
            }
        }

        //  Same as GetMaterial() - but this is for indestructible content
        public byte GetIndestructibleContent(ref Vector3I voxelCoordInCell)
        {
            return 0xFF;  // Anything but full 255 isn't working quite right in SE.
            // Perhaps SE Alpha team haven't quite worked out if this is still to be supported in SE.

            //if (m_singleMaterialForWholeCell == true)
            //{
            //    return m_singleIndestructibleContent;
            //}
            //else
            //{
            //    return m_indestructibleContent[voxelCoordInCell.X * xStep + voxelCoordInCell.Y * yStep + voxelCoordInCell.Z * zStep];
            //}
        }

        #endregion
    }
}