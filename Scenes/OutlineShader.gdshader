shader_type canvas_item;

uniform vec4 outline_color : source_color;

void vertex() {
	
}

bool HighlightCheck(vec4 pixel, sampler2D tex, vec2 position){
	float y = 1.0/float(textureSize(tex,0).y);
	float x = 1.0/float(textureSize(tex,0).x);
	int count = 0;
	if(pixel.a != 0.0) return false;
	if(texture(tex, position + vec2(x,0)).a !=0.0) count ++;
	if(texture(tex, position + vec2(0,y)).a !=0.0) count ++;
	if(texture(tex, position + vec2(-x,0)).a !=0.0) count ++;
	if(texture(tex, position + vec2(0,-y)).a !=0.0) count ++;
	return count != 0;
}



vec4 Replace(vec4 pixel, bool highlight){
	if(highlight){
		return outline_color;
	}
	else return pixel;
}

void fragment() {
	vec4 sprite = texture(TEXTURE,UV);
	COLOR = Replace(sprite, HighlightCheck(sprite, TEXTURE,UV));
}




