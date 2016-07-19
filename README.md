Simulating Weight in VR
=========
A quick exploration of methods for conveying “heaviness” in virtual reality
Read more in our Medium article [here](https://medium.com/@B__REEL/d161e87990b).

## Precompiled binaries
Download precompiled binaries from the [releases](releases) pages.

## How to run
You'll need [Unity v5.4.~](https://unity3d.com/unity/beta) and an HTC Vive.

After importing the project into Unity, choose between one of the two root scenes on the Project Browser:

  * Assets/Scenes/Scene1.unity
  * Assets/Scenes/Scene2.unity

Then hit play.  
After you're in, you can press the grip buttons to go to another scene.

## Components
Components must be attached to game objects containing at least 1 collider and a rigid body.

* InputController - Handles active controllers (one per scene required)
* DragByParenting - Attach to an object you want to be able to pick up
* DragWithJoint - Direct link - see [Medium](https://medium.com/@B__REEL/d161e87990b) article for more details
* DragByChangingPhysics - Loose link - see [Medium](https://medium.com/@B__REEL/d161e87990b) article for more details
* PlaySoundOnCollision - Plays an audio source when the current game object collides with something else.

## Creating a new scene

1. Create an empty game object and attach the InputController component to it.
2. Drag all SteamVR prefabs into your scene: [SteamVR], [Status] and [CameraRig]
3. Add one of the components above to your game objects.

Feel free to fork the repository or submit an [issue](https://github.com/B-Reel/vr-weight/issues) with questions or feedback.

Tack
/ B-Reel
