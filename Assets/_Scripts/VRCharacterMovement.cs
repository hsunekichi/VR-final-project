using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class DynamicVRCharacterController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionProperty moveInput;

    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float additionalHeight = 0.05f;
    [SerializeField] private float minHeight = 0.0f;
    [SerializeField] private float maxHeight = 0.5f;
    [SerializeField] private float gravity = -9.81f;
    private float fallingSpeed = 0f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    private void OnEnable()
    {
        moveInput.action.Enable();
    }

    private void OnDisable()
    {
        moveInput.action.Disable();
    }

    private void Update()
    {
        //if (characterController == null || !characterController.enabled || cameraTransform == null)
        //    return;

        UpdateCharacterController();

        //Vector2 input = moveInput.action.ReadValue<Vector2>();

        //if (input == Vector2.zero)
        //    return;

        //Vector3 direction = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        //direction.y = 0;
        //direction.Normalize();

        //// Gravedad aplicada solo si no estamos en suelo
        //if (characterController.isGrounded)
        //{
        //    fallingSpeed = gravity * Time.deltaTime;
        //}
        //else
        //{
        //    fallingSpeed += gravity * Time.deltaTime;
        //    Debug.Log(fallingSpeed);
        //}


        //Vector3 move = direction * moveSpeed + Vector3.up * fallingSpeed;

        //characterController.Move(move * Time.deltaTime);
    }

    private void UpdateCharacterController()
    {
        // Solo usamos la altura de la cámara local para ajustar el height
        float headHeight = Mathf.Clamp(cameraTransform.localPosition.y, minHeight, maxHeight);
        //characterController.height = headHeight + additionalHeight;
        characterController.height = 0.05f;

        // El center solo tiene en cuenta la altura, no x ni z (estos deben estar siempre en 0 en VR Rig)
        Vector3 newCenter = Vector3.zero;
        newCenter.y = 0.05f;
        //newCenter.y = (characterController.height / 2f);

        characterController.center = newCenter;
    }
}
