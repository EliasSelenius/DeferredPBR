using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System;

namespace Engine {
    public interface IResourceProvider {
        /*
            Note:
                the resource identifier is always in this format:
                providerName(.folder*).resource.extension    
        
        */
        string providerName { get; }
        IEnumerable<string> enumerate(string extension);
        string getText(string resid);
        Bitmap getBitmap(string resid);
    }

    public class FileResourceProvider : IResourceProvider {
        
        public string providerName { get; private set; }

        string directory;

        public FileResourceProvider(string directory = ".") {
            this.directory = Path.GetFullPath(directory);
            providerName = (new DirectoryInfo(directory)).Name;
        }

        private string path2resid(string path) {
            return providerName + "." + Path.GetRelativePath(directory, path).Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
        }
        private string resid2path(string resid) {
            var i = resid.LastIndexOf('.');
            var ext = resid.Substring(i);
            var res = resid.Substring(providerName.Length, i - providerName.Length);
            res = "." + res.Replace('.', Path.DirectorySeparatorChar) + ext;
            return res;
        }

        public IEnumerable<string> enumerate(string extension) => Directory.GetFiles(directory, "*." + extension, SearchOption.AllDirectories).Select(x=>path2resid(x));
        public Bitmap getBitmap(string resid) => new Bitmap(Path.Combine(directory, resid2path(resid)));
        public string getText(string resid) => File.ReadAllText(Path.Combine(directory, resid2path(resid)));
    }

    public class EmbeddedResourceProvider : IResourceProvider {

        public string providerName => assembly.GetName().Name;

        System.Reflection.Assembly assembly;
        string[] resNames;

        public EmbeddedResourceProvider(System.Reflection.Assembly assembly) {
            this.assembly = assembly;
            resNames = assembly.GetManifestResourceNames();
        }

        public IEnumerable<string> enumerate(string extension) => resNames.Where(x => x.EndsWith("." + extension));

        public Bitmap getBitmap(string resid) {
            using var stream = assembly.GetManifestResourceStream(resid);
            return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
        }

        public string getText(string resid) {
            using var s = assembly.GetManifestResourceStream(resid);
            using var sr = new System.IO.StreamReader(s);
            return sr.ReadToEnd();
        }
    }
}