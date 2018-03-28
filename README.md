# INVRTER : School project
The goal of this project is to be able to use the camera on an htc vive 
to control the Virtual Reality thanks to our hand.

## Unity :
In order to correctly bind opencvsharp to your local install of opencv 
run the following commands :
```
git clone https://github.com/shimat/opencvsharp.git 
```
#### Make
Type the following code in a console: 
```
cd opencvsharp/src
cmake .
make
```
When successfully compiled, OpenCvSharpExtern/libOpenCvSharpExtern.so is 
generated. Replace the one in Assets/ by it. 
