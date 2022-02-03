using System;
using System.Linq;
using UnityEditor;

namespace Grid.Editor {
    [CustomEditor(typeof(Grid))]
    public class GridEditor : UnityEditor.Editor {
        
        private string[] _propertyNames;
        private int _propertyNameSelected = 0;
        
        private void OnEnable() {
            var types = StringUtil.TilePropertyTypes();
            _propertyNameSelected = types.IndexOf(typeof(GridTileProperty));
            
            var t = target as Grid;
            if (t != null) {
                var actualNames = t.PropertyNames;
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

            if (_propertyNameSelected < 0) _propertyNameSelected = 0;
            if (_propertyNameSelected < types.Count) {
                _propertyNames = StringUtil.TilePropertyNames(types[_propertyNameSelected]);
            } else {
                _propertyNames = new [] { "Type A", "Type B" };
            }
            
            
        }

        private void OnDisable() {
            
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var t = target as Grid;
            if (t == null) return;
            
            EditorGUILayout.Separator();
            
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

                t.PropertyNames = _propertyNames;
            }
        }
    }
}