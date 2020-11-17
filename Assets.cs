using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

static class Assets {

    static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
    static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    public static void load() {
        loadShaders();
        loadTextures();
    }

    public static Shader getShader(string name) => shaders[name];
    public static Texture2D getTexture2D(string name) => textures[name];

    static void loadTextures() {
        foreach (var file in Directory.EnumerateFiles("data/", "*.png", SearchOption.AllDirectories)) {
            var fi = new FileInfo(file);
            textures[fi.Name] = Texture2D.fromFile(file);
        }
    }

    static void loadShaders() {
        // load files:
        /*var srcs = new Dictionary<string, string>();
        foreach (var file in Directory.EnumerateFiles("data/shaders", "*.glsl", SearchOption.TopDirectoryOnly)) {
            srcs.Add(file, File.ReadAllText(file));
        }*/

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