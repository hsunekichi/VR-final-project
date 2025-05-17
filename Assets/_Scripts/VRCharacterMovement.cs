using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class DynamicVRCharacterController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionProperty moveInput;

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
        UpdateCharacterController();
    }

    private void UpdateCharacterController()
    {
        characterController.height = 0.05f;

        Vector3 newCenter = Vector3.zero;
        newCenter.y = 0.05f;

        characterController.center = newCenter;
    }
}
