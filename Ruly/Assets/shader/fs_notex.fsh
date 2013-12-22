precision mediump float;
varying vec2 vTexCoord;
uniform sampler2D sTex;
uniform vec4 uDif;
void main() {
  vec4 tex;

  tex  = texture2D(sTex,  vTexCoord);
  gl_FragColor = uDif * tex;
}
