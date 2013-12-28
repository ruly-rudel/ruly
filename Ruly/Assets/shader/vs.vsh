precision highp float;
attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec2 aUv;
attribute vec3 aBlend;
uniform vec3 uLightDir;
uniform mat4 uPMatrix;
uniform mat4 uMBone[{0}];
uniform float uPow;
varying vec4 vTexCoord;
void main() {{
  float v;
  float spec;
  vec4 b1;
  vec4 b2;
  vec4 b;
  vec3 n1;
  vec3 n2;
  vec3 n;
  vec4 pos;
  mat4 m1;
  mat4 m2;

  pos = vec4(aPosition, 1.0);
  m1  = uMBone[int(aBlend.x)];
  m2  = uMBone[int(aBlend.y)];
  b1  = m1 * pos;
  b2  = m2 * pos;
  b   = mix(b2, b1, aBlend.z * 0.01);
  gl_Position = uPMatrix * b;

//  n = mat3(m1[0].xyz, m1[1].xyz, m1[2].xyz) * vec3(aNormal.x, aNormal.y, -aNormal.z);
  n = mat3(m1[0].x, m1[1].x, m1[2].x, m1[0].y, m1[1].y, m1[2].y, m1[0].z, m1[1].z, m1[2].z) * vec3(aNormal.x, aNormal.y, -aNormal.z);
  v = dot(n, uLightDir);
  spec = min(1.0, pow(max(v, 0.0), uPow));
  v = v * 0.5 + 0.5;
  vTexCoord   = vec4(aUv, v, spec);
}}
