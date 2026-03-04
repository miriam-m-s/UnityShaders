using UnityEngine;

[ExecuteAlways]
public class Liquid : MonoBehaviour
{
    [SerializeField][Range(0f, 2f)] float WobbleSpeed = 1.0f;    // Velocidad de la onda
    [SerializeField][Range(0f, 2f)] float Recovery = 0.1f;       // Qué tan rápido vuelve a 0
    [SerializeField][Range(0f, 0.5f)] float MaxWobble = 0.05f;   // Máximo wobble
    [SerializeField][Range(0f, 5f)] float VelocityScale = 1.0f;  // Escala de efecto según velocidad
    [SerializeField] private float wobbleOffsetLimit = 0.2f;
    private Renderer rend;
    private Vector3 lastPos;
    private Vector3 velocity;

    private float wobbleX = 0f;
    private float wobbleZ = 0f;
    private float wobbleAddX = 0f;
    private float wobbleAddZ = 0f;
    private float time = 0f;
    float sine=0f;
    Vector3 lastvelocity;
    void Awake()
    {
        rend = GetComponent<Renderer>();
        lastPos = transform.position;
        lastvelocity=Vector3.zero;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        time += deltaTime;

        // Calcula la velocidad
        velocity = (transform.position - lastPos) / deltaTime;

        // Aplica wobble solo si supera un threshold
        wobbleAddX += Mathf.Clamp(velocity.x + (velocity.y * 0.2f) * VelocityScale, -MaxWobble, MaxWobble);
        wobbleAddZ += Mathf.Clamp(velocity.z + (velocity.y * 0.2f) * VelocityScale, -MaxWobble, MaxWobble);

        // Decay / recuperación hacia 0
        wobbleAddX = Mathf.Lerp(wobbleAddX, 0f, deltaTime * Recovery);
        wobbleAddZ = Mathf.Lerp(wobbleAddZ, 0f, deltaTime * Recovery);

        // Onda senoidal para el wobble
        float pulse = 2f * Mathf.PI * WobbleSpeed;

        //sine = Mathf.Lerp(sine, Mathf.Sin(pulse * time), deltaTime * Mathf.Clamp(velocity.magnitude, 0,1));

        float mVelocity = Mathf.Clamp(velocity.magnitude, 0f, 1f); // normaliza magnitud
        float targetSine = Mathf.Sin(pulse * time);    // escala la onda según velocidad

        // Lerp desde el valor actual hacia el objetivo, usando deltaTime * Recovery
        bool tooMuchWobble =
            Mathf.Abs(wobbleAddX) > wobbleOffsetLimit ||
            Mathf.Abs(wobbleAddZ) > wobbleOffsetLimit;
        //if (!tooMuchWobble)
        //{
        float motion = velocity.magnitude ;

        // Normalizar movimiento (ajusta 5f según tu escala real)
        float normalizedMotion = Mathf.Clamp01(motion / 5f);

        // Invertirlo (más movimiento = menos wobble)
        float wobbleFactor = 1f - normalizedMotion;
        sine = Mathf.Lerp(sine, targetSine, deltaTime * wobbleFactor);
            //}
     

            wobbleX = wobbleAddX * sine;
        wobbleZ = wobbleAddZ * sine;
        
        // Envía al shader
        rend.sharedMaterial.SetFloat("_RotationX", Mathf.Clamp(wobbleX,-1,1));
        rend.sharedMaterial.SetFloat("_RotationY", Mathf.Clamp(wobbleZ, -1, 1));

        // Guarda posición para el siguiente frame
        lastPos = transform.position;
        lastvelocity = velocity;
    }
}