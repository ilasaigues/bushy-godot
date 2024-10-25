shader_type canvas_item;
uniform sampler2D pallete : filter_nearest;
uniform vec4 default_light : source_color;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

vec4 overlay(vec4 base, vec4 blend){
	vec4 limit = step(0.5, base);
	return mix(2.0 * base * blend, 1.0 - 2.0 * (1.0 - base) * (1.0 - blend), limit);
}

vec4 replace(vec4 color_tex)
{
	float step_y = 1.0/float(textureSize(pallete,0).y);
	for(int i = 0; i <= textureSize(pallete,0).y; i++)
	{
		vec4 current_input = texture(pallete, vec2(0, step_y * float(i)));
		vec4 current_output = texture(pallete, vec2(1, step_y * float(i)));
		if(distance(color_tex,current_input) == 0.0)
		{
			return current_output;
		}
	}
	return overlay(color_tex, default_light);
}


void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 sprite = texture(TEXTURE,UV);
	vec4 screen = texture(SCREEN_TEXTURE, SCREEN_UV);
	vec3 result = replace(screen).rgb;
	COLOR = vec4(result, sprite.a);
}
