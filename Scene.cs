using System.Collections.Generic;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using Nums;


class Scene {

    public static Scene active = new Scene();

    public Skybox skybox;
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
        entity.transform.position.y = - 60;
        entities.Add(entity);

        { // lights

            /*
            dirlights.Add(new Dirlight {
                dir = new Nums.vec3(1,1,1).normalized(),
                color = Nums.vec3.one * 1f
            });

            dirlights.Add(new Dirlight {
                color = (0.2f, 0.2f, 1)
            });

            dirlights.Add(new Dirlight {
                dir = new Nums.vec3(0,0,-1),
                color = (0,1,0)
            });

            dirlights.Add(new Dirlight {
                dir = new Nums.vec3(-1,1,-1).normalized(),
                color = (1,0.2f,0.2f)
            });*/

            pointlights.Add(new Pointlight {
                position = (3, 3, 3),
                color = (10, 10, 10)
            });

/*
            pointlights.Add(new Pointlight {
                position = (6, 6, 3),
                color = (3, 30, 3)
            });*/
        }

        { // textured plane
            var m = MeshFactory<Vertex>.genPlane(10, 10f);
            var e = new Entity {
                renderer = new MeshRenderer {
                    mesh = m,
                    materials = new[] { new PBRMaterial {
                        //albedoMap = Assets.getTexture2D("test.png"),
                        albedo = 1f,
                        roughness = 0.5f,
                        metallic = 1f
                } }
                }
            };
            e.transform.position.z += 20;
            e.transform.scale.xz *= 2000;
            entities.Add(e);
        }

        { // material spectrum
            var mesh = MeshFactory<Vertex>.genSphere(100, 1f);

            mesh.mutate((v, i) => {
                v.position += v.position * Nums.math.range(0, 0.01f);
                return v;
            });
            mesh.genNormals();
            //mesh.bufferdata();

            for (float r = 0; r <= 1f; r += 0.1f) {
                for (float m = 0; m <= 1f; m += 0.1f) {
                    var mat = new PBRMaterial {
                        albedo = Nums.vec3.one,
                        roughness = r,
                        metallic = m
                    };
                    var e = new Entity {
                        renderer = new MeshRenderer {
                            mesh = mesh,
                            materials = new[] { mat }
                        }
                    };
                    e.transform.position.xy = new Nums.vec2(r, m) * 25;
                    entities.Add(e);
                }
            }
        }

    
        { // planet
            var mesh = MeshFactory<Vertex>.genSphere(100, 1f);
            mesh.mutate((v, i) => {
                var p = v.position;
                v.position *= 100f;

                for (int o = 1; o <= 4; o++) {
                    v.position += p * Nums.math.gradnoise(new Nums.vec3(o,o,o) * 10f + p * 5f * o) * 10f / o;
                }

                return v;
            });
            //mesh.flipIndices();
            mesh.genNormals();
            mesh.bufferdata();


            var planet = new Entity {
                renderer = new MeshRenderer {
                    mesh = mesh,
                    materials = new[] { 
                        new PBRMaterial { 
                            roughness = 0.5f,
                            metallic = 1f,
                            albedo = 1f
                        }
                    }
                }
            };
            planet.transform.position = (0, 0, 340);

            entities.Add(planet);
        }

    }

    public void renderGeometry() {
        camera.updateUniforms();
        foreach (var e in entities) e.render();
    }

    public void renderLights() {
        Renderer.lightPass_dirlight.use();
        foreach (var light in dirlights) {
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightDir"), light.dir.x, light.dir.y, light.dir.z);
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
            Lights.dirlightMesh.render();
        }
        

        Renderer.lightPass_pointlight.use();
        foreach (var light in pointlights) {

            vec3 v = (camera.viewMatrix.transpose * new vec4(light.position.x, light.position.y, light.position.z, 1.0f)).xyz;
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightPosition"), 1, ref v.x);
            GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
            var m = light.calcModelMatrix();
            GLUtils.setUniformMatrix4(Renderer.lightPass_pointlight.id, "model", ref m);
            Lights.pointlightMesh.render();
        }
    }

    public void update() {
        camera.move();
        
        //pointlights[0].position = camera.transform.position + camera.transform.forward;
        entities[0].children[3].transform.rotate(Nums.vec3.unity, 0.01f);
    }
}