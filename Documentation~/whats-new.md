# What's new in the Aseprite Importer package

Discover new features and improvements in the latest updates to Aseprite Importer.

For more information and for fixes in other versions, refer to the [changelog](../changelog/CHANGELOG.html).

## Version 1.2.3

### The importer now recognizes layer ids

Aseprite Importer can now use the `Layer.uuid` property to identify each layer when you import a file, rather than the name and path. For more information, refer to [Frequently asked questions](ImporterFAQ.md#enable-uuid-for-layers).

## Version 1.2.0

### Tile Set import mode

You can now set **Import Mode** to **Tile Set** to read tile data from an Aseprite file and generate tilemap assets, including tile assets and a tile palette. For more information, refer to [Importer features](ImporterFeatures.md).

### Single event receiver for all animation events

You can now dispatch events to a single `OnAnimationEvent(string)` method, rather than multiple named methods. For more information, refer to [Frequently asked questions](ImporterFAQ.md#single-event-receiver-for-all-events).

## Additional resources

- [Changelog](../changelog/CHANGELOG.html)
