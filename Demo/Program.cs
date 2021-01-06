using System;

using System.Collections.Generic;

namespace Demo {
    class Program {
        static void Main(string[] args) {

            /*        
            Engine.IResourceProvider provider = new Engine.EmbeddedResourceProvider(typeof(Engine.app).Assembly);
            
            System.Console.WriteLine(provider.getText("Engine.data.testAssets.xml"));

            provider = new Engine.FileResourceProvider();
            
            System.Console.WriteLine(provider.getText("Demo.data.shaders.test.frag.glsl"));
            */
            
            Engine.app.Main();
        }
    }
}
