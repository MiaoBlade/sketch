shader_type canvas_item;
render_mode unshaded;

varying float idcolor;

void vertex(){
	uint packedPos=floatBitsToUint(INSTANCE_CUSTOM[VERTEX_ID]);
	vec2 pos=unpackHalf2x16(packedPos);

	VERTEX.x=pos.x;
	VERTEX.y=pos.y;
	idcolor=float(INSTANCE_ID-(INSTANCE_ID>>1<<1));
}
void fragment() {
	//COLOR=texture(TEXTURE,UV);
	COLOR=vec4(0.2,0.2,0.2,1.0);
	//COLOR.r=idcolor;
}
