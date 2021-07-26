#ifndef NOISE_UTILS
#define NOISE_UTILS

// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

// Value    Noise 2D, Derivatives: https://www.shadertoy.com/view/4dXBRH
// Gradient Noise 2D, Derivatives: https://www.shadertoy.com/view/XdXBRH
// Value    Noise 3D, Derivatives: https://www.shadertoy.com/view/XsXfRH
// Gradient Noise 3D, Derivatives: https://www.shadertoy.com/view/4dffRH
// Value    Noise 2D             : https://www.shadertoy.com/view/lsf3WH
// Value    Noise 3D             : https://www.shadertoy.com/view/4sfGzS
// Gradient Noise 2D             : https://www.shadertoy.com/view/XdXGW8
// Gradient Noise 3D             : https://www.shadertoy.com/view/Xsl3Dl
// Simplex  Noise 2D             : https://www.shadertoy.com/view/Msf3WH



//========================================================================================================================
//	Gradient2D
//========================================================================================================================
// Gradient Noise (http://en.wikipedia.org/wiki/Gradient_noise), not to be confused with
// Value Noise, and neither with Perlin's Noise (which is one form of Gradient Noise)
// is probably the most convenient way to generate noise (a random smooth signal with 
// mostly all its energy in the low frequencies) suitable for procedural texturing/shading,
// modeling and animation.
// It produces smoother and higher quality than Value Noise, but it's of course slighty more
// expensive.


float2 gradienthash2D(float2 x)
{
	const float2 k = float2(0.3183099, 0.3678794);
	x = x * k + k.yx;
	return -1.0 + 2.0*frac(16.0 * k*frac(x.x*x.y*(x.x + x.y)));
}


float gradientnoise2D(in float2 p)
{
	float2 i = floor(p);
	float2 f = frac(p);

	float2 u = f * f*(3.0 - 2.0*f);

	return lerp(lerp(dot(gradienthash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
		dot(gradienthash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
		lerp(dot(gradienthash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
			dot(gradienthash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}

float3 ridgegradientnoise2D(float2 p)
{
	return 1.0 - abs(gradientnoise2D(p));
}

float gradientfbm2D(float2 p, int octaves, float freq, float lacunarity, float gain, float amp)
{
	float sum = 0;
	for (int i = 0; i < octaves; i++)
	{
		sum += gradientnoise2D(p * freq) * amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

float ridgegradientfbm2D(float2 p, int octaves, float freq, float lacunarity, float gain, float amp)
{
	float sum = 0;
	for (int i = 0; i < octaves; i++)
	{
		sum += ridgegradientnoise2D(p * freq) * amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}


//========================================================================================================================
//	Simplex2D
//========================================================================================================================
// Simplex Noise (http://en.wikipedia.org/wiki/Simplex_noise), a type of gradient noise
// that uses N+1 vertices for random gradient interpolation instead of 2^N as in regular
// latice based Gradient Noise.

float2 simplexhash2D( float2 p )
{
	p = float2(dot(p,float2(127.17,311.71)),
			   dot(p,float2(269.58,183.32)));

	//return -1.0 + 2.0*frac(sin(p)*41.153123);
	return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float simplexnoise2D(float2 p)
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;

	float2 i = floor( p + (p.x+p.y)*K1 );
	
    float2 a = p - i + (i.x+i.y)*K2;
    float2 o = step(a.yx,a.xy);    
    float2 b = a - o + K2;
	float2 c = a - 1.0 + 2.0*K2;

    float3 h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );

	float3 n = h*h*h*h*float3( dot(a, simplexhash2D(i+0.0)), dot(b, simplexhash2D(i+o)), dot(c, simplexhash2D(i+1.0)));

    return dot(n, float3(70.0, 70.0, 70.0));
}







float3 ridgesimplexnoise2D(float2 p)
{
	return 1.0 - abs(simplexnoise2D(p));
}


float simplexfbm2D(float2 p, int octaves, float freq, float lacunarity, float gain, float amp)
{
	float sum = 0;
	for (int i = 0; i < octaves; i++)
	{
		sum += simplexnoise2D(p * freq) * amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

float ridgesimplexfbm2D(float2 p, int octaves, float freq, float lacunarity, float gain, float amp)
{
	float sum = 0;
	for (int i = 0; i < octaves; i++)
	{
		sum += ridgesimplexnoise2D(p * freq) * amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}


#endif