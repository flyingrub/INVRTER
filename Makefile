all:
	g++ `pkg-config --cflags --libs opencv` hello.cpp && ./a.out