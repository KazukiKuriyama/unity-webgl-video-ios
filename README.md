# unity-webgl-video-ios
Video playback on UnityWebGL is very unstable on some platforms (especially iOS). This source code avoids the problem by dynamically passing textures between jslib and C#.

# Notes
* Currently, it is only possible to operate after building, and it cannot be operated on UnityEditor or other platforms.
* Due to browser security rules, even if you use this source code, you must tap the screen once before playing the audio. If the tap is made after the timing when the audio is originally played, the audio for that video will not be played. *Under investigation
  * To counteract this problem, this sample Scene does not automatically play the video, but requires the input operation of a button in advance.  
