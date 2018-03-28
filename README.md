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
