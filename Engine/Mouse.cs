
using Nums;

namespace Engine {

    public enum MouseState {
        free,
        disabled,
        hidden
    }

    public enum MouseButton {
        left = 0,
        right = 1,
        middle = 2
    }

    public static class Mouse {
        
        public static vec2 position => new vec2(app.window.MousePosition.X, app.window.MousePosition.Y);
        public static vec2 ndcPosition {
            get {
                var p = position / new vec2(Renderer.windowWidth, Renderer.windowHeight) * 2f - vec2.one;
                p.y = -p.y;
                return p;
            }
        }


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


        public static bool isDown(MouseButton btn) => app.window.IsMouseButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
        public static bool isPressed(MouseButton btn) => app.window.IsMouseButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
        public static bool isReleased(MouseButton btn) => app.window.IsMouseButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
    }
}
