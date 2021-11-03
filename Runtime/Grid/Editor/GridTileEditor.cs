using System;
using System.Linq;
using System.Text.RegularExpressions;
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

        public override void OnInspectorGUI() {
            var allTiles = targets.OfType<GridTile>().ToArray();
            if (allTiles.Length == 0) return;

            EditorGUILayout.BeginVertical();

            DrawHeaders();
            DrawRows(allTiles);

            EditorGUILayout.EndVertical();
        }

        private void DrawHeaders() {
            var propertyNames  = Enum.GetNames(typeof(GridTileProperty)).Select(FormatName).ToArray();
            var propertyValues = Enum.GetValues(typeof(GridTileProperty)).Cast<GridTileProperty>().ToArray();
            
            var maxWidth = 0.0f;
            for (var propIdx = 0; propIdx < propertyValues.Length; propIdx++) {
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
            
            for (var propIdx = 0; propIdx < propertyValues.Length; propIdx++) {
                var content = new GUIContent(propertyNames[propertyValues.Length - propIdx - 1]);
                var textDimensions = GUI.skin.label.CalcSize(content);
                rect.x = originalX + maxWidth - textDimensions.x;
                GUI.Label(rect, content);
                rect.Set(rect.x, rect.y + PropertyColumnWidth + PropertyColumnPadding, rect.width, rect.height);
            }
            
            GUI.matrix = prevMatrix;
        }

        private void DrawRows(GridTile[] tiles) {
            var propertyValues = Enum.GetValues(typeof(GridTileProperty)).Cast<GridTileProperty>().ToArray();
            
            // Draw Center
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Center", GUILayout.Width(LabelColumnWidth));
            for (var propIdx = 0; propIdx < propertyValues.Length; propIdx++) {
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
                for (var propIdx = 0; propIdx < propertyValues.Length; propIdx++) {
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

        private static string FormatName(string name) {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
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