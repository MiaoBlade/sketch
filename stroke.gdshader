shader_type canvas_item;
render_mode unshaded;

uniform float useDebug;

varying float idvalue;
varying vec4 idcolor;

const vec4 debug_key_color=vec4(1.0,0.2,0,1.0);
const vec4 debug_int_color=vec4(0.2,0.2,0.8,1.0);

void vertex(){
	uint packedPos=floatBitsToUint(INSTANCE_CUSTOM[VERTEX_ID]);
	vec2 pos=unpackHalf2x16(packedPos);
	VERTEX.x=pos.x;
	VERTEX.y=pos.y;
	
	idvalue=float(INSTANCE_ID-(INSTANCE_ID>>1<<1));
	vec4 dc=mix(debug_int_color,debug_key_color,COLOR.a);
	idcolor=mix(vec4(COLOR.rgb,1.0),dc,useDebug);
}
void fragment() {
	COLOR=idcolor;
	COLOR.a=1.0-idvalue*0.3*useDebug;
}
