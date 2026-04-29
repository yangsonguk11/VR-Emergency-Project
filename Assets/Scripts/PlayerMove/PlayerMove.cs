using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 100f;
    public float gravity = -9.81f;
    private Vector3 velocity;


    float xRotation = 0f;
    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // WASD 이동
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // 바닥에 닿아 있으면 아래로 살짝 눌러줌
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;

        // 중력 이동
        controller.Move(velocity * Time.deltaTime);

        // 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}