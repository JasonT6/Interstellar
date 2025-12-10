uniform mat4 view;
uniform mat4 proj;
uniform float animation_seconds;
uniform bool is_moon;
uniform bool render_grid;

in vec3 sphere_fs_in;
in vec3 normal_fs_in;
in vec4 pos_fs_in;
in vec4 view_pos_fs_in;

out vec3 color;

uniform sampler2D galaxy_texture;

vec3 background_grid(vec3 dir) {
  // gradient
  float t = clamp(dir.y * 0.5 + 0.5, 0.0, 1.0);
  vec3 bot = vec3(0.01, 0.01, 0.02);
  vec3 mid = vec3(0.06, 0.09, 0.14);
  vec3 top = vec3(0.14, 0.18, 0.26);
  vec3 base = mix(bot, mix(mid, top, t * t), t);

  float lon = atan(dir.z, dir.x);
  float lat = asin(clamp(dir.y, -1.0, 1.0));
  vec2 uv = vec2(lon / (2.0 * M_PI), lat / M_PI) + 0.5;

  // grid
  float grid_density = 18.0;
  vec2 g = fract(uv * grid_density);
  float line_width = 0.01;
  float grid_line = step(g.x, line_width) + step(1.0 - g.x, line_width) + step(g.y, line_width) + step(1.0 - g.y, line_width);
  grid_line = clamp(grid_line, 0.0, 1.0);
  vec3 grid_color = grid_line * vec3(1.0, 1.0, 1.0);

  return base + grid_color;
}

vec3 background_texture(vec3 dir) {
  
  dir = normalize(dir);

  float lon = atan(dir.z, dir.x);
  float lat = asin(clamp(dir.y, -1.0, 1.0));

  vec2 uv;
  uv.x = lon / (2.0 * M_PI) + 0.5;
  uv.y = lat / M_PI + 0.5;
  uv.y = 1.0 - uv.y;

  vec3 color = texture(galaxy_texture, uv).rgb;

  return color;
}

vec3 add_pulsing_stars(vec3 dir) {
  
  float lon = atan(dir.z, dir.x);
  float lat = asin(clamp(dir.y, -1.0, 1.0));
  vec2 uv = vec2(lon / (2.0 * M_PI), lat / M_PI);

  vec2 star_uv = uv * 50.0;
  vec2 cell = floor(star_uv);
  vec2 rnd = random2(vec3(cell, 3.17));

  vec2 starCenter = cell + rnd;
  vec2 delta = star_uv - starCenter;

  float radius = 0.03;
  float d = length(delta);
  float star = smoothstep(radius, 0.0, d);

  float pulse = sin(animation_seconds * 2.5 + rnd.y * 40.0);
  pulse = smoothstep(-0.2, 1.0, pulse);
  float twinkle = mix(0.3, 1.2, pulse);

  return star * twinkle * vec3(1.8);
}

vec3 add_green_gradient(vec3 dir) {

  float t = clamp(dir.y * 0.5 + 0.5, 0.0, 1.0);

  vec3 low  = vec3(0.0, 0.25, 0.28);
  vec3 high = vec3(0.35, 0.85, 0.75);

  float pulse = 0.85 + 0.15 * sin(animation_seconds * 0.8);

  vec3 color = mix(low, high, t) * pulse;
  color = color / (1.0 + color);

  return color;
}


void main() {

  vec3 view_dir = normalize(view_pos_fs_in.xyz);
  vec3 forward = vec3(0.0, 0.0, -1.0);

  mat4 inv_view_full = inverse(view);
  vec3 cam_world = vec3(inv_view_full * vec4(0.0, 0.0, 0.0, 1.0));
  float dist = max(0.1, length(cam_world));

  // spherical coords
  float cos_theta = clamp(dot(view_dir, forward), -1.0, 1.0);
  float theta = acos(cos_theta);
  float phi = atan(view_dir.y, view_dir.x);
  
  // black hole parameters
  float hole_radius_world = 3.0;
  float hole_radius = atan(hole_radius_world / dist);

  // lens parameters
  float lens_radius_center = hole_radius;
  float lens_radius_width  = hole_radius * 1.45;

  // ring parameters
  float ring_radius = hole_radius * 1.025;
  float ring_width  = hole_radius * 0.25;

  // angular horizon cutoff
  if (theta < hole_radius * 0.8) {
    color = vec3(0.0);
    return;
  }

  // lens distortion effect
  float bend_region = smoothstep(lens_radius_center, lens_radius_center + lens_radius_width, theta);
  float bend_strength = (1.0 - bend_region);

  bend_strength *= bend_strength;
  bend_strength *= smoothstep(ring_radius * 1.8, ring_radius, theta);

  float max_deflect = radians(28.0);
  float warped_theta = theta + bend_strength * max_deflect;

  float max_swirl = radians(180.0);
  float swirl = bend_strength * max_swirl;
  float hemi = smoothstep(0.0, 0.4, abs(view_dir.y)) * sign(view_dir.y);
  float warped_phi = phi + swirl * hemi;

  vec3 warped_dir = normalize(vec3(sin(warped_theta) * cos(warped_phi), sin(warped_theta) * sin(warped_phi), -cos(warped_theta)));

  mat3 inv_rot = mat3(inv_view_full);
  vec3 warped_world = normalize(inv_rot * warped_dir);
  
  // background
  vec3 bg;

  if (render_grid) {
    bg = background_grid(warped_world);
  }
  else {
    bg = background_texture(warped_world);
    bg *= 10.0;
    bg += add_pulsing_stars(warped_world);
    bg += 0.125 * add_green_gradient(warped_world);
  }

  // ring
  // orthonormal tangent
  vec3 ortho = view_dir - forward * cos_theta;
  float ortho_len = length(ortho);

  if (ortho_len < 1e-4) {
    ortho = vec3(1.0, 0.0, 0.0);
    ortho_len = 1.0;
  }

  ortho /= ortho_len;

  float angle = atan(ortho.y, ortho.x);
  float spin = sin(angle * 2.0 + animation_seconds * 1);
  float ring = exp(-pow((theta - ring_radius) / (ring_width * 0.22), 2.0));
  ring *= 0.9 + 0.5 * spin;
  vec3 ring_color = vec3(1.9, 1.25, 0.7) * ring;

  // horizon fade
  float fade = smoothstep(hole_radius * 0.8, hole_radius, theta);
  color = (bg + ring_color) * fade;
  color = clamp(color, 0.0, 6.0);
}
