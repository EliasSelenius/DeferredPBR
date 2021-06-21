using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Drawing;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using Engine.Gui;
using Nums;
using Engine.Toolset;



namespace Engine {
    public static class Assets {

        public static Dictionary<string, Shader> shaders = new();
        public static Dictionary<string, string> shaderSources = new();

        public static Dictionary<string, Texture2D> textures = new();
        public static Dictionary<string, PBRMaterial> materials = new();

        public static Dictionary<string, Prefab> prefabs = new();

        public static Dictionary<string, Gui.Font> fonts = new();

        public static Dictionary<string, Mesh<Vertex>> meshes = new();

        public static Dictionary<string, Collada> colladaFiles = new();

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
                    textures[res] = new Texture2D(WrapMode.Repeat, Filter.Linear, Utils.bitmapToColorArray(provider.getBitmap(res)));
                }
            }

            { // fonts
                foreach (var res in provider.enumerate("fnt")) {
                    var i = res.LastIndexOf(".");
                    var atlasName = res.Substring(0, i) + ".png";
                    fonts.Add(res, new Gui.Font(res, provider.getText(res), getTexture2D(atlasName)));
                }
            }

            { // collada
                foreach (var res in provider.enumerate("dae")) {
                    var doc = new XmlDocument();
                    doc.LoadXml(provider.getText(res));
                    var collada = new Collada(doc);
                    colladaFiles.Add(res, collada);

                    var assetname = res.Substring(0, res.LastIndexOf('.')) + ".";

                    // prefabs
                    foreach (var p in collada.prefabs) {
                        prefabs.Add(assetname + p.Key, p.Value);
                    }

                    // meshes
                    foreach (var m in collada.meshes) {
                        meshes.Add(assetname + m.Key, m.Value.mesh);
                    }

                    // materials
                    foreach (var m in collada.materials) {
                        materials.Add(assetname + m.Key, m.Value);
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
        public static Mesh<Vertex> getMesh(string name) => meshes[name];


        static void loadFromXml(XmlDocument doc) {
            foreach (var elm in doc.DocumentElement.ChildNodes) {
                var xml = elm as XmlElement;

                if (xml is null) continue; // skip anything that isnt an xmlElement (like a comment..)

                var assetName = xml.GetAttribute("name");


                // shaders:
                if (xml.Name.Equals("shader")) {
                    shaders.Add(assetName, new Shader(assetName, shaderSources[xml.GetAttribute("fragsrc")], shaderSources[xml.GetAttribute("vertsrc")]));
                }
                // materials:
                else if (xml.Name.Equals("material")) {
                    var mat = new PBRMaterial();
                    var a = xml.GetElementsByTagName("albedo").Item(0).InnerText.Split(' ').Select(x => float.Parse(x) / 255f).ToArray();
                    mat.albedo = new Nums.vec3(a[0], a[1], a[2]);

                    mat.metallic = float.Parse(xml.GetElementsByTagName("metallic").Item(0).InnerText);
                    mat.roughness = float.Parse(xml.GetElementsByTagName("roughness").Item(0).InnerText);

                    materials.Add(assetName, mat);
                }
                // prefabs: 
                else if (xml.Name.Equals("prefab")) {}
                // scenes
                else if (xml.Name.Equals("scene")) {}
                // other.... (custom xml asset?)
                else {}
            }
        }


#region gui rendering


/*

    assets to draw in GUI
        - shaders
            - glsl source files
        - textures
            - settings (genMipmap, filter)
        - materials
        - prefabs
        - fonts
        - meshes



*/

        static (string title, Action<Textflow> draw)[] pages = {
            ("Prefabs", tf => {
                foreach (var p in prefabs) {
                    tf.writeline(p.Key);
                }
            }),
            ("Materials", tf => {
                foreach (var p in materials) {
                    tf.writeline(p.Key);
                }
            }),
            ("Meshes", tf => {
                foreach (var p in meshes) {
                    tf.writeline(p.Key);
                }
            }),
            ("Shaders", tf => {
                foreach (var p in shaders) {
                    tf.writeline(p.Key);
                }
            }),
            ("Textures", tf => {
                foreach (var p in textures) {
                    tf.writeline(p.Key);
                }
            })
        };

        static int page = 0;

        internal static void drawGui(Canvas canvas) {
            var tf = new Textflow(canvas) {
                font = getFont("Engine.data.fonts.monospaced.fnt"),  // Gui.Font.arial,
                fontsize = 16,
                textcolor = Editor.theme.textColor,
                pos = (10, 40)
            };


            canvas.text(3, Gui.Font.arial, 32, pages[page].title, Editor.theme.textColor);
            pages[page].draw(tf);



            // background
            var backgroundSize = new vec2(canvas.width / 3.5f, canvas.height - 6); 
            canvas.rect(3, backgroundSize, Editor.theme.backgroundColor);
            // border
            canvas.rectborder(3, backgroundSize, 3, Editor.theme.borderColor);

        }

#endregion

    }


    class Textflow {
        public enum Align {
            left, center, right
        }
        
        public Canvas canvas;

        public Gui.Font font;
        public int fontsize;
        public vec2 pos;
        public color textcolor;
        public Align align;


        public Textflow(Canvas canvas) {
            this.canvas = canvas;
        }

        public void writeline(string text) {
            canvas.text(pos, font, fontsize, text, in textcolor);
            pos.y += fontsize;
        }
    }


/*
    public abstract class Asset {
        public string name { get; }
        public bool isReadonly { get; }

        public Asset(string name) {

        }


    }

    class MaterialAsset : Asset {
        public MaterialAsset()

    }
    */
}

