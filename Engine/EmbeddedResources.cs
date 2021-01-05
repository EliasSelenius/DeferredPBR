
using System.Collections.Generic;

namespace Engine {
    internal static class EmbeddedResources {

        public static string getText(string name) {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(EmbeddedResources));
            using var s = assembly.GetManifestResourceStream(name);
            using var sr = new System.IO.StreamReader(s);
            return sr.ReadToEnd();
        }

        public static System.Drawing.Bitmap getBitmap(string name) {
            using var s = System.Reflection.Assembly.GetAssembly(typeof(EmbeddedResources)).GetManifestResourceStream(name);
            return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(s);
        }

        public static Shader getShader(string name) {
            var path = "DeferredPBR.data.shaders." + name;
            return new Shader(getText(path + ".frag.glsl"), getText(path + ".vert.glsl"));
        }

        public static System.Xml.XmlDocument getXml(string name) {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(getText(name));
            return doc;
        }
    }
}