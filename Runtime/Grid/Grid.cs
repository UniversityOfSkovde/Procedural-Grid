using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Grid {
    [ExecuteAlways]
    public sealed class Grid : MonoBehaviour {

        [Min(0)]
        [Tooltip("The size (in tiles) of the map in X and Z")]
        public Vector2Int Size = new Vector2Int(5, 5);

        [SerializeField]
        [Tooltip("Prefab that is spawned for each tile in the Dungeon.")]
        private GameObject TilePrefab;
        
        [SerializeField, HideInInspector]
        private uint[] _tileData;
        
        void Start() {
            RecreateTiles();
        }

        public void SetTileProperties(Vector2Int id, uint properties) {
            if (id.x < 0 || id.x >= Size.x ||
                id.y < 0 || id.y >= Size.y) return;
            EnsureGridCapacity();
            _tileData[id.y * Size.x + id.x] = properties;
#if UNITY_EDITOR
            _requireFullUpdate = true;
#else
            Regenerate(id - Vector2Int.one, id + Vector2Int.one * 2);
#endif
        }

        private void EnsureGridCapacity() {
            var size = Size.x * Size.y;
            if (_tileData == null) {
                _tileData = new uint[size];
            } else if (_tileData.Length < size) {
                var newArray = new uint[size];
                for (var i = 0; i < _tileData.Length; i++) {
                    newArray[i] = _tileData[i];
                }
                _tileData = newArray;
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
            EditorApplication.update += UpdateOnceIfNecessary;
        }
        
        private void OnDisable() {
            EditorApplication.update -= UpdateOnceIfNecessary;
        }
#endif

        private void RecreateTiles() {
            RecreateTiles(Vector2Int.zero, new Vector2Int(int.MaxValue, int.MaxValue));
        }
        
        private void RecreateTiles(Vector2Int from, Vector2Int to) {
            var selectedIds = new HashSet<Vector2Int>();
            foreach (var selected in Selection.gameObjects) {
                if (selected.TryGetComponent<GridTile>(out var selectedTile)) {
                    selectedIds.Add(selectedTile.Id);
                }
            }
            
            var removeBuffer = new Queue<GameObject>();
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<GridTile>(out var tile)) {
                    var id = tile.Id;
                    if (tile._attached && 
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
                    
                    tile.Set(_tileData[id.y * Size.x + id.x], GetNeighboursOf(id));
                    tile._attached = true;
                }
            }
            
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<GridTile>(out var tile)) {
                    var id = tile.Id;
                    if (selectedIds.Contains(id)) {
                        objectsToSelect.Add(tile.gameObject);
                    }
                }
            }

            if (objectsToSelect.Count > 0) {
                Selection.objects = objectsToSelect.ToArray();
            }
        }

        private uint[] GetNeighboursOf(Vector2Int id) {
            var neighbours = new uint[8];
            var step = Vector2Int.right;
            var diag = Vector2Int.one;
            for (var i = 0; i < 4; i++) {
                neighbours[i * 2]     = TryGetBitset(id + step);
                neighbours[i * 2 + 1] = TryGetBitset(id + diag);
                step = new Vector2Int(-step.y, step.x);
                diag = new Vector2Int(-diag.y, diag.x);
            }
            return neighbours;
        }

        private uint TryGetBitset(Vector2Int id) {
            if (id.x < 0 || id.x >= Size.x ||
                id.y < 0 || id.y >= Size.y) return 0;
            return _tileData[id.y * Size.x + id.x];
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
            obj.hideFlags = HideFlags.DontSave;

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
    }
}