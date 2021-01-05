using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Drawing;
using System.Linq;

namespace Engine {
    public static class Assets {

        static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        static Dictionary<string, string> shaderSources = new Dictionary<string, string>();

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static Dictionary<string, PBRMaterial> materials = new Dictionary<string, PBRMaterial>();

        static IResourceProvider provider;
        public static void load(IResourceProvider provider = null) {
            Assets.provider = provider;

            { // shader source files 
                foreach (var res in provider.enumerate("glsl")) {

                }
            }


            loadShaderSources();

            loadShaders();
            loadTextures();

            materials["default"] = PBRMaterial.defaultMaterial;

            Assets.provider = null;
        }

        public static Shader getShader(string name) => shaders[name];
        public static Texture2D getTexture2D(string name) => textures[name];
        public static PBRMaterial getMaterial(string name) => materials[name];


        static void loadFromXml(XmlDocument doc) {
            foreach (var elm in doc.DocumentElement.ChildNodes) {
                var xml = elm as XmlElement;
                var assetName = xml.GetAttribute("name");


                // shaders:
                if (xml.Name.Equals("shader")) {
                    shaders.Add(assetName, new Shader(shaderSources[xml.GetAttribute("fragsrc")], shaderSources[xml.GetAttribute("vertsrc")]));
                }
                // materials:
                else if (xml.Name.Equals("material")) {}
                // prefabs: 
                else if (xml.Name.Equals("prefab")) {}
                // scenes
                else if (xml.Name.Equals("scene")) {}
                // other.... (custom xml asset?)
                else {}
            }
        }


        static void loadTextures() {
            foreach (var file in Directory.EnumerateFiles("data/", "*.png", SearchOption.AllDirectories)) {
                var fi = new FileInfo(file);
                textures[fi.Name] = Texture2D.fromFile(file);
            }
        }

        static void loadShaderSources() {
            
            string includes(string src) {
                var m = Regex.Match(src, "#include +\"(?<filename>[a-zA-Z._]+)\"");
                if (m.Success) {
                    var file = m.Groups["filename"].Value.Trim('\"');
                    src = src.Replace(m.Value, File.ReadAllText("data/shaders/" + file));

                    src = includes(src);
                }
                return src;
            }
            
            foreach (var file in Directory.EnumerateFiles("data/", "*.glsl", SearchOption.AllDirectories)) {
                
                //shaderSources.Add(file, )
            }
        }

        static void loadShaders() {
            
            if (!Directory.Exists("data/shaders/")) return;


            string includes(string src) {
                var m = Regex.Match(src, "#include ?(\".*?\")");
                if (m.Success) {
                    var file = m.Groups[1].Value.Trim('\"');
                    src = src.Replace(m.Value, File.ReadAllText("data/shaders/" + file));

                    src = includes(src);
                }
                return src;
            }

            
        
            // create shader programs:
            foreach (var dir in Directory.EnumerateDirectories("data/shaders")) {
                var fragsrc = File.ReadAllText(dir + "/frag.glsl");
                var vertsrc = File.ReadAllText(dir + "/vert.glsl");
                fragsrc = includes(fragsrc);
                vertsrc = includes(vertsrc);

                var dirinfo = new DirectoryInfo(dir);
                var shader = new Shader(fragsrc, vertsrc);
                System.Console.WriteLine("Shader Program " + shader.id + ": " + dirinfo.Name);
                shaders.Add(dirinfo.Name, shader);
            }
        }
    } 
}

