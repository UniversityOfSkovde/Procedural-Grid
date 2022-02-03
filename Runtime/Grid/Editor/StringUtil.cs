using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Grid.Editor {
    public static class StringUtil {
        public static string FormatName(string name) {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
        }

        public static List<Type> TilePropertyTypes() {
            return TypeCache.GetTypesDerivedFrom<Enum>()
                .Where(t => t.Namespace == "Grid")
                .ToList();
        }

        public static string[] TilePropertyNames(Type enumType) {
            return enumType.GetEnumNames().Select(FormatName).ToArray();
        }
    }
}