shader_type canvas_item;

uniform vec4 interior_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 border_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float border_thickness : hint_range(0.0, 0.5) = 0.05;

void fragment() {
    // Center the UV coordinates so (0,0) is the middle of the control
    vec2 uv = UV - vec2(0.5);
    
    // Calculate the distance from the center
    float dist = length(uv);
    
    // Smooth anti-aliasing factor to keep edges perfectly crisp
    float af = fwidth(dist) * 0.5;
    
    // Draw the outer edge of the entire circle
    float circle = smoothstep(0.5, 0.5 - af, dist);
    
    // Draw the inner edge where the borde ends and the interior begins
    float border_edge = 0.5 - 2 * border_thickness / node_size.x;
    float interior = smoothstep(border_edge, border_edge - af, dist);
    
    // Mix the colors based on the distances
    vec4 final_color = mix(border_color, interior_color, interior);
    
    // Set the transparency outside the circle
    final_color.a *= circle;
    
    COLOR = final_color;
}