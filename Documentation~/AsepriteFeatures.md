# Aseprite Features
This page highlights which Aseprite feature the Aseprite Importer supports/does not support.

## Supported features
**File formats**
- .ase & .aseprite
- Color modes (All modes are supported)
    - RGBA
    - Grayscale
    - Indexed

**Layer settings**
- Visible/Hidden layer
    - Hidden layers are by default not imported. This can be changed by checking “Include hidden layers” in the import settings.
- Linked Cells
- Tags
    - Note: Only Animation Direction: Forward is supported.

## Unsupported features
- [Layer groups](https://www.aseprite.org/docs/layer-group/)
    - The importer will just skip the group object, but import the underlying layers.
    - The importer respects the visibility mode selected for the group. If a group is hidden, underlying layers will not be imported by default.
- Layer modes
    - All layers are imported as Layer Mode: Normal.
- Layer opacity
    - All layers are imported with 100% opacity.
- [Individual frame timings](https://www.aseprite.org/docs/frame-duration/)
    - The animation clips are created using the Constant Frame Rate (Frame > Constant Frame Rate). The importer will not take individual frame timings into consideration. 
- [Slices](https://www.aseprite.org/docs/slices/)
- [Tilemaps](https://www.aseprite.org/docs/tilemap/)