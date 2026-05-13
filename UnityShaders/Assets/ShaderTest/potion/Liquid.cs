using UnityEngine;

[ExecuteAlways]
public class Liquid : MonoBehaviour
{
    [SerializeField][Range(0f, 2f)] float SloshSpeed = 1.0f;
    [SerializeField][Range(0f, 5f)] float ReturnSpeed = 0.63f;
    [SerializeField][Range(0f, 0.5f)] float MaxSlosh = 0.05f;

    private Renderer rend;
    private MaterialPropertyBlock block;

    private Vector3 lastPos;
    private Vector3 vel;

    private Quaternion lastRot;
    private Vector3 angular_vel;

    private float sloshX = 0f;
    private float sloshZ = 0f;
    private float slosh_add_x = 0f;
    private float slosh_add_z = 0f;

    private float time = 0f;
    private float oscillation = 0f;

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

        // --- RETURN SPEED ---
        // We use Mathf.Lerp to gradually move the slosh value back to zero.
        // This simulates "viscosity" or "friction", making the liquid return to a resting state.
        // 
        // Logic: In every frame, we move a percentage (Recovery * deltaTime) of the remaining 
        // distance towards 0. This creates an exponential decay effect where the liquid 
        // moves fast at first and then slows down smoothly as it approaches the center, 
        // ensuring the stabilization feels organic rather than robotic or linear.
        slosh_add_x = Mathf.Lerp(slosh_add_x, 0f, deltaTime * ReturnSpeed);
        slosh_add_z = Mathf.Lerp(slosh_add_z, 0f, deltaTime * ReturnSpeed);

        // -- VELOCITY --
        vel = (transform.position - lastPos) / deltaTime;

        // --- ANGULAR VELOCITY CALCULATION ---
        // 1. Calculate the rotation difference (delta) between the current and last frame.
        // 2. Convert that rotation into Euler angles (degrees).
        // 3. Use DeltaAngle to ensure the rotation takes the shortest path (fixes the 0-360 degree jump).
        // 4. Divide by deltaTime to get the velocity (degrees per second).
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
        Vector3 deltaEuler = deltaRot.eulerAngles;

        deltaEuler.x = Mathf.DeltaAngle(0, deltaEuler.x);
        deltaEuler.y = Mathf.DeltaAngle(0, deltaEuler.y);
        deltaEuler.z = Mathf.DeltaAngle(0, deltaEuler.z);

        angular_vel = deltaEuler / deltaTime;

        // --- MOVEMENT INERTIA ---
        // The liquid reacts to both linear and rotational movement of the container.
        //
        // Linear velocity simulates how the liquid lags behind when the object moves.
        // For example, if the container moves to the right, the liquid appears to
        // push toward the opposite side due to inertia.
        //
        // A portion of the vertical velocity is also included to create a subtle
        // response when the object moves up or down.
        //
        // Angular velocity simulates the effect of rotation on the liquid.
        // When the container tilts or spins, the liquid shifts accordingly,
        // imitating centrifugal and rotational forces.
        //
        // The resulting value is clamped to avoid excessive sloshing that would
        // look unstable or unrealistic.
        slosh_add_x += Mathf.Clamp(
            vel.x + vel.y * 0.2f+(angular_vel.z + angular_vel.y) * 0.01f,
            -MaxSlosh,
            MaxSlosh);
        //(angularVelocity.x + angularVelocity.y) * 0.01f
        slosh_add_z += Mathf.Clamp(
            vel.z + vel.y * 0.2f+ (angular_vel.x + angular_vel.y) * 0.01f,
            -MaxSlosh,
            MaxSlosh);

        // OSCILLATION
        float pulse = 2f * Mathf.PI * SloshSpeed;
        float target_cos = Mathf.Cos(pulse * time);

        oscillation = Mathf.Lerp(oscillation, target_cos, deltaTime * Mathf.Clamp(vel.magnitude+angular_vel.magnitude*0.01f, 6f, 10f));
        oscillation = Mathf.Lerp( oscillation,1.0f, deltaTime * (vel.magnitude + angular_vel.magnitude * 0.01f) * 2f);
        sloshX = slosh_add_x * oscillation;
        sloshZ = slosh_add_z * oscillation;

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