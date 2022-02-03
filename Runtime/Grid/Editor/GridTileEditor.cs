using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Grid.Editor {
    [CustomEditor(typeof(GridTile)), CanEditMultipleObjects]
    public class GridTileEditor : UnityEditor.Editor {

        private static readonly string[] NeighbourNames = {
            "Right",
            "Up Right",
            "Up",
            "Up Left",
            "Left",
            "Down Left",
            "Down",
            "Down Right"
        };

        private const int Self = -1;
        private const float LabelColumnWidth = 80;
        private const float PropertyColumnWidth = 24;
        private const float PropertyColumnPadding = 3;

        private Grid _grid;
        private string[] _propertyNames;
        private int _propertyNameSelected = 0;

        private void OnEnable() {
            _grid = FindObjectOfType<Grid>();
            var types = StringUtil.TilePropertyTypes();
            if (_grid == null) {
                _propertyNameSelected = types.IndexOf(typeof(GridTileProperty));
                if (_propertyNameSelected < 0) _propertyNameSelected = 0;
                if (_propertyNameSelected < types.Count) {
                    _propertyNames = StringUtil.TilePropertyNames(types[_propertyNameSelected]);
                } else {
                    _propertyNames = new [] { "Type A", "Type B" };
                }
            } else {
                _propertyNames = _grid.PropertyNames;
                var actualNames = _grid.PropertyNames;
                for (var i = 0; i < types.Count; i++) {
                    var currentNames = StringUtil.TilePropertyNames(types[i]);
                    if (actualNames.Length != currentNames.Length) continue;
                    var found = true;
                    for (var j = 0; j < actualNames.Length; j++) {
                        if (actualNames[j] != currentNames[j]) {
                            found = false;
                            break;
                        }
                    }

                    if (found) {
                        _propertyNameSelected = i;
                        break;
                    }
                }
            }
        }

        private void OnDisable() {
            _grid = null;
        }

        public override void OnInspectorGUI() {
            var allTiles = targets.OfType<GridTile>().ToArray();
            if (allTiles.Length == 0) return;

            if (_grid == null || !allTiles[0].IsAttached) {
                var types = StringUtil.TilePropertyTypes();
                
                EditorGUI.BeginChangeCheck();
                _propertyNameSelected = EditorGUILayout.Popup("Tile Property Enum", _propertyNameSelected,
                    types.Select(t => t.Name)
                        .Select(StringUtil.FormatName).ToArray());
                if (_propertyNameSelected > types.Count) _propertyNameSelected = types.Count - 1;
                if (_propertyNameSelected < 0) _propertyNameSelected = 0;
                if (EditorGUI.EndChangeCheck()) {
                    if (_propertyNameSelected < types.Count) {
                        _propertyNames = StringUtil.TilePropertyNames(types[_propertyNameSelected]);
                    }
                }
            } else {
                _propertyNames = GetPropertyNames();
            }

            EditorGUILayout.BeginVertical();

            DrawHeaders();
            DrawRows(allTiles);

            EditorGUILayout.EndVertical();
        }

        private void DrawHeaders() {
            var propertyNames = _propertyNames;
            
            var maxWidth = 0.0f;
            for (var propIdx = 0; propIdx < propertyNames.Length; propIdx++) {
                var content = new GUIContent(propertyNames[propIdx]);
                var textDimensions = GUI.skin.label.CalcSize(content);
                if (textDimensions.x > maxWidth) {
                    maxWidth = textDimensions.x;
                }
            }

            var rect = GUILayoutUtility.GetRect(100, maxWidth);
            rect.Set(rect.x + LabelColumnWidth + (PropertyColumnWidth + PropertyColumnPadding) * propertyNames.Length, 
                rect.y, rect.width - LabelColumnWidth, PropertyColumnWidth);

            var originalX = rect.x;
            var prevMatrix = GUI.matrix;
            
            GUIUtility.RotateAroundPivot(90, rect.position);
            
            for (var propIdx = 0; propIdx < propertyNames.Length; propIdx++) {
                var content = new GUIContent(propertyNames[propertyNames.Length - propIdx - 1]);
                var textDimensions = GUI.skin.label.CalcSize(content);
                rect.x = originalX + maxWidth - textDimensions.x;
                GUI.Label(rect, content);
                rect.Set(rect.x, rect.y + PropertyColumnWidth + PropertyColumnPadding, rect.width, rect.height);
            }
            
            GUI.matrix = prevMatrix;
        }

        private void DrawRows(GridTile[] tiles) {
            var propertyNames = _propertyNames;
            
            // Draw Center
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Center", GUILayout.Width(LabelColumnWidth));
            for (var propIdx = 0; propIdx < propertyNames.Length; propIdx++) {
                var oldValue = tiles[0].GetProperty(propIdx);
                EditorGUI.BeginChangeCheck();

                GUIStyle style = GUI.skin.toggle;
                if (ShowMixedValue(tiles, Self, propIdx)) {
                    style = new GUIStyle("ToggleMixed");
                }
                
                var newValue = GUILayout.Toggle(oldValue, GUIContent.none, style, GUILayout.Width(PropertyColumnWidth));
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObjects(tiles.Select(t => (UnityEngine.Object) t.gameObject).ToArray(), "Edit Tile Property");
                    foreach (var tile in tiles) {
                        tile.SetProperty(propIdx, newValue);
                        EditorUtility.SetDirty(tile.gameObject);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Draw surrounding
            for (var neighbourIdx = 0; neighbourIdx < 8; neighbourIdx++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(NeighbourNames[neighbourIdx], GUILayout.Width(LabelColumnWidth));

                var attached = tiles[0].IsAttached;
                EditorGUI.BeginDisabledGroup(attached);
                for (var propIdx = 0; propIdx < propertyNames.Length; propIdx++) {
                    ShowMixedValue(tiles, neighbourIdx, propIdx);
                    var oldValue = tiles[0].GetNeighbourProperty(neighbourIdx, propIdx);
                    if (!attached) EditorGUI.BeginChangeCheck();
                    var newValue = GUILayout.Toggle(oldValue, GUIContent.none, GUILayout.Width(PropertyColumnWidth));
                    if (!attached && EditorGUI.EndChangeCheck()) {
                        Undo.RecordObjects(tiles.Select(t => (UnityEngine.Object) t.gameObject).ToArray(), "Edit Tile Neighbour Property");
                        foreach (var tile in tiles) {
                            tile.SetNeighbourProperty(neighbourIdx, propIdx, newValue);
                            EditorUtility.SetDirty(tile.gameObject);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private string[] GetPropertyNames() {
            string[] propertyNames;
            if (_grid != null && _grid.TryGetPropertyNames(out var names)) {
                propertyNames = names.Select(StringUtil.FormatName).ToArray();
            } else {
                propertyNames = Enum.GetNames(typeof(GridTileProperty)).Select(StringUtil.FormatName).ToArray();
            }
            return propertyNames;
        }

        private static bool ShowMixedValue(GridTile[] selection, int neighbour, int property) {
            var first = selection[0].GetNeighbourProperty(neighbour, property);
            if (selection.Any(t => first != t.GetNeighbourProperty(neighbour, property))) {
                EditorGUI.showMixedValue = true;
                return true;
            }
            EditorGUI.showMixedValue = false;
            return false;
        }
    }
}