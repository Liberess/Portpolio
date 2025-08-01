#include "ColorModel.h"

using namespace Liberess;

void ColorModel::RGBtoCMYK(float r, float g, float b, uint8& c, uint8& m, uint8& y, uint8& k)
{
	float C, M, Y, K;

	r = 1.0 - (r / 255.0);
	g = 1.0 - (g / 255.0);
	b = 1.0 - (b / 255.0);
 
	if (r < g)
		K = r;
	else
		K = g;
 
	if (b < K)
		K = b;
 
	C = (r - K)/(1.0 - K);
	M = (g - K)/(1.0 - K);
	Y = (b - K)/(1.0 - K);
 
	C = (C * 100) + 0.5;
	M = (M * 100) + 0.5;
	Y = (Y * 100) + 0.5;
	K = (K * 100) + 0.5;
 
	c = static_cast<uint8>(C);
	m = static_cast<uint8>(M);
	y = static_cast<uint8>(Y);
	k = static_cast<uint8>(K);
}

COLORREF ColorModel::CMYKtoRGB(uint8 c, uint8 m, uint8 y, uint8 k)
{
	uint8 r, g, b;

	float R, G, B;
	float C, M, Y, K;
 
	C = static_cast<float>(c);
	M = static_cast<float>(m);
	Y = static_cast<float>(y);
	K = static_cast<float>(k);
 
	C = C / 255.0;
	M = M / 255.0;
	Y = Y / 255.0;
	K = K / 255.0;
	
	R = C * (1.0 - K) + K;
	G = M * (1.0 - K) + K;
	B = Y * (1.0 - K) + K;
 
	R = (1.0 - R) * 255.0 + 0.5;
	G = (1.0 - G) * 255.0 + 0.5;
	B = (1.0 - B) * 255.0 + 0.5;
 
	r = static_cast<uint8>(R);
	g = static_cast<uint8>(G);
	b = static_cast<uint8>(B);

	return RGB(r,g,b);
}
