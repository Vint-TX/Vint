#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

out vec3 Normal;
out vec3 FragPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    vec4 position = vec4(aPosition, 1);

    gl_Position = position * model * view * projection;
    FragPos = vec3(model * position);
    Normal = aNormal * mat3(transpose(inverse(model)));
}
