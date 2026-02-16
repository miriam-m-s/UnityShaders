void LightingCelShaded_float(
    float Smoothness,
    float3 Normal,
    float3 View,
    float3 LightDirection,
    float3 LightColor,
    out float3 Color)
{
    float3 N = normalize(Normal);
    float3 V = normalize(View);
    float3 L = normalize(-LightDirection);

    float3 H = normalize(L + V);

    float spec = saturate(dot(N, H));
    spec = pow(spec, exp2(10 * Smoothness + 1));
    spec *= Smoothness;
    spec = smoothstep(0.3f,0.5f,spec);

    Color = LightColor * spec;
}