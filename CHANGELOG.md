# Changelog

## [1.0.0-pre.2] - 2023-02-27
### Added
- Added support for individual frame timings in animation clips.
- Added support for layer groups.
- Added support for Layer & Cel opacity.
- Added support for repeating/non repeating tags/clips.

### Changed
- The importer UI is now re-written in UI Toolkit.
- If a Model Prefab only contains one SpriteRenderer, all components will be placed on the root GameObject, rather than generating a single GameObject to house them.
- A Sorting Group component is added by default to Model Prefabs with more than one Sprite Renderer.

### Fixed
- Fixed an issue where renaming an Asperite file in Unity would throw a null reference exception. (DANB-384)
- Fixed an issue where the background importer would import assets even when Unity Editor has focus.
- Fixed an issue where the Pixels Per Unit value could be set to invalid values.

## [1.0.0-pre.1] - 2023-01-06
### Added
- First release of this package.