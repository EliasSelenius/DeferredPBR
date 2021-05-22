using System.Collections.Generic;

namespace Engine.Toolset {
    public static partial class Editor {

        public enum MessageLevel { notification, warning, error }
        record Entry(string message, MessageLevel level);
        static List<Entry> messageEntries = new();
        
        public static void notify(string msg) => messageEntries.Add(new Entry(msg, MessageLevel.notification));
        public static void warn(string msg) => messageEntries.Add(new Entry(msg, MessageLevel.warning));
        public static void error(string msg) => messageEntries.Add(new Entry(msg, MessageLevel.error));

        internal static void renderConsole() {
            float y = 0;
            for (int i = 0; i < messageEntries.Count; i++) {
                var e = messageEntries[i];
                canvas.text((0, y), Gui.Font.arial, 16, e.message, getMessageColor(e.level));
                y += 16;
            }
        }

        static color getMessageColor(MessageLevel level) => level switch { MessageLevel.notification => color.white, MessageLevel.warning => color.yellow, MessageLevel.error => color.red, _ => throw new System.Exception("Ooops") };

    }

}