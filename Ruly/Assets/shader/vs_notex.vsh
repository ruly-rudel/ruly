precision mediump float;
attribute vec3 aPosition;
uniform mat4 uPMatrix;
void main() {
  vec4 pos;

  pos = vec4(aPosition.xyz, 1.0);
  gl_Position = uPMatrix * pos;
}
