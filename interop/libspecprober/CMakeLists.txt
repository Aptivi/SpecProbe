cmake_minimum_required(VERSION 3.25)
project(specprober
    VERSION     0.2.0
    DESCRIPTION "Native helper library for SpecProbe's managed world hardware parsing"
    LANGUAGES   C
)

add_library(specprober SHARED
    src/specprober.c
)
target_compile_features(specprober PUBLIC c_std_99)
target_include_directories(specprober PUBLIC include)
