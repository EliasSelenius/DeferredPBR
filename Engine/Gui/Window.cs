
using System.Collections.Generic;
using Nums;
using System;

namespace Engine.Gui {

    /*public class WindowingSystem : View {
        
        readonly LinkedList<Window> windows = new LinkedList<Window>();
        internal Window currentWindow;
        internal bool isWindowSelected;
        vec2 mouse_offset;

        public ColorTheme theme = ColorTheme.darkGreenish;

        vec2 initialWindPos = 50;
        internal vec2 initialWindowPosition => initialWindPos += 30; 


        public void addWindow(Window window) {
            windows.AddLast(window);
            currentWindow = window;
            window.canvas = this.canvas;
            window.attach(this);
        }

        internal void select(Window w) {
            currentWindow = w;
            isWindowSelected = true;
            mouse_offset = Mouse.position - w.window_pos;
        }

        
        protected override void renderBody() {
            foreach (var window in windows) {
                window.render();
                if (Utils.insideBounds(Mouse.position - window.window_pos, window.window_size) && Mouse.isPressed(MouseButton.left)) currentWindow = window;
            }
            

            if (currentWindow != null && currentWindow != windows.Last.Value) {
                // make sure the current window is drawn ontop of all other windows
                windows.Remove(currentWindow);
                windows.AddLast(currentWindow);   
            }

            if (Mouse.isReleased(MouseButton.left)) isWindowSelected = false;
            if (isWindowSelected) {
                currentWindow.window_pos = Mouse.position - mouse_offset;
            }

        }
    }

    public class Window : View {

        public bool active = true;

        public vec2 window_pos = -1;
        public vec2 window_size;
        public string title;

        bool render_background = true;

        public WindowingSystem system { get; private set; }

        public Window(string title, vec2 s) {
            window_size = s;
            this.title = title;
        }


        internal void attach(WindowingSystem sys) {
            system = sys;

            if (window_pos.x == -1f) window_pos = system.initialWindowPosition;

            onAttached();
        }
        protected virtual void onAttached() { }


        protected virtual void renderContent() {}
        protected override void renderBody() {
            if (!active) return;
            
            start(window_pos, (window_size.x, 22));
            

            if (hovering()) {
                if (Mouse.isPressed(MouseButton.left)) system.select(this);
            } 

            // fill the titlebar
            if (system.currentWindow == this && system.isWindowSelected) {
                fill(system.theme.primaryColor * 1.4f);
            } else {
                fill(system.theme.primaryColor);
            }

            // close button
            start((width - 22, 0), 22); 
                fill(color.white); 
                if (hovering() && Mouse.isReleased(MouseButton.left)) active = false; 
            end();
            
            
            // window title:
            //start((100, 22));
            //fill(color.silver);
            //text(title, 22, system.theme.textColor);
            text(title, 22, system.theme.textColor);
            //end();



            // window content
            start((0, height), window_size);
                fill(system.theme.backgroundColor);
                renderContent();
            end();
            end();
        }

    }


    class DebugWindow : Window {        

        public DebugWindow() : base("Debug Info", (300, 100)) {
        }

        protected override void renderContent() {
        
            text("Time: " + (Renderer.deltaTime * 1000.0).ToString("##.#") + "ms, fps: " + Renderer.fps.ToString(), 16, system.theme.textColor);

            start(new vec2(0, 16), new vec2(width / 2f, 16));
            if (button("fullscreen", size)) {
                
                Application.window.WindowState = Application.window.WindowState == OpenTK.Windowing.Common.WindowState.Fullscreen ?
                    OpenTK.Windowing.Common.WindowState.Maximized : OpenTK.Windowing.Common.WindowState.Fullscreen; 
            }
            end();

        }
    }*/

    public class WindowSystem {
        readonly LinkedList<Window> windows = new();
        public ColorTheme theme = ColorTheme.darkGreenish;

        public void addWindow(Window window) {
            window.sys = this;
            windows.AddLast(window);
        }

        public void render(Canvas canvas) {

            foreach (var window in windows) {
                window.render(canvas);
            }
        }
    }

    public class Window {
        public WindowSystem sys { get; internal set; }

        string title = "Hello Window";
        int titlebarHeight = 18;
        vec2 pos = 100, size = (320, 200);

        public void render(Canvas canvas) {
            // title bar
            canvas.rect(pos, new vec2(size.x, titlebarHeight), sys.theme.primaryColor);
            canvas.text(pos, Font.arial, titlebarHeight, title, sys.theme.textColor);

            // content area
            canvas.rect(pos, size, sys.theme.backgroundColor);
        }
    }
}