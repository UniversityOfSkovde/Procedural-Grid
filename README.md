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

## License
Copyright 2021 Emil Forslund

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
