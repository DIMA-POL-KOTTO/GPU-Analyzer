cbuffer TransformBuffer : register(b0)
{
    float4x4 mvp;
};

struct VSInput
{
    float3 pos : POSITION;
};

struct VSOutput
{
    float4 pos : SV_POSITION;
};

VSOutput main(VSInput input)
{
    VSOutput output;
    output.pos = mul(float4(input.pos, 1.0), mvp);
    return output;
}
