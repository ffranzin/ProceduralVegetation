#ifndef QUATERNION_UTILS
#define QUATERNION_UTILS

inline float4 QuaternionMultiplication(float4 q1, float4 q2) {

	return float4(q1.x * q2.w + q1.y * q2.z - q1.z * q2.y + q1.w * q2.x,
				  -q1.x * q2.z + q1.y * q2.w + q1.z * q2.x + q1.w * q2.y,
				  q1.x * q2.y - q1.y * q2.x + q1.z * q2.w + q1.w * q2.z,
				  -q1.x * q2.x - q1.y * q2.y - q1.z * q2.z + q1.w * q2.w);
}

 
inline float4 QuaternionFromToRotation(float3 from, float3 to)
{
	float k_cos_theta = dot(from, to);
	float k = sqrt(length(from) * length(to));

	if (k_cos_theta / k == -1)	return float4(0,0,0,0);

	float3 c = cross(from, to);

	return float4(c.x, c.y, c.z, (k_cos_theta + k));
} 



inline	float4 QuaternionFromToRotationNormalized(float3 from, float3 to)
{
	float k_cos_theta = dot(from, to);
	float k = 1; //sqrt(length(from) * length(to));

	float3 c = cross(from, to);

	return float4(c.x, c.y, c.z, (k_cos_theta + k));
} 


inline float3 QuaternionRotatePoint(float3 p, float4 r)
{
    float4 conjugation = float4(-r.x, -r.y, -r.z, r.w);
	 
    return QuaternionMultiplication(r, 
						QuaternionMultiplication(float4(p, 0), conjugation)).xyz;
}



inline float4 QuaternionFromNormalizedDirection(float3 dir, float angle)
{	
	angle = radians(angle);

	return float4(dir * sin(angle * .5), cos(angle * .5));
}

#endif