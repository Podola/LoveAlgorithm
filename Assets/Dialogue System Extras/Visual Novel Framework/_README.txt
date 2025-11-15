/*
------------------------------
  Dialogue System for Unity  
    Visual Novel Framework
        
  Copyright © Pixel Crushers
------------------------------

This visual novel framework is a free add-on for the Dialogue System for Unity.

Instructions in are the file Visual_Novel_Framework_Manual.pdf.

-----------------------------------------------------------------------
VERSION HISTORY:

VERSION 2.0.16:
- Background manager now restores background image when loading saved game.

VERSION 2.0.15:
- When deleting all saved games, start menu's Continue button changes back to Start.
- When loading saved game, no longer briefly loads start menu scene.

VERSION 2.0.14:
- Added ability to cross-fade backgrounds. (Use new Background Canvas prefab or add Background 2 image to yours.)

VERSION 2.0.13:
- Updated localization text table.

VERSION 2.0.12:
- Minor bug fixes.

VERSION 2.0.11:
- Added Background(imagename) sequencer command.

VERSION 2.0.10:
- Configured Gameplay scene for instant playtesting from scene.
- Fixed issue with Credits button.

VERSION 2.0.9:
- Options Panel now lets you set default values for audio volumes & character speed.

VERSION 2.0.8:
- Visual Novel Menu Canvas menu panels are now nested prefabs for easier customization.
- Removed redundant UnityEvent from QuitToMenuConfirm button that activated Start Panel.

VERSION 2.0.7:
- Added History.
- Added Backtrack.
- Added Auto Play toggle.
- Added Skip to player response menu.

VERSION 2.0.6.1:
- Backgrounds can load from addressables, assetbundle, or Resources.

VERSION 2.0.5:
- Updated to Unity 2019.3.

VERSION 2.0.4.4:
- Added checkbox to Achievements to reset when starting new game.

VERSION 2.0.4.3:
- RememberCurrentDialogueEntry updated to use new ConversationStateSaver.

VERSION 2.0.4.2:
- Minor updates for Dialogue System 2.1.7+.

VERSION 2.0.4:
- Updated for compatibility with Ink. (Define symbol USE_INK)
- Fixed: Start scene pointed to Example Gameplay scene instead of Gameplay scene.

VERSION 2.0.3:
- Added Achievements.
- Added Quest Log button.
- Updated VNLoadLevel() sequencer command to support Save System.
- Panels now support open/close animations.

VERSION 2.0.2:
- Fixed saving conversation state, which was broken due to incomplete switch to new 
  save system components on prefabs.
- Runtime changes to Actor's Background field are now supported.
- Added SceneTransitionManager component to Dialogue Manager.

VERSION 2.0.1:
- Fixed Visual Novel Canvas prefab to point to Gameplay scene, not Example Gameplay.

VERSION 2.0:
- Updated for Dialogue System for Unity 2.0.

VERSION 1.2:
- Checks Background field in conversations, dialogue entries, and actors.
- Checks CurrentStage field in conversations and dialogue entries. 
  (Value shown in saved game slot summary.)

VERSION 1.1:
- Added VNLoadLevel() to allow level changing.

VERSION 1.0:
- Initial version.
*/