shader_type canvas_item;
render_mode unshaded;

varying float idvalue;
varying vec4 idcolor;

void vertex(){
	uint packedPos=floatBitsToUint(INSTANCE_CUSTOM[VERTEX_ID]);
	vec2 pos=unpackHalf2x16(packedPos);

	VERTEX.x=pos.x;
	VERTEX.y=pos.y;
	//COLOR=vec4(1.0,0.2,0.2,1.0);
	//idcolor=float(INSTANCE_ID-(INSTANCE_ID>>1<<1));
	idcolor=COLOR;
	
}
void fragment() {
	//COLOR=vec4(idcolor,0.2,0.2,1.0);
	COLOR=idcolor;
}
