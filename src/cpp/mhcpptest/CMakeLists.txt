cmake_minimum_required(VERSION 2.8)

## section: Macro
MACRO(ADD_MSVC_PRECOMPILED_HEADER PrecompiledHeader PrecompiledSource SourcesVar)
  IF(MSVC)
    GET_FILENAME_COMPONENT(PrecompiledBasename ${PrecompiledHeader} NAME_WE)
    SET(PrecompiledBinary "${CMAKE_CURRENT_BINARY_DIR}/${PrecompiledBasename}.pch")
    SET(Sources ${${SourcesVar}})

    SET_SOURCE_FILES_PROPERTIES(${PrecompiledSource}
                                PROPERTIES COMPILE_FLAGS "/Yc\"${PrecompiledHeader}\" /Fp\"${PrecompiledBinary}\""
                                           OBJECT_OUTPUTS "${PrecompiledBinary}")
    SET_SOURCE_FILES_PROPERTIES(${Sources}
                                PROPERTIES COMPILE_FLAGS "/Yu\"${PrecompiledBinary}\" /FI\"${PrecompiledBinary}\" /Fp\"${PrecompiledBinary}\""
                                           OBJECT_DEPENDS "${PrecompiledBinary}")  
    # Add precompiled header to SourcesVar
    LIST(APPEND ${SourcesVar} ${PrecompiledSource})
  ENDIF(MSVC)
ENDMACRO(ADD_MSVC_PRECOMPILED_HEADER)

## start setting
SET (this_target mhcpptest)
PROJECT(${this_target})



## section: include directory
INCLUDE_DIRECTORIES(
  ../mhcpp
)

## section: source files
# Add your source files here (one file per line), please SORT in alphabetical order for future maintenance
SET (${this_target}_SOURCE_FILES
    main.cpp
)

## section: header files
# Add your header files here(one file per line), please SORT in alphabetical order for future maintenance!
SET(${this_target}_HEADER_FILES
    )

SOURCE_GROUP("Header Files" FILES 
	
	)
SOURCE_GROUP("Source Files" FILES 
	
	)
SOURCE_GROUP("Resource Files" FILES 
	
	)


## section: precompiled header	
#ADD_MSVC_PRECOMPILED_HEADER("precompiled.h" "precompiled.cpp" MySources)
#ADD_LIBRARY(MyLibrary ${MySources})

SET_SOURCE_FILES_PROPERTIES(${this_target}_HEADER_FILES
                            PROPERTIES HEADER_FILE_ONLY TRUE)
LIST(APPEND ${this_target}_SOURCE_FILES ${${this_target}_HEADER_FILES})

## section: add definitions
#   add prefix -D. example> -DSHP
#  - DO NOT add  the following definitions(already defined in ${OSP_DEFINITIONS}:
#   -DSHP, -DWIN32, -D_WINDOWS, -D_DEBUG, -D_USRDLL, -D_CRT_SECURE_NO_DEPRECATE
ADD_DEFINITIONS(
    # -DCMAKE_CXX_FLAGS:STRING=' -std=c++11 '    
    )

# JM needed to use this as of 2015-02, after an upgrade to latest Debian setup. The Add_definitions macro was not working, somehow.
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

## section: add target

ADD_EXECUTABLE(${this_target} ${${this_target}_SOURCE_FILES} )

## section: add dependency
# dependency determines overall build order.
## per202 comments out the following - no such thing for Linux.
#ADD_DEPENDENCIES(${this_target} 
#    netcdf.lib
#	lib_json.lib
#	datatypes.lib
#	)
        
## per202 comments out the following - no such thing for Linux.
## section: add dependency
# dependency determines overall build order.
# ADD_DEPENDENCIES(${this_target} 
#    netcdf.lib
# 	)


######### 2015-06 Trying to rely on shared location with LD_LIBRARY_PATH or lrelated to locate these.
######### It is otherwise problematic to deploy on platforms where you do not have root (sudo) access, since you cannot use ldconfig
#find_library (JSONCPP_LIBRARY_PATH libjsoncpp.so PATHS ../Externals/jsoncpp NO_CMAKE_PATH NO_CMAKE_ENVIRONMENT_PATH NO_SYSTEM_ENVIRONMENT_PATH NO_CMAKE_SYSTEM_PATH )
#message ("The path to the jsoncpp library is ${JSONCPP_LIBRARY_PATH}")
#
#find_library (YAMLCPP_LIBRARY_PATH libyaml-cpp.so PATHS ../Externals/yaml-cpp NO_CMAKE_PATH NO_CMAKE_ENVIRONMENT_PATH NO_SYSTEM_ENVIRONMENT_PATH NO_CMAKE_SYSTEM_PATH )
#message ("The path to the yaml-cpp library is ${YAMLCPP_LIBRARY_PATH}")
#
#find_library (DATATYPES_LIBRARY_PATH libdatatypes.so PATHS ../Externals/datatypes/datatypes NO_CMAKE_PATH NO_CMAKE_ENVIRONMENT_PATH NO_SYSTEM_ENVIRONMENT_PATH NO_CMAKE_SYSTEM_PATH )
#message ("The path to the datatypes library is ${DATATYPES_LIBRARY_PATH}")
#
## Your-external "mylib"
#add_library( datatypes SHARED IMPORTED )
#add_library( jsoncpp SHARED IMPORTED )
#add_library( yaml-cpp SHARED IMPORTED )
## You can define two import-locations: one for debug and one for release.
#set_target_properties( datatypes PROPERTIES IMPORTED_LOCATION ${DATATYPES_LIBRARY_PATH} )
#set_target_properties( jsoncpp PROPERTIES IMPORTED_LOCATION ${JSONCPP_LIBRARY_PATH} )
#set_target_properties( yaml-cpp PROPERTIES IMPORTED_LOCATION ${YAMLCPP_LIBRARY_PATH} )
######### 

#ADD_DEFINITIONS(
#     -DL${DATATYPES_LIBRARY_PATH}
#    )


## section: set link libraries
TARGET_LINK_LIBRARIES( ${this_target}
        )
