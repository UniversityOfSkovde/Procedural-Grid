# Procedural Grid
Grid system for Unity that generates and updates a square grid of objects. Each object can then generate its own geometry based on information abou tits neighbours.

## Installation
You can add this package to your project by going to `Window -> Package Manager -> + -> Add Package From git URL...` and then enter `https://github.com/UniversityOfSkovde/Procedural-Grid.git`.

## Getting Started
To try out the grid, first you need something that can draw to the scene view. Create a new script and call it "Piece". Add the following:
```csharp
using Grid;
using UnityEngine;

[RequireComponent(typeof(GridTile))]
public class Piece : MonoBehaviour {
    private void OnDrawGizmos() {
        var tile = GetComponent<GridTile>();
        if (tile.GetProperty(GridTileProperty.Solid)) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawCube(transform.position, new Vector3(1, 0.1f, 1));
    }
}
```

Go back to Unity, right-click in the hierarchy and create a new Empty GameObject, and drag the new script into its inspector panel. It should render a flat, green cube in the scene view. Try pressing the top-left checkbox in the "GridTile" component to make the tile solid. that you make the tile red.

To get a complete grid and not just a tile, click and drag the game object from the hierarchy into the Assets-panel to create a new prefab. Then delete the object from the scene. Right-click in the hierarchy and create a new Empty GameObject. This time, add the existing component "Grid" to it (the C#-script, not the built-in Unity component). As the "Tile Prefab", pick the prefab you just created. The object will automatically generate a grid of instances for you. If you click on one of them, you will see that the "neighbours" checkboxes in the "GridTile"-component are disabled. They will automatically get the value from their neighbours. You can use that information in your script like this:

```csharp
if (tile.GetNeighbourProperty(0, GridTileProperty.Solid)) {
    Gizmos.color = Color.yellow;
}
```

Each neighbour has an index. The indices goes in counter-clockwise order starting at the neighbour to the right (index = 0), then up-right (index = 1), then up (index = 2) and so on. `GridTileProperty` is an enum that defines all the boolean properties that can be stored in the grid. By default, it has two properties; `Solid` and `Water`. If you copy this enum into your Assets-folder, you can change it and the GUI will automatically pick up the new properties. Just make sure you still set the namespace to `Grid`.

```csharp
namespace Grid {
    public enum MyTestEnum {
        Forest, Hills, Mountains, River
    }
}
```

## License
Copyright 2021 Emil Forslund

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
