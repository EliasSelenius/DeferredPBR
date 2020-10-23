using System.Collections.Generic;
using System.Xml;
using OpenTK.Graphics.OpenGL4;

class Scene {

    public static Scene active = new Scene();

    public Camera camera = new Camera();
    public readonly List<Entity> entities = new List<Entity>();
    public readonly List<Pointlight> pointlights = new List<Pointlight>();
    public readonly List<Dirlight> dirlights = new List<Dirlight>();

    public Scene() {
    
        var xml = new XmlDocument();
        xml.Load("data/models/Ships.dae");
        var co = (new Collada(xml).toEntity());
        var entity = new Entity();
        foreach (var item in co) {
            entity.addChild(item.Value);
        }

        entities.Add(entity);


        dirlights.Add(new Dirlight {
            color = (0.1f, 0.1f, 1)
        });

        dirlights.Add(new Dirlight {
            dir = new Nums.vec3(0,0,1)
        });

        dirlights.Add(new Dirlight {
            dir = new Nums.vec3(0,0,-1),
            color = (0,1,0)
        });
    
    }

    public void renderGeometry() {
        camera.updateUniforms();
        foreach (var e in entities) e.render();
    }

    public void renderLights() {
        foreach (var light in dirlights) {
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass.id, "lightDir"), light.dir.x, light.dir.y, light.dir.z);
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass.id, "lightColor"), light.color.x, light.color.y, light.color.z);
            GL.BindVertexArray(Renderer.quadVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        foreach (var light in pointlights) {
            
        }
    }

    public void update() {
        camera.move();
    }
}