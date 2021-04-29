using Nums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Engine.Gui;

namespace Engine.Editor {
    class TextEditor {
        public static TextEditor selected = null;
        
        static TextEditor() {
            Keyboard.onKeyPressed += keyboard_keypressed;
            Keyboard.onTextInput += keyboard_textinput;
        }

        static void keyboard_keypressed(key k, keymod m) {
            var s = selected;
            if (s == null) return;

            if (k == key.Up) s.moveCursor(0, -1); 
            else if (k == key.Down) s.moveCursor(0, 1); 
            else if (k == key.Left) s.moveCursor(-1, 0); 
            else if (k == key.Right) s.moveCursor(1, 0); 

            else if (k == key.Enter) {
                var sb = new StringBuilder(s.currentLine.ToString().Substring(s.cursor.x));
                s.currentLine.Remove(s.cursor.x, s.currentLine.Length - s.cursor.x);
                s.lines.AddAfter(s.lines.Find(s.currentLine), sb); 
                s.moveCursor(-s.cursor.x, 1);
            } else if (k == key.Backspace) {
                if (s.cursor.x == 0) {
                    if (s.cursor.y != 0) {
                        var o = s.lines.ElementAt(s.cursor.y - 1);
                        var l = o.Length;
                        o.Append(s.currentLine.ToString());
                        s.lines.Remove(s.currentLine);
                        s.moveCursor(l, -1);
                    }
                } else {
                    s.currentLine.Remove(s.cursor.x - 1, 1);
                    s.moveCursor(-1, 0);
                }
            } else if (k == key.Delete) {
                if (s.cursor.x == s.currentLine.Length) {
                    if (s.cursor.y != s.lines.Count-1) {
                        s.lines.ElementAt(s.cursor.y + 1).Insert(0, s.currentLine.ToString());
                        s.lines.Remove(s.currentLine);
                        s.moveCursor(0, 0);
                    }
                } else {
                    s.currentLine.Remove(s.cursor.x, 1);
                }
            } else if (k == key.Tab) {
                s.input("    ");
            }
        }


        static void keyboard_textinput(string text) {
            selected?.input(text);
        }
        
        
        LinkedList<StringBuilder> lines = new LinkedList<StringBuilder>();
        StringBuilder currentLine;

        ivec2 cursor = ivec2.zero;

        public TextEditor() {

            // add first initial line
            currentLine = new StringBuilder("Nice text rendering dude!");
            lines.AddLast(currentLine);


            setText(Assets.getShader("unlit").sources[OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader]);
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
            Console.notify(text + " was pressed");
            
            currentLine.Insert(cursor.x, text);
            cursor.x += text.Length;
        }

        public void render(Gui.Canvas canvas) {

            var textcolor = color.hex(0xd4d4d4ff);
            canvas.text(100, Font.arial, 22, "Some file title", textcolor);


            var textareapos = new vec2(110, 140);

            vec2 linepos = textareapos;
            int i = 0;
            foreach(var sb in lines) {
                canvas.text(linepos, Font.arial, 16, (i+1).ToString(), in textcolor);
                canvas.text((linepos.x + 25, linepos.y), Font.arial, 16, sb.ToString(), in textcolor);
                linepos.y += 16;
                i++;
            }

            // cursor
            canvas.rect(textareapos + (25 + Text.length(currentLine.ToString(), cursor.x, 16, Font.arial), cursor.y * 16), (2, 16), in color.white);

            canvas.rectborder(textareapos, (20, linepos.y - textareapos.y), 1, in textcolor);
            

            canvas.rect((canvas.width - 110, 100), 10, in color.gray);


            canvas.rect(textareapos, canvas.size - (220, 250), color.hex(0x2e2e2eff));
            canvas.rect(100, canvas.size - 200, color.hex(0x1e1e1eff));

        }
    }

}

