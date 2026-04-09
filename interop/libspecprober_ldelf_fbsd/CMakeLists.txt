cmake_minimum_required(VERSION 3.25)
project(specprober_ldelf_fbsd
    VERSION     0.2.0
    DESCRIPTION "ld-elf.so.1 wrapper for FreeBSD"
    LANGUAGES   C
)

add_library(specprober_ldelf_fbsd SHARED
    src/sp_le_wrapper.c
)
target_compile_features(specprober_ldelf_fbsd PUBLIC c_std_99)
target_include_directories(specprober_ldelf_fbsd PUBLIC include)
