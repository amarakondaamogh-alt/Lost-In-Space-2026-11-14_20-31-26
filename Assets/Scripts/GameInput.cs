using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// One place that reads player input. Works whether Unity's Player Settings uses the
// old Input Manager, the new Input System package, or both.
public static class GameInput
{
    public static float Horizontal()
    {
#if ENABLE_INPUT_SYSTEM
        float v = 0f;
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) v -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) v += 1f;
        }

        Gamepad g = Gamepad.current;
        if (g != null && Mathf.Abs(v) < 0.01f)
        {
            float x = g.leftStick.x.ReadValue();
            if (Mathf.Abs(x) > 0.2f) v = x;
        }
        return Mathf.Clamp(v, -1f, 1f);
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    public static bool JumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null && (k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame))
            return true;

        Gamepad g = Gamepad.current;
        return g != null && g.buttonSouth.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }

    public static bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame))
            return true;

        Gamepad g = Gamepad.current;
        return g != null && g.startButton.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
    }

    // R = go back to the last checkpoint.
    public static bool RespawnPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null && k.rKey.wasPressedThisFrame)
            return true;

        Gamepad g = Gamepad.current;
        return g != null && g.buttonWest.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    // M = mute / unmute the music.
    public static bool MutePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        return k != null && k.mKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.M);
#endif
    }
}
