
const float PI = 3.14159265359;


float DistributionGGX(in vec3 N, in vec3 H, in float roughness) {
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(in float NdotV, in float roughness) {
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(in vec3 N, in vec3 V, in vec3 L, in float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}
vec3 fresnelSchlick(in float cosTheta, in vec3 F0) {
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
} 


vec3 cookTorranceBRDF(in vec3 radiance, in vec3 F0, in vec3 N, in vec3 V, in vec3 L, in vec3 albedo, in float roughness, in float metallic) {
    
    vec3 H = normalize(V + L);

    float NDF = DistributionGGX(N, H, roughness);        
    float G   = GeometrySmith(N, V, L, roughness);      
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    vec3 specular     = numerator / max(denominator, 0.001);  
            
    float NdotL = max(dot(N, L), 0.0);                
    return (kD * albedo / PI + specular) * radiance * NdotL; 
}


vec3 CalcDirlight(in vec3 light_dir, in vec3 light_color, in vec3 F0, in vec3 N, in vec3 V, in vec3 albedo, in float roughness, in float metallic) {
    // calculate radiance
    vec3 L = normalize(light_dir);
    vec3 radiance = light_color;        
    return cookTorranceBRDF(radiance, F0, N, V, L, albedo, roughness, metallic);
}

vec3 CalcPointlight(in vec3 light_pos, in vec3 light_color, in vec3 fragpos, in vec3 F0, in vec3 N, in vec3 V, in vec3 albedo, in float roughness, in float metallic) {
    // calculate radiance
    vec3 L = normalize(light_pos - fragpos);
    float distance    = length(light_pos - fragpos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance     = light_color * attenuation;

    return cookTorranceBRDF(radiance, F0, N, V, L, albedo, roughness, metallic);
}


