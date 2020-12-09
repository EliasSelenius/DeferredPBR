
using Nums;

public enum MouseState {
    free,
    disabled,
    hidden
}

public static class Mouse {
    
    public static vec2 position => new vec2(app.window.MousePosition.X, app.window.MousePosition.Y);
    
    private static MouseState _state;
    public static MouseState state {
        get => _state;
        set {
            _state = value;
            // NOTE: OpenTKs CursorVisible/CursorGrabbed api is silly.
            switch (_state) {
                case MouseState.free:
                    app.window.CursorVisible = true;
                    break;
                case MouseState.disabled:
                    app.window.CursorGrabbed = true;
                    break;
                case MouseState.hidden:
                    app.window.CursorVisible = false;
                    break;
            }
        }
    }
}