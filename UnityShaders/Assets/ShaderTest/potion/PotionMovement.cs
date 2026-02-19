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
    [Range(0f, 5f)] public float bubbleThreshold = 1.5f;
    [Range(0f, 10f)] public float bubbleRecoverySpeed = 2f;
    float bubbleAmount=0.0f;
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

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        // -------- MOVIMIENTO --------
        Vector3 velocity = (transform.position - lastPosition) / deltaTime;
        lastPosition = transform.position;

        Vector3 movementEffect = -velocity * movementInfluence;

        // -------- ROTACIÓN --------
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);
        lastRotation = transform.rotation;

        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        Vector3 angularVelocity = axis * angle * Mathf.Deg2Rad / deltaTime;
        Vector3 rotationEffect = -angularVelocity * rotationInfluence;

        // -------- COMBINAR --------
        Vector3 targetWobble = movementEffect + rotationEffect;

        // Intensidad total del movimiento
        float intensity = targetWobble.magnitude;

        // Activar burbujas si supera el umbral
        bubbleAmount = Mathf.Lerp(
              bubbleAmount,
              intensity > bubbleThreshold ? 1f : 0f,
              deltaTime * bubbleRecoverySpeed
          );

        // Suavizar wobble
        wobble = Vector3.Lerp(wobble, targetWobble, deltaTime * recoverySpeed);


       

        // Aplicar al shader
        rend.GetPropertyBlock(block);
        block.SetFloat("_Bubble", bubbleAmount);
        block.SetFloat("_RotationX", wobble.x);
        block.SetFloat("_RotationY", wobble.z);

        rend.SetPropertyBlock(block);
    }
}
