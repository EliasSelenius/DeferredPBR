
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
        
        public static vec2 position => new vec2(Application.window.MousePosition.X, Application.window.MousePosition.Y);
        public static vec2 ndcPosition {
            get {
                var p = position / new vec2(Renderer.windowWidth, Renderer.windowHeight) * 2f - vec2.one;
                p.y = -p.y;
                return p;
            }
        }

        public static vec2 delta => new vec2(Application.window.MouseState.Delta.X, Application.window.MouseState.Delta.Y);
        public static float wheeldelta => Application.window.MouseState.ScrollDelta.Y;

        private static MouseState _state;
        public static MouseState state {
            get => _state;
            set {
                _state = value;
                // NOTE: OpenTKs CursorVisible/CursorGrabbed api is silly.
                switch (_state) {
                    case MouseState.free:
                        Application.window.CursorVisible = true;
                        break;
                    case MouseState.disabled:
                        Application.window.CursorGrabbed = true;
                        break;
                    case MouseState.hidden:
                        Application.window.CursorVisible = false;
                        break;
                }
            }
        }


        public static bool isDown(MouseButton btn) => Application.window.IsMouseButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
        public static bool isPressed(MouseButton btn) => Application.window.IsMouseButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
        public static bool isReleased(MouseButton btn) => Application.window.IsMouseButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)btn);
    }
}
