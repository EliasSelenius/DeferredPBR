
layout (std140) uniform Application {
    vec4 size_time_deltatime;
} application;

vec2 getWindowSize() { return application.size_time_deltatime.xy; }
float getTime() { return application.size_time_deltatime.z; }
float getDeltatime() { return application.size_time_deltatime.w; }