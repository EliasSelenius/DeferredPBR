using System.Collections.Generic;

namespace Engine.Toolset {
    public static class Console {

        public enum Level {
            notification, warning, error
        }

        record Entry(string message, Level level, float lifetime = 5) {
            public readonly double borntime = Application.time;
        }
        static List<Entry> entries = new();
        
        public static void notify(string msg) => entries.Add(new Entry(msg, Level.notification));
        public static void warn(string msg) => entries.Add(new Entry(msg, Level.warning));
        public static void error(string msg) => entries.Add(new Entry(msg, Level.error));

        internal static void render(Gui.Canvas canvas) {
            float y = 0;
            for (int i = 0; i < entries.Count; i++) {
                var e = entries[i];
                if (Application.time - e.borntime > e.lifetime) {
                    entries.Remove(e);
                    i--;
                }
                
                canvas.text((0, y), Gui.Font.arial, 16, e.message, e.level switch { Level.notification => color.white, Level.warning => color.yellow, Level.error => color.red });
                y += 16;
            }
        } 

    }

}