precision mediump float;
attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec2 aUv;
uniform vec3 uLightDir;
uniform mat4 uPMatrix;
varying vec3 vTexCoord;
void main() {
  float v;
  vec4 pos;
  vec3 n;

  pos = vec4(aPosition.xyz, 1.0);
  gl_Position = uPMatrix * pos;

  n = vec3(aNormal.xy, -aNormal.z);
  v = dot(n, uLightDir);
  v = v * 0.5 + 0.5;
  vTexCoord = vec3(aUv, v);
}
