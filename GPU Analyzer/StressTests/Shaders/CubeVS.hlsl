cbuffer TransformBuffer : register(b0)
{
    float4x4 viewProj;
    float time;
};

struct VS_IN
{
    float3 pos : POSITION;
    float4 instancePos : INSTANCE_POS;
    float4 instanceColor : INSTANCE_COLOR;
    float instanceRot : INSTANCE_ROT;
};

struct VS_OUT
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};

// Функция создания матрицы вращения вокруг оси Y
float4x4 rotationY(float angle)
{
    float s, c;
    sincos(angle, s, c);
    
    return float4x4(
        c, 0, s, 0,
        0, 1, 0, 0,
        -s, 0, c, 0,
        0, 0, 0, 1
    );
}

// Функция создания матрицы вращения вокруг оси X
float4x4 rotationX(float angle)
{
    float s, c;
    sincos(angle, s, c);
    
    return float4x4(
        1, 0, 0, 0,
        0, c, -s, 0,
        0, s, c, 0,
        0, 0, 0, 1
    );
}

// Функция создания матрицы вращения вокруг оси Z
float4x4 rotationZ(float angle)
{
    float s, c;
    sincos(angle, s, c);
    
    return float4x4(
        c, -s, 0, 0,
        s, c, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );
}

VS_OUT main(VS_IN input)
{
    VS_OUT output;
    
    // Комбинируем начальное вращение инстанса с анимацией от времени
    float rotationAngle = input.instanceRot + time * 0.5f; // 0.5 радиана в секунду
    
    // Создаем матрицы вращения
    float4x4 rotY = rotationY(rotationAngle);
    float4x4 rotX = rotationX(rotationAngle * 0.7f); // Немного другая скорость
    float4x4 rotationMatrix = mul(rotX, rotY);
    
    // Матрица масштаба (уменьшаем кубы)
    float4x4 scaleMatrix = float4x4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );
    
    // Матрица переноса (позиция инстанса)
    float4x4 translationMatrix = float4x4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        input.instancePos.x, input.instancePos.y, input.instancePos.z, 1
    );
    
    // Комбинируем: масштаб -> вращение -> перенос
    float4x4 modelMatrix = mul(mul(scaleMatrix, rotationMatrix), translationMatrix);
    
    // Трансформация вершин
    float4 worldPos = mul(float4(input.pos, 1.0), modelMatrix);
    float4 viewPos = mul(worldPos, viewProj);
    
    output.pos = viewPos;
    output.color = input.instanceColor;
    
    return output;
}