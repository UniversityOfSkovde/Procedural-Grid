using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Grid {
    [ExecuteAlways]
    public sealed class Grid : MonoBehaviour, ISerializationCallbackReceiver {

        public delegate void GridEventHandler(Grid grid, GridTile[] tiles);
        public event GridEventHandler TilesCreated;
        
        [Min(0)]
        [Tooltip("The size (in tiles) of the map in X and Z")]
        public Vector2Int Size = new Vector2Int(5, 5);

        [SerializeField]
        [Tooltip("Prefab that is spawned for each tile in the Dungeon.")]
        private GameObject TilePrefab;

        [Space]
        
        [SerializeField, HideInInspector]
        [Tooltip("Names for the types as they should appear in the editor (it is the order that is important)")]
        internal string[] PropertyNames =
            typeof(GridTileProperty).GetEnumNames();
        
        [SerializeField, HideInInspector]
        private CellData[] _cellData;

        [SerializeField, HideInInspector] 
        private uint[] _tileData;
        
        [Serializable]
        private struct CellData {
            public uint Properties;
            public string JsonData;

            public CellData(uint properties, string jsonData) {
                Properties = properties;
                JsonData = jsonData;
            }
        }
        
        void Start() {
#if UNITY_EDITOR
            UpdateOnceIfNecessary();
#else
            RecreateTiles();
#endif
        }

        public bool TryGetPropertyNames(out List<string> names) {
            if (PropertyNames != null && PropertyNames.Length > 0) {
                names = new List<string>(PropertyNames);
                return true;
            }

            names = null;
            return false;
        }

        public bool IsInside(Vector2Int id) => id.x >= 0 && id.x < Size.x &&
                                               id.y >= 0 && id.y < Size.y;

        public void SetTileProperties(Vector2Int id, uint properties) {
            if (!IsInside(id)) return;
            EnsureGridCapacity();
            var idx = IndexOf(id);
            _cellData[idx] = new CellData(properties, _cellData[idx].JsonData);
            UpdateGridAround(id);
        }

        public uint GetTileProperties(Vector2Int id) {
            if (!IsInside(id)) return 0;
            EnsureGridCapacity();
            return _cellData[IndexOf(id)].Properties;
        }

#if CSHARP_7_3_OR_NEWER
        public bool GetTileProperty<T>(Vector2Int id, T property) where T : Enum => GetTileProperty(id, Convert.ToInt32(property));
        public void SetTileProperty<T>(Vector2Int id, T property, bool value) where T : Enum  => SetTileProperty(id, Convert.ToInt32(property), value);
#else
        public bool GetTileProperty<T>(Vector2Int id, T property) where T : struct, IConvertible => GetTileProperty(id, Convert.ToInt32(property));
        public void SetTileProperty<T>(Vector2Int id, T property, bool value) where T : struct, IConvertible  => SetTileProperty(id, Convert.ToInt32(property), value);
#endif
        
        public bool GetTileProperty(Vector2Int id, GridTileProperty property) => GetTileProperty(id, (int)property);

        public bool GetTileProperty(Vector2Int id, int property) {
            var bitset = GetTileProperties(id);
            return Bitset.Get(bitset, property);
        }
        
        public void SetTileProperty(Vector2Int id, GridTileProperty property, bool value) => SetTileProperty(id, (int)property, value);
        
        public void SetTileProperty(Vector2Int id, int property, bool value) {
            if (GetTileProperty(id, property) == value) return;
            var bitset = GetTileProperties(id);
            Bitset.Set(ref bitset, property, value);
            SetTileProperties(id, bitset);
        }

        public string GetTileData(Vector2Int id) {
            if (!IsInside(id)) return null;
            EnsureGridCapacity();
            return _cellData[IndexOf(id)].JsonData;
        }
        
        public T GetTileData<T>(Vector2Int id) {
            if (!IsInside(id)) return default;
            EnsureGridCapacity();
            var data = _cellData[IndexOf(id)].JsonData;
            return data == null || data.Equals("") ? default : JsonUtility.FromJson<T>(data);
        }
        
        public void GetTileDataOverwrite<T>(Vector2Int id, T existing) {
            if (!IsInside(id)) return;
            EnsureGridCapacity();
            var data = _cellData[IndexOf(id)].JsonData;
            if (data == null || data.Equals("")) return;
            JsonUtility.FromJsonOverwrite(data, existing);
        }

        public void SetTileData(Vector2Int id, string json) {
            if (!IsInside(id)) return;
            EnsureGridCapacity();
            var idx = IndexOf(id);
            var existing = _cellData[idx];
            _cellData[idx] = new CellData(existing.Properties, json);
            UpdateGridAround(id);
        }

        public void SetTileData<T>(Vector2Int id, T data) {
            if (!IsInside(id)) return;
            EnsureGridCapacity();
            var idx = IndexOf(id);
            var existing = _cellData[idx];
            var json = JsonUtility.ToJson(data, false);
            _cellData[idx] = new CellData(existing.Properties, json);
            UpdateGridAround(id);
        }

        private void UpdateGridAround(Vector2Int id) {
#if UNITY_EDITOR
            _requireFullUpdate = true;
#else
            RecreateTiles(id - Vector2Int.one, id + Vector2Int.one * 2);
#endif
        }

        private void EnsureGridCapacity() {
            var size = Size.x * Size.y;
            if (_cellData == null) {
                _cellData = new CellData[size];
            } else if (_cellData.Length < size) {
                var newArray = new CellData[size];
                for (var i = 0; i < _cellData.Length; i++) {
                    newArray[i] = _cellData[i];
                }
                _cellData = newArray;
            }
        }

        public void OnBeforeSerialize() {
            _tileData = Array.Empty<uint>();
        }

        public void OnAfterDeserialize() {
            // This is only for backwards compatability. The first time a grid
            // is loaded in version 1.1.0, tile properties are moved from
            // _tileData to _cellData. 
            if (_tileData.Length > 0) {
                EnsureGridCapacity();
                var count = Math.Min(_tileData.Length, Size.x * Size.y);
                for (var i = 0; i < count; i++) {
                    var properties = _tileData[i];
                    _cellData[i] = new CellData(properties, _cellData[i].JsonData);
                }
            }
        }

#if UNITY_EDITOR
        private bool _requireFullUpdate = false;
        private void OnValidate() {
            _requireFullUpdate = true;
        }

        private void UpdateOnceIfNecessary() {
            if (!_requireFullUpdate) return;
            _requireFullUpdate = false;
            RecreateTiles();
        }
        
        private void OnEnable() {
            if (!Application.isPlaying)
            {
                EditorApplication.update += UpdateOnceIfNecessary;
            }
        }
        
        private void OnDisable() {
            if (!Application.isPlaying)
            {
                EditorApplication.update -= UpdateOnceIfNecessary;
            }
        }
#endif

        private void RecreateTiles() {
            RecreateTiles(Vector2Int.zero, new Vector2Int(int.MaxValue, int.MaxValue));
        }
        
        private void RecreateTiles(Vector2Int from, Vector2Int to) {
            var selectedIds = new HashSet<Vector2Int>();
#if UNITY_EDITOR
            foreach (var selected in Selection.gameObjects) {
                if (selected.TryGetComponent<GridTile>(out var selectedTile)) {
                    selectedIds.Add(selectedTile.Id);
                }
            }
#endif
            
            var removeBuffer = new Queue<GameObject>();
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<GridTile>(out var tile)) {
                    var id = tile.Id;
                    if (tile.IsAttached && 
                            id.x >= from.x && id.x < to.x &&
                            id.y >= from.y && id.y < to.y) {
                        removeBuffer.Enqueue(child.gameObject);
                    }
                }
            }
            
            from.x = Math.Max(0, from.x);
            from.y = Math.Max(0, from.y);
            to.x = Math.Min(Size.x, to.x);
            to.y = Math.Min(Size.y, to.y);
            
            EnsureGridCapacity();

            while (removeBuffer.Count > 0) {
                var remove = removeBuffer.Dequeue();
                DestroyTile(remove);
            }

            var objectsToSelect = new List<UnityEngine.Object>();
            for (var i = from.y; i < to.y; i++) {
                for (var j = from.x; j < to.x; j++) {
                    var id = new Vector2Int(j, i);
                    var tile = CreateTile(id);
                    if (selectedIds.Contains(id)) {
                        objectsToSelect.Add(tile.gameObject);
                    }

                    var idx = IndexOf(id);
                    var tileData = _cellData[idx];
                    tile.Set(tileData.Properties, GetNeighboursOf(id), tileData.JsonData);
                    tile.SetAttached(true);
                }
            }

            var allTiles = new List<GridTile>();
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<GridTile>(out var tile)) {
                    var id = tile.Id;
                    if (selectedIds.Contains(id)) {
                        objectsToSelect.Add(tile.gameObject);
                    }
                    allTiles.Add(tile);
                }
            }
            
#if UNITY_EDITOR
            if (objectsToSelect.Count > 0) {
                Selection.objects = objectsToSelect.ToArray();
            }
#endif
            
            TilesCreated?.Invoke(this, allTiles.ToArray());
        }

        private uint[] GetNeighboursOf(Vector2Int id) {
            var neighbours = new uint[8];
            var step = Vector2Int.right;
            var diag = Vector2Int.one;
            for (var i = 0; i < 4; i++) {
                neighbours[i * 2]     = GetTileProperties(id + step);
                neighbours[i * 2 + 1] = GetTileProperties(id + diag);
                step = new Vector2Int(-step.y, step.x);
                diag = new Vector2Int(-diag.y, diag.x);
            }
            return neighbours;
        }
        
        private GridTile CreateTile(Vector2Int position) {
            GameObject obj;
            if (TilePrefab == null) {
                obj = new GameObject();
            } else {
                obj = Instantiate(TilePrefab);
            }

            obj.name = $"Tile ({position.x}, {position.y})";
            obj.transform.parent = transform;
            obj.transform.localPosition = new Vector3(position.x, 0.0f, position.y);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

            if (!obj.TryGetComponent<GridTile>(out var tile)) {
                tile = obj.AddComponent<GridTile>();
            }
            
            tile._id = position;
            return tile;
        }

        private void DestroyTile(GameObject child) {
            if (Application.isPlaying) {
                Destroy(child);
            } else {
                DestroyImmediate(child);
            }
        }

        private int IndexOf(Vector2Int id) => id.y * Size.x + id.x;
    }
}