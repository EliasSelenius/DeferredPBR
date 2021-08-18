using System;
using System.Reflection;
using Nums;

namespace Engine.Gui {
    public class Common {

        const BindingFlags bindingflags = 
            BindingFlags.Public | 
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        /*
        
            bool => checkbox
            string => textbox
            float, int, etc.. => numberfield
            [Range(0, 1)] => slider
            [Ignore] => dont display
            color => color picker
            vec3 => numberfield, numberfield, numberfield
            quat => euler
            enum => dropdown
        */
        public static void displayObject(Canvas canvas, vec2 startPos, object obj) {

            var type = obj.GetType();
            var fields = type.GetFields(bindingflags);

            vec2 pos = startPos;
            for (int i = 0; i < fields.Length; i++) {
                var field = fields[i];


                // boolean, checkbox
                if (field.FieldType == typeof(bool)) {
                    var v = (bool)field.GetValue(obj);
                    
                    canvas.checkbox(pos, ref v, size:10);
                    pos += 10 + 4;

                    field.SetValue(obj, v);
                }

                // name:
                //canvas.text(pos, )


                pos.x = startPos.x;
            }

        }


        public class TestObject {
            bool is_somth;
            public bool is_here;
            string name;
        }
    }
}