using Nums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Engine.Gui;

namespace Engine.Gui {
    public class Textbox {
        LinkedList<StringBuilder> lines = new LinkedList<StringBuilder>();
        StringBuilder currentLine;

        ivec2 cursor = ivec2.zero;

        public Font font = Font.monospaced;
        public int fontsize = 16;
        public color fontcolor = color.white;

        bool _editing = false;
        public bool editing {
            get => _editing;
            set {
                if (_editing == value) return;

                _editing = value;
                if (_editing) {
                    Keyboard.onKeyPressed += keyboard_keypressed;
                    Keyboard.onTextInput += keyboard_textinput;
                } else {
                    Keyboard.onKeyPressed -= keyboard_keypressed;
                    Keyboard.onTextInput -= keyboard_textinput;
                }
            }
        }
        
        bool force_cursor_visibility;

        public Textbox() {

            // add first initial line
            currentLine = new StringBuilder("Nice text rendering dude!");
            lines.AddLast(currentLine);

        }
        
        void keyboard_keypressed(key k, keymod m) {

            force_cursor_visibility = true;

            if (m == keymod.control) {
                switch (k) {
                    case key.Up: fontsize += 2; break;
                    case key.Down: fontsize -= 2; break;
                }
            } else {
                switch (k) {
                    case key.Up: moveCursor(0, -1); break;
                    case key.Down: moveCursor(0, 1); break;
                    case key.Left: moveCursor(-1, 0); break;
                    case key.Right: moveCursor(1, 0); break;


                    case key.Enter: 
                        var sb = new StringBuilder(currentLine.ToString().Substring(cursor.x));
                        currentLine.Remove(cursor.x, currentLine.Length - cursor.x);
                        lines.AddAfter(lines.Find(currentLine), sb); 
                        moveCursor(-cursor.x, 1);
                        break;

                    case key.Backspace:
                        if (cursor.x == 0) {
                            if (cursor.y != 0) {
                                var o = lines.ElementAt(cursor.y - 1);
                                var l = o.Length;
                                o.Append(currentLine.ToString());
                                lines.Remove(currentLine);
                                moveCursor(l, -1);
                            }
                        } else {
                            currentLine.Remove(cursor.x - 1, 1);
                            moveCursor(-1, 0);
                        }
                        break;

                    case key.Delete: 
                        if (cursor.x == currentLine.Length) {
                            if (cursor.y != lines.Count-1) {
                                lines.ElementAt(cursor.y + 1).Insert(0, currentLine.ToString());
                                lines.Remove(currentLine);
                                moveCursor(0, 0);
                            }
                        } else {
                            currentLine.Remove(cursor.x, 1);
                        }
                        break;
                        
                    case key.Tab: 
                        input("    ");
                        break;
                }
            }
        }


        void keyboard_textinput(string text) {
            input(text);
        }

        public void setText(string text) {
            lines.Clear();
            var ls = text.Split('\n').Select(x => x.TrimEnd('\r'));
            foreach (var line in ls) {
                lines.AddLast(new StringBuilder(line));
            }
            currentLine = lines.First.Value;
        }

        public string getText() {
            var res = new StringBuilder();
            foreach (var line in lines) {
                res.AppendLine(line.ToString());
            }
            return res.ToString();
        }

        private void moveCursor(int x, int y) {
            currentLine = lines.ElementAt(cursor.y = math.clamp(cursor.y += y, 0, lines.Count-1));
            cursor.x = math.clamp(cursor.x += x, 0, currentLine.Length);
        }

        private void input(string text) {            
            currentLine.Insert(cursor.x, text);
            cursor.x += text.Length;
        }

        public void render(Gui.Canvas canvas, vec2 offset) {

            //var textcolor = color.hex(0xd4d4d4ff);

            // text content
            vec2 linepos = offset;
            int i = 0;
            foreach(var sb in lines) {
                canvas.text(linepos, font, fontsize, sb.ToString(), in fontcolor);
                linepos.y += fontsize;
                i++;
            }


            // cursor
            const float blinktime = 1;
            if (force_cursor_visibility || Application.time % blinktime < blinktime / 2f) {
                var cursorpos = offset + new vec2(
                    x: Text.length(currentLine.ToString(), 0, cursor.x, fontsize, font), 
                    y: cursor.y * fontsize );

                canvas.rect(cursorpos, new vec2(2, fontsize), in color.white);

                force_cursor_visibility = false;
            }
         

        }
    }

}

