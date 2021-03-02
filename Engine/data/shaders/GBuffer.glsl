

uniform sampler2D g_Albedo_Metallic;
uniform sampler2D g_Normal_Roughness;
uniform sampler2D g_Fragpos;

#ifdef MOUSE_PICKING
uniform sampler2D g_ObjectID;
#endif


struct GBufferData {
    vec3 albedo;
    float metallic, roughness;
    vec3 normal;
    vec3 fragpos;

#ifdef MOUSE_PICKING
    int objectID
#endif

};

void readGBuffer(in vec2 uv, out GBufferData gBuffer) {
    vec4 gam = texture(g_Albedo_Metallic, uv);
    vec4 gnr = texture(g_Normal_Roughness, uv);
    vec4 gf  = texture(g_Fragpos, uv); 

    gBuffer.albedo = gam.xyz;
    gBuffer.metallic = gam.w;
    gBuffer.normal = gnr.xyz;
    gBuffer.roughness = gnr.w;
    gBuffer.fragpos = gf.xyz;

#ifdef MOUSE_PICKING
    gBuffer.objectID = texture(g_ObjectID, uv);
#endif

}