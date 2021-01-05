using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace Engine {
    public interface IResourceProvider {
        IEnumerable<string> enumerate(string extension);
        string getText(string name);
        Bitmap getBitmap(string name);
    }

    public class FileResourceProvider : IResourceProvider {
        public IEnumerable<string> enumerate(string extension) => Directory.GetFiles(".", "*." + extension, SearchOption.AllDirectories);
        public Bitmap getBitmap(string name) => new Bitmap(name);
        public string getText(string name) => File.ReadAllText(name);
    }

    public class EmbeddedResourceProvider : IResourceProvider {
        System.Reflection.Assembly assembly;
        string[] resNames;

        public EmbeddedResourceProvider(System.Reflection.Assembly assembly) {
            this.assembly = assembly;
            resNames = assembly.GetManifestResourceNames();
        }

        public IEnumerable<string> enumerate(string extension) => resNames.Where(x => x.EndsWith("." + extension));

        public Bitmap getBitmap(string name) {
            using var stream = assembly.GetManifestResourceStream(name);
            return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
        }

        public string getText(string name) {
            using var s = assembly.GetManifestResourceStream(name);
            using var sr = new System.IO.StreamReader(s);
            return sr.ReadToEnd();
        }
    }
}