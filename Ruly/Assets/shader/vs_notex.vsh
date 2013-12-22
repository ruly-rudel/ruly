precision mediump float;
attribute vec3 aPosition;
attribute vec2 aUv;
uniform mat4 uPMatrix;
varying vec2 vTexCoord;
void main() {
  vec4 pos;

  pos = vec4(aPosition.xyz, 1.0);
  gl_Position = uPMatrix * pos;
  vTexCoord = aUv;
}
