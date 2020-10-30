using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

static class Assets {

    static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

    public static void load() {
        loadShaders();
    }

    public static Shader getShader(string name) => shaders[name];

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
            shaders.Add(dirinfo.Name, new Shader(fragsrc, vertsrc));
        }
    }
} 