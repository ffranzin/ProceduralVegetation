
#ifndef GROUND_VEGETATION_WIND_WAVE
#define GROUND_VEGETATION_WIND_WAVE

#include "TerrainEngine.cginc"

#define WIND 0

sampler2D _windWave;
float _windSpeed;
float _windWaveSize;
float _windAmount;

//adapted from https://github.com/1upon0/sura-2015/blob/master/builtin_shaders-5.0.2f1/CGIncludes/TerrainEngine.cginc
void TerrainWaveGrassCustom(inout half4 vertex, half4 worldPosition)
{
	half4 _waveXSize = half4(0.012, 0.02, 0.06, 0.024) * _windWaveSize;
	half4 _waveZSize = half4 (0.036, .02, 0.02, 0.05) * _windWaveSize;
	half4 waveSpeed = half4 (0.3, .5, .4, 1.2) * 4;

	half4 _waveXmove = half4(0.012, 0.02, -0.06, 0.048);
	half4 _waveZmove = half4 (0.015, 0.02, -0.02, 0.1);

	half4 waves;
	waves = worldPosition * _waveXSize;
	waves += worldPosition.z * _waveZSize;

	// Add in time to model them over time
	waves += _windSpeed * waveSpeed;
	waves = frac(waves);

	half4 s, c;
	FastSinCos (waves, s, c);

	s = s * s;
	s = s * s;
	s = s * _windAmount;

	half3 waveMove = half3 (dot (s, _waveXmove), 0, dot (c * s, _waveZmove)) * half3(sin(_Time.x), 0, cos(_Time.x));
				
	vertex.xz -= waveMove.xz *_windAmount;
}

inline void ApplyWindToVertex(inout appdata_full v, half4 posWorld)
{
	_windAmount *= v.texcoord.y;
	TerrainWaveGrassCustom(v.vertex, posWorld);
}

inline void UpdateWind(half2 seed, float displacementMultiplier)
{
	float2 wind_uv = (seed % 1024) / 1024;
	_windAmount *=  displacementMultiplier * max(0.5, tex2Dlod(_windWave, float4(wind_uv, 1, 1)).r);
	_windSpeed *= _Time;
}
#endif