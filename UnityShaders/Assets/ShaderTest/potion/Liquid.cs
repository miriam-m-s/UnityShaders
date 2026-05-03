using UnityEngine;

[ExecuteAlways]
public class Liquid : MonoBehaviour
{
    [SerializeField][Range(0f, 2f)] float SloshSpeed = 1.0f;
    [SerializeField][Range(0f, 5f)] float Recovery = 0.1f;
    [SerializeField][Range(0f, 0.5f)] float MaxSlosh = 0.05f;

    private Renderer rend;
    private MaterialPropertyBlock block;

    private Vector3 lastPos;
    private Vector3 velocity;

    private Quaternion lastRot;
    private Vector3 angularVelocity;

    private float sloshX = 0f;
    private float sloshZ = 0f;
    private float sloshAddX = 0f;
    private float sloshAddZ = 0f;

    private float time = 0f;
    private float sine = 0f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void OnEnable()
    {
        if (block == null)
            block = new MaterialPropertyBlock();

        if (rend == null)
            rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        time += deltaTime;

        // RECOVERY
        sloshAddX = Mathf.Lerp(sloshAddX, 0f, deltaTime * Recovery);
        sloshAddZ = Mathf.Lerp(sloshAddZ, 0f, deltaTime * Recovery);

        // VELOCIDAD LINEAL
        velocity = (transform.position - lastPos) / deltaTime;

        // VELOCIDAD ANGULAR
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
        Vector3 deltaEuler = deltaRot.eulerAngles;

        deltaEuler.x = Mathf.DeltaAngle(0, deltaEuler.x);
        deltaEuler.y = Mathf.DeltaAngle(0, deltaEuler.y);
        deltaEuler.z = Mathf.DeltaAngle(0, deltaEuler.z);

        angularVelocity = deltaEuler / deltaTime;

        // INERCIA POR MOVIMIENTO
        sloshAddX += Mathf.Clamp(
            velocity.x + velocity.y * 0.2f+(angularVelocity.z + angularVelocity.y) * 0.01f,
            -MaxSlosh,
            MaxSlosh);
        //(angularVelocity.x + angularVelocity.y) * 0.01f
        sloshAddZ += Mathf.Clamp(
            velocity.z + velocity.y * 0.2f+ (angularVelocity.x + angularVelocity.y) * 0.01f,
            -MaxSlosh,
            MaxSlosh);

        // OSCILACIÓN
        float pulse = 2f * Mathf.PI * SloshSpeed;
        float targetSine = Mathf.Sin(pulse * time);

        sine = Mathf.Lerp(sine, targetSine, deltaTime * Mathf.Clamp(velocity.magnitude+angularVelocity.magnitude*0.01f, 6f, 10f));
        sine = Mathf.Lerp( sine,1.0f, deltaTime * (velocity.magnitude + angularVelocity.magnitude * 0.01f) * 2f);
        sloshX = sloshAddX * sine;
        sloshZ = sloshAddZ * sine;

        // ENVIAR AL SHADER (sin crear materiales)
        rend.GetPropertyBlock(block);

        block.SetFloat("_RotationX", Mathf.Clamp(-sloshX, -1f, 1f));
        block.SetFloat("_RotationZ", Mathf.Clamp(sloshZ, -1f, 1f));

        rend.SetPropertyBlock(block);

        // GUARDAR FRAME
        lastPos = transform.position;
        lastRot = transform.rotation;
    }
}