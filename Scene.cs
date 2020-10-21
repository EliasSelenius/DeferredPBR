using System.Collections.Generic;
using System.Xml;

class Scene {

    public static Scene active = new Scene();

    Camera camera = new Camera();
    List<Entity> entities = new List<Entity>();
    List<Pointlight> pointlights = new List<Pointlight>();
    List<Dirlight> dirlights = new List<Dirlight>();

    public Scene() {
    
        var xml = new XmlDocument();
        xml.Load("data/models/Ships.dae");
        var co = (new Collada(xml).toEntity());
        var entity = new Entity();
        foreach (var item in co) {
            entity.addChild(item.Value);
        }

        entities.Add(entity);
    
    }

    public void renderGeometry() {
        camera.updateUniforms();
        foreach (var e in entities) e.render();
    }

    public void renderLights() {

    }

    public void update() {
        camera.move();
    }
}