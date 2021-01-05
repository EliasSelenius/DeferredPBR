using System;

using System.Collections.Generic;

namespace Demo {
    class Program {
        static void Main(string[] args) {
            
            Engine.IResourceProvider provider = new Engine.EmbeddedResourceProvider(typeof(Engine.app).Assembly);
            System.Console.WriteLine("EMBEDDED RESOURCES");
            foreach (var r in provider.enumerate("png")) {
                System.Console.WriteLine(r);
            }

            provider = new Engine.FileResourceProvider();
            System.Console.WriteLine("FILE SYSTEM RESOURCES");
            foreach (var r in provider.enumerate("glsl")) {
                System.Console.WriteLine(r);
            }

            
            //Engine.app.Main();
        }
    }
}
