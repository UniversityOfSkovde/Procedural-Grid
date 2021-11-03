﻿using UnityEngine;

namespace Grid {
    
    /// <summary>
    /// Each tile has an id (the local position as a <c>Vector2Int</c>)
    /// and a bitset. The bitset stores either true or false for each possible
    /// property detailed in <c>GridTileProperty</c>. In addition to
    /// this, each tile stores a cached version of the bitsets of its nearest
    /// neighbours. If the tile belongs to a <c>Grid</c>, then it is
    /// responsible for updating these cached bitsets.
    /// 
    /// Neighbours are stored like this:
    /// <list type="table">
    ///     <item>
    ///         <term><c>0</c></term>
    ///         <description>Right</description>
    ///     </item>
    ///     <item>
    ///         <term><c>1</c></term>
    ///         <description>Up-Right</description>
    ///     </item>
    ///     <item>
    ///         <term><c>2</c></term>
    ///         <description>Up</description>
    ///     </item>
    ///     <item>
    ///         <term><c>3</c></term>
    ///         <description>Up-Left</description>
    ///     </item>
    ///     <item>
    ///         <term><c>4</c></term>
    ///         <description>Left</description>
    ///     </item>
    ///     <item>
    ///         <term><c>5</c></term>
    ///         <description>Down-Left</description>
    ///     </item>
    ///     <item>
    ///         <term><c>6</c></term>
    ///         <description>Down</description>
    ///     </item>
    ///     <item>
    ///         <term><c>7</c></term>
    ///         <description>Down-Right</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class GridTile : MonoBehaviour {
        
        internal Vector2Int _id;
        internal bool _attached;
        
        public Vector2Int Id => _id;
        public bool IsAttached => _attached;
        
        [SerializeField, HideInInspector] private uint _bitset = 0u;
        [SerializeField, HideInInspector] private uint[] _neighbours = new uint[8];

        internal void Set(uint bitset, uint[] neighbours) {
            _bitset     = bitset;
            _neighbours = neighbours;
        }
        
        public bool GetProperty(GridTileProperty property) => GetProperty((int)property);
        public bool GetProperty(int property) => Bitset.Get(_bitset, property);

        public void SetProperty(GridTileProperty property, bool value) => SetProperty((int)property, value);
        public void SetProperty(int property, bool value) {
            // If no changed were made, do nothing.
            if (GetProperty(property) == value) return;
            
            // Modify this local instance
            Bitset.Set(ref _bitset, property, value);
            
            // If the tile is attached to a Grid, send the modification up to the parent
            if (IsAttached) {
                // Enqueue property change
                var parent = transform.parent.GetComponent<Grid>();
                parent.SetTileProperties(Id, _bitset);
            }
        }

        public bool GetNeighbourProperty(int neighbour, GridTileProperty property) {
            return GetNeighbourProperty(neighbour, (int)property);
        }
        
        public bool GetNeighbourProperty(int neighbour, int property) {
            if (neighbour < 0) return GetProperty(property);
            EnsureNeighbourSize();
            return Bitset.Get(_neighbours[neighbour], property);
        }
        
        public void SetNeighbourProperty(int neighbour, GridTileProperty property, bool value) {
            SetNeighbourProperty(neighbour, (int)property, value);
        }
        
        public void SetNeighbourProperty(int neighbour, int property, bool value) {
            if (neighbour < 0) {
                SetProperty(property, value);
                return;
            }
            EnsureNeighbourSize();
            Bitset.Set(ref _neighbours[neighbour], property, value);
        }

        private void EnsureNeighbourSize() {
            if (_neighbours == null) {
                _neighbours = new uint[8];
                return;
            }

            if (_neighbours.Length < 8) {
                var newArray = new uint[8];
                for (var i = 0; i < _neighbours.Length; i++) {
                    newArray[i] = _neighbours[i];
                }
                _neighbours = newArray;
            }
        }
    }
}