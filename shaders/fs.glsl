#version 330
 
// shader input
in vec2 uv;						// interpolated texture coordinates
in vec4 normal;					// interpolated normal
in vec4 worldPos;
uniform sampler2D pixels;		// texture sampler
uniform vec3 ambientColor;

// shader output
out vec4 outputColor;
uniform vec3 lightPos;

// fragment shader
void main()
{
    vec3 L = lightPos - worldPos.xyz;
    float dist = length(L);
    L = normalize(L);

    vec3 materialColor = texture(pixels, uv).xyz;

    vec3 lightColorDiff = vec3( 0.2f, 0.8f, 0.4f );
    float attenuation = 1.0f / (dist * dist);

    vec4 L4 = vec4( L.xyz, 1);
    vec4 Rv4 = L4 - 2 * dot(L4, normal) * normal;
    vec3 Rv = normalize(Rv4.xyz);
    
    vec3 lightColorSpec = lightColorDiff;

    outputColor = vec4( materialColor * (max( 0.0f, dot( L, normal.xyz ) ) * attenuation * lightColorDiff 
    + ambientColor
    + lightColorSpec * max( 0.0f, pow( dot(L, Rv), 0.8f ))), 1);

}