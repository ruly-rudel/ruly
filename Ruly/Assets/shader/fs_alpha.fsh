precision mediump float;
varying vec4 vTexCoord;
uniform sampler2D sToon;
uniform sampler2D sTex;
uniform sampler2D sSph;
uniform sampler2D sSpa;
uniform vec4 uDif;
uniform vec4 uSpec;
void main() {
  vec4 toon;
  vec4 tex;
  vec4 sph;
  vec4 spa;
  vec4 spec;
  vec4 difamb;

  toon = texture2D(sToon, vec2(0.5, vTexCoord.z));
  tex  = texture2D(sTex,  vTexCoord.xy);
  sph  = texture2D(sSph,  vTexCoord.xy);
  spa  = texture2D(sSpa,  vTexCoord.xy);
  spec   = uSpec * vTexCoord.w;
  difamb = uDif  * toon;
  if(difamb.a == 0.0) {
  	discard;
  } else {
  	gl_FragColor = tex * sph * difamb + spa + spec;
  }
}

