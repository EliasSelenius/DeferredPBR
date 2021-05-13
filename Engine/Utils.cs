
using Nums;
using static Nums.math;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine {
    public static class Utils {
        public static Vector3 toOpenTK(this vec3 v) => new Vector3(v.x, v.y, v.z);
        public static Vector4 toOpenTK(this vec4 v) => new Vector4(v.x, v.y, v.z, v.w);
        public static Matrix4 toOpenTK(this mat4 m) => new Matrix4(m.row1.toOpenTK(), m.row2.toOpenTK(), m.row3.toOpenTK(), m.row4.toOpenTK());


        public static vec2 toNums(this Vector2 v) => new vec2(v.X, v.Y);
        public static vec3 toNums(this Vector3 v) => new vec3(v.X, v.Y, v.Z);
        public static vec4 toNums(this Vector4 v) => new vec4(v.X, v.Y, v.Z, v.W);
        public static mat4 toNums(this Matrix4 m) => new mat4(m.Row0.toNums(), m.Row1.toNums(), m.Row2.toNums(), m.Row3.toNums());


        public static bool insideBounds(vec2 v, vec2 bounds) => v.x < bounds.x && v.x > 0 && v.y < bounds.y && v.y > 0;
        

        public static void invert(in mat4 m, out mat4 inv) => inv = toNums(toOpenTK(m).Inverted());

        public static color randColor(int seed) => color.rgba(math.range(seed, 0, 1), math.range(seed+1, 0, 1), math.range(seed+2, 0, 1), 1.0f);
        public static color randColor() => color.rgba(math.range(0, 1), math.range(0, 1), math.range(0, 1), 1.0f);


        public static void getTangentsOfNormal(in vec3 forward, out vec3 left, out vec3 up) {
            if (forward.dot(vec3.unity) > 0.999f) left = forward.cross(vec3.unitx);
            else left = forward.cross(vec3.unity);
            
            up = forward.cross(left);
        }


        private static readonly List<mat4> matrixStack = new List<mat4> { mat4.identity }; 
        public static mat4 currentMatrix => matrixStack[matrixStack.Count - 1];
        public static void pushMatrix() {
            matrixStack.Add(currentMatrix);
        }
        public static void popMatrix() {
            if (matrixStack.Count == 1) throw new System.Exception("too many pops");
            matrixStack.RemoveAt(matrixStack.Count - 1);
        }
        public static void translate(vec3 translation) {
            var c = currentMatrix;
            c.row4.xyz += translation;
            matrixStack[matrixStack.Count - 1] = c;
        }
        public static void scale(vec3 scale) {
            var s = new mat4(
                scale.x, 0, 0 ,0,
                0, scale.y, 0, 0,
                0, 0, scale.z, 0,
                0, 0, 0,       1
            );
            matrixStack[matrixStack.Count - 1] = s * currentMatrix;
        }
        public static void rotateX(float a) {
            var r = new mat4(1, 0, 0, 0,
                            0, cos(a), -sin(a), 0,
                            0, sin(a), cos(a), 0,
                            0, 0, 0, 1);
            matrixStack[matrixStack.Count - 1] = r * currentMatrix;
        }
        public static void rotateY(float a) {
            var r = new mat4(cos(a), 0, sin(a), 0,
                            0, 1, 0, 0,
                            -sin(a), 0, cos(a), 0,
                            0, 0, 0, 1);
            matrixStack[matrixStack.Count - 1] = r * currentMatrix;
        }
        public static void rotateZ(float a) {
            var r = new mat4(cos(a), -sin(a), 0, 0,
                            sin(a), cos(a), 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);
            matrixStack[matrixStack.Count - 1] = r * currentMatrix;
        }
        public static void rotate(float a, float b, float c) {

            matrixStack[matrixStack.Count - 1] = createRotation(a, b, c) * currentMatrix;
        }

        public static mat4 createRotation(float a, float b, float c) {
            float sina = sin(a),
                sinb = sin(b),
                sinc = sin(c),
                cosa = cos(a),
                cosb = cos(b),
                cosc = cos(c);
            return new mat4(cosb*cosc, -cosb*sinc, sinb, 0,
                            sina*sinb*cosc + cosa*sinc, -sina*sinb*sinc + cosa*cosc, -sina*cosb, 0,
                            -cosa*sinb*cosc + sina*sinc, cosa*sinb*sinc + sina*cosc, cosa*cosb, 0,
                            0,0,0,1);
        }



        public static color[,] bitmapToColorArray(System.Drawing.Bitmap bitmap) {
            var pixels = new color[bitmap.Width, bitmap.Height];
            // , bitmap.PixelFormat
            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bytes = new byte[data.Stride * bitmap.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            float toFloat(byte b) => ((float)b) / 255f;
            
            var bpp = data.Stride / data.Width;

            for (int i = 0; i < bytes.Length; i += bpp) {
                var pixelindex = (i / bpp);
                pixels[pixelindex % bitmap.Width, pixelindex / bitmap.Width] = new color {
                    r = toFloat(bytes[i + 2]),
                    g = toFloat(bytes[i + 1]),
                    b = toFloat(bytes[i]),
                    a = bpp == 4 ? toFloat(bytes[i + 3]) : 1f
                };
                
                //new color32(bytes[i + 2], bytes[i + 1], bytes[i], bpp == 4 ? bytes[i + 3] : (byte)255);
            }
            bitmap.UnlockBits(data);

            return pixels;
        }



    }
}

