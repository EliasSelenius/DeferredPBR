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
        public bool editing = false;
        
        public Textbox() {

            Keyboard.onKeyPressed += keyboard_keypressed;
            Keyboard.onTextInput += keyboard_textinput;


            // add first initial line
            currentLine = new StringBuilder("Nice text rendering dude!");
            lines.AddLast(currentLine);


            setText(Assets.getShader("unlit").sources[OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader]);
        }
        
        void keyboard_keypressed(key k, keymod m) {

            if (k == key.Up) moveCursor(0, -1); 
            else if (k == key.Down) moveCursor(0, 1); 
            else if (k == key.Left) moveCursor(-1, 0); 
            else if (k == key.Right) moveCursor(1, 0); 

            else if (k == key.Enter) {
                var sb = new StringBuilder(currentLine.ToString().Substring(cursor.x));
                currentLine.Remove(cursor.x, currentLine.Length - cursor.x);
                lines.AddAfter(lines.Find(currentLine), sb); 
                moveCursor(-cursor.x, 1);
            } else if (k == key.Backspace) {
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
            } else if (k == key.Delete) {
                if (cursor.x == currentLine.Length) {
                    if (cursor.y != lines.Count-1) {
                        lines.ElementAt(cursor.y + 1).Insert(0, currentLine.ToString());
                        lines.Remove(currentLine);
                        moveCursor(0, 0);
                    }
                } else {
                    currentLine.Remove(cursor.x, 1);
                }
            } else if (k == key.Tab) {
                input("    ");
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

            var textcolor = color.hex(0xd4d4d4ff);

            // text content
            vec2 linepos = offset;
            const int fontsize = 16;
            int i = 0;
            foreach(var sb in lines) {
                canvas.text(linepos, Font.arial, fontsize, sb.ToString(), in textcolor);
                linepos.y += fontsize;
                i++;
            }


            // cursor
            var cursorpos = offset + new vec2(
                x: Text.length(currentLine.ToString(), 0, cursor.x, fontsize, Font.arial), 
                y: cursor.y * fontsize );

            canvas.rect(cursorpos, new vec2(2, fontsize), in color.white);

            

            //canvas.rect((canvas.width - 110, 100), 10, in color.gray);


            //canvas.rect(textareapos, canvas.size - (220, 250), color.hex(0x2e2e2eff));
            //canvas.rect(100, canvas.size - 200, color.hex(0x1e1e1eff));

        }
    }

}

