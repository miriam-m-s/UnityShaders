using UnityEngine;

[ExecuteAlways]
public class PotionMovement : MonoBehaviour
{
    Renderer rend;
    MaterialPropertyBlock block;

    Vector3 lastPosition;
    Quaternion lastRotation;

    Vector3 wobble;

    [Range(0f, 2f)] public float movementInfluence = 0.2f;
    [Range(0f, 2f)] public float rotationInfluence = 0.2f;
    [Range(0f, 10f)] public float recoverySpeed = 3f;
    [Range(0f, 5f)] public float bubbleThreshold = 1.0f;
    [Range(0f, 10f)] public float bubbleRecoverySpeed = 2f;

    float bubbleAmount=0.0f;
    float bubbleVelocity=0.0f;
    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        if (rend == null) return;
        float deltaTime = Application.isPlaying ? Time.deltaTime : 0.02f;

        // 1. Calcular velocidades
        Vector3 velocity = (transform.position - lastPosition) / deltaTime;
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad) / deltaTime;

        // 2. Efecto visual (Wobble)
        Vector3 targetWobble = (-velocity * movementInfluence) + (-angularVelocity * rotationInfluence);
        wobble = Vector3.Lerp(wobble, targetWobble, deltaTime * recoverySpeed);

        // 3. Lógica de Burbujas (Basada en la fuerza del movimiento actual)
        // Usamos targetWobble.magnitude para detectar el "frenazo" o cambio brusco
        float currentIntensity = targetWobble.magnitude;

        if (currentIntensity > bubbleThreshold)
        {
            // Si el movimiento es fuerte, las burbujas suben rápido
            bubbleAmount = Mathf.Lerp(bubbleAmount, 1f, deltaTime * 5f);
            bubbleVelocity = Mathf.Lerp(bubbleVelocity, 1f, deltaTime * 5f);
        }
        else
        {
            // Si se detiene, las burbujas desaparecen gradualmente
            bubbleAmount = Mathf.Lerp(bubbleAmount, 0f, deltaTime * bubbleRecoverySpeed);
            bubbleVelocity = Mathf.Lerp(bubbleVelocity, 0f, deltaTime * bubbleRecoverySpeed);
        }

        // 4. Aplicar al Shader
        rend.GetPropertyBlock(block);
       

        block.SetFloat("_Bubble", bubbleAmount);
        block.SetFloat("_BubbleVelocity",bubbleVelocity* 0.0005f);

        // Mapeo de rotación del líquido (Wobble)
        float maxWobble = 2f;
        block.SetFloat("_RotationX", Mathf.Clamp(wobble.x / maxWobble, -1f, 1f));
        block.SetFloat("_RotationY", Mathf.Clamp(wobble.z / maxWobble, -1f, 1f));

        rend.SetPropertyBlock(block);

        // Guardar estados
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}
