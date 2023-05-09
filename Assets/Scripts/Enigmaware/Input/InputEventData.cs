#region

using UnityEngine.InputSystem;

#endregion

/// <summary>
///     Generates data from new input system events.
/// </summary>
public class InputEventData
{
    public bool Down;
    public bool Pressed;
    public bool Up;

    public void RefreshKeyData()
    {
        Down = false;
        Up = false;
    }

    public void UpdateKeyState(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Up = false;
            Down = true;
            Pressed = true;
        }
        else if (context.canceled)
        {
            Up = true;
            Down = false;
            Pressed = false;
        }
    }
}