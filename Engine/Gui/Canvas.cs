using System.Collections.Generic;

namespace Engine.Gui {

    /*
        Canvas
            encapsulates all gui classes
        
        takes care of batching text
    */

    public class Canvas {
        readonly List<View> views = new();

        public int width, height;

        public Canvas(int w, int h) {
            (width, height) = (w, h);
        }

        public void resize(int w, int h) {
            (width, height) = (w, h);
        }

        public void addView(View view) {
            views.Add(view);
            view.canvas = this;
        }
        
        public void render() {
            for (int i = 0; i < views.Count; i++) {
                views[i].render();
            }
        }

    }
}