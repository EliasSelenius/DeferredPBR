using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Drawing;
using System.Linq;
using System.Collections.ObjectModel;
using System;

namespace Engine {
    public static class Assets {

        public static Dictionary<string, Shader> shaders = new();
        static Dictionary<string, string> shaderSources = new();

        public static Dictionary<string, Texture2D> textures = new();
        public static Dictionary<string, PBRMaterial> materials = new();

        public static Dictionary<string, Prefab> prefabs = new();

        public static Dictionary<string, Gui.Font> fonts = new();

        static List<IResourceProvider> providers = new List<IResourceProvider>();


        public static void load(IResourceProvider provider) {

            providers.Add(provider);

            { // shader source files 
                foreach (var res in provider.enumerate("glsl")) {
                    shaderSources.Add(res, provider.getText(res));
                }

                // process include directives
                var rgx = new Regex("#include +\"(?<filename>[a-zA-Z._]+)\"");
                foreach (var source in shaderSources) {

                    var matches = rgx.Matches(source.Value);
                    for (int m = 0; m < matches.Count; m++) {
                        var match = matches[m];
                        shaderSources[source.Key] = shaderSources[source.Key].Replace(match.Value, shaderSources[match.Groups["filename"].Value]);
                    }
                }
            }

            { // textures
                foreach (var res in provider.enumerate("png")) {
                    textures[res] = new Texture2D(WrapMode.Repeat, Filter.Nearest, Utils.bitmapToColorArray(provider.getBitmap(res)));
                }
            }

            { // fonts
                foreach (var res in provider.enumerate("fnt")) {
                    var i = res.LastIndexOf(".");
                    var atlasName = res.Substring(0, i) + ".png";
                    fonts.Add(res, new Gui.Font(provider.getText(res), getTexture2D(atlasName)));
                }
            }

            { // collada
                // TODO: load materials aswell
                foreach (var res in provider.enumerate("dae")) {
                    var doc = new XmlDocument();
                    doc.LoadXml(provider.getText(res));
                    var collada = new Collada(doc);
                    var prefs = collada.toPrefabs();
                    foreach (var p in prefs) {
                        var prefabName = res.Substring(0, res.LastIndexOf('.')) + "." + p.Key;
                        prefabs.Add(prefabName, p.Value);
                    }
                }
            }

            { // xml assets
                foreach (var res in provider.enumerate("xml")) {
                    var doc = new XmlDocument();
                    doc.LoadXml(provider.getText(res));
                    loadFromXml(doc);
                }
            }
        }


        public static Shader getShader(string name) => shaders[name];
        public static Texture2D getTexture2D(string name) => textures[name];
        public static PBRMaterial getMaterial(string name) => materials[name];
        public static Gui.Font getFont(string name) => fonts[name];
        public static Prefab getPrefab(string name) => prefabs[name];

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
    } 
}

