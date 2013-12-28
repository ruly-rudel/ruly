precision highp float;
attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec2 aUv;
uniform vec3 uLightDir;
uniform mat4 uPMatrix;
uniform float uPow;
varying vec4 vTexCoord;
void main() {
  float v;
  float spec;
  vec4 pos;
  vec3 n;

  pos = vec4(aPosition.xyz, 1.0);
  gl_Position = uPMatrix * pos;

  n = vec3(aNormal.xy, -aNormal.z);
  v = dot(n, uLightDir);
  v = v * 0.5 + 0.5;
  spec = min(1.0, pow(max(v, 0.0), uPow));
  vTexCoord = vec4(aUv, v, spec);
}
