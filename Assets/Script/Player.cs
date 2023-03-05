using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour{

    //Player Components
    private CharacterController _controller;
    [Header("GRAPPLE\b")]
    [SerializeField] private Camera _camera;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] [Tooltip("Layers sur lesquels le grappin peut être utilisé.")] private LayerMask surface;
    private RaycastHit _raycastHit;
    [Range(1, 100)] [Tooltip("Portée maximum du grappin.")] [SerializeField] private float maxDistance;
    private bool isGrapping;
    private Vector3 target;
    [Range(0, 100)] [Tooltip("Vitesse à laquelle le joueur se tracte avec le grappin.")] [SerializeField] private float grappleSpeed;
    [SerializeField] [Tooltip("Point de départ du grappin")] private Transform hook;
    
    //Player settings
    [Header("MOVE SETTINGS\b")]
    [Range(0, 100)] [Tooltip("Vitesse à laquelle le joueur se déplace.")] [SerializeField] private float speed;
    [Range(0, 100)] [Tooltip("Sensibilité de la caméra.")] [SerializeField] private float cameraSensitivity;
    [Range(0, 100)] [Tooltip("Limite verticale de la caméra.")] [SerializeField] private float yRotationLimit;
    private bool isGrounded => _controller.isGrounded;
    private Vector3 direction;
    private Vector3 input;
    private Quaternion rotation;
    private float gravity = -9.81f;
    [Range(0, 10)] [Tooltip("Gravity multiplier.")] [SerializeField] private float weight;
    private bool hasReachedDestination => Vector3.Distance(transform.position, target) < 1f;
    
    
    void Awake() {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        WriteMove();
        RotateCamera();
        MoveToSpot();
    }

    public void ReadMoveInput(InputAction.CallbackContext context) {
        input = context.ReadValue<Vector2>();
    }

    private void WriteMove() {
        if(isGrapping) return;
        if (isGrounded) {
            direction = transform.right * input.x * speed * Time.deltaTime;
            direction = transform.forward * input.y * speed * Time.deltaTime;
        }
        direction.y += gravity * weight * Time.deltaTime;
        _controller.Move(direction);
    }

    private void RotateCamera() {
        rotation.x += Input.GetAxis("Mouse X") * cameraSensitivity;
        rotation.y += Input.GetAxis("Mouse Y") * cameraSensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        transform.localRotation = xQuat * yQuat;
    }

    public void Grapple(InputAction.CallbackContext context) {
        if (!context.started)return;
        if (isGrapping) {
            Unhook();
        }
        else {
            if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _raycastHit, maxDistance, surface)) {
                isGrapping = true;
                target = _raycastHit.point;
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(1, target);
            }
        }
    }

    private void MoveToSpot() {
        if(!isGrapping) return;
        transform.position = Vector3.Lerp(transform.position, target,
            grappleSpeed * Time.deltaTime / Vector3.Distance(transform.position, target));
        _lineRenderer.SetPosition(0, hook.position);
        if(hasReachedDestination) Unhook();
    }

    private void Unhook() {
        isGrapping = false;
        _lineRenderer.enabled = false;
    }
}
