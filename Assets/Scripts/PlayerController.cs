using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header ("Movement")] 
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 8f;
    [SerializeField] float jumpingGravityMod = 3f; //For double jump
    [SerializeField] float fallingGravityMod = 4f; //Speed of falling
    [SerializeField] float jumpCache;
    [SerializeField] int maxJumps = 2;
    [SerializeField] int jumpCount = 0;
    [SerializeField] bool doubleJump = false;
    //[SerializeField] bool isJumping = false;
    [SerializeField] Vector2 movement = Vector2.zero;
    [SerializeField] Vector3 jumpVelocity = Vector3.zero;
    [SerializeField] Vector3 jumpDir = Vector3.up;

    [Header ("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerInputActions input;

    [Header ("Transforms")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform playerTransform;

    [Header ("Misc")]
    //[SerializeField] InputManager control;
    [SerializeField] int count = 0;
    [SerializeField] float collisionDetectionRad = 0.5f;
    [SerializeField] TextMeshProUGUI countText;
	[SerializeField] GameObject winTextObject;
    [SerializeField] GameObject restartBtn;

    void Awake() {
        input = new PlayerInputActions();
        input.Player.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        playerTransform = GetComponent<Transform>();
        groundMask = LayerMask.GetMask("Ground");

        count = 0;
        countText = GameObject.Find("CountText").GetComponent<TextMeshProUGUI>();
        winTextObject = GameObject.Find("WinText"); 
        restartBtn = GameObject.Find("RestartButton");
        SetCountText();
        winTextObject.SetActive(false);
        restartBtn.SetActive(false);
    }

    // void OnEnable() {
        
    // }

    // void OnMove(InputValue input) {
    //     movement = input.Get<Vector2>(); // Get the direction vector from input
    // }

    // void OnJump(InputAction.CallbackContext input) {
    //     // if (control.jumpOn) {
    //     //     jumpInputCache = jumpInputLenience;
    //     // } else {
    //     //     jumpInputCache -= Time.deltaTime;
    //     // }
    //     isJumping = input.action.triggered;
    // }

    Vector2 MovementDir() {
        return input.Player.Move.ReadValue<Vector2>();
        //movement = movement.normalized;
    }

    bool isJumping() {
        return input.Player.Jump.IsPressed();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isJumping() && controller.isGrounded) // If player presses space
        {
            jumpVelocity = jumpDir.normalized * jumpSpeed;
            jumpCache = Time.time + 0.3f; 
        }
        
        else if (!controller.isGrounded && isJumping() && !doubleJump && Time.time > jumpCache) { // Player is on the ground
            jumpVelocity = jumpDir.normalized * jumpSpeed;
            doubleJump = true;
        } 
        else if (controller.isGrounded){ // Player is falling down 
            Debug.Log("Grounded");
            jumpVelocity = Vector3.zero;
            jumpCount = 0;
            doubleJump = false;
        }
        else {
            Debug.Log("Falling");
            jumpVelocity += Physics.gravity * fallingGravityMod * Time.fixedDeltaTime;
        }

        movement = MovementDir() * moveSpeed;
        Vector3 dir = new Vector3(movement.x, jumpVelocity.y, movement.y);

        controller.Move(dir * Time.fixedDeltaTime); // Moving Controller
        //rb.AddForce(dir * moveSpeed * Time.fixedDeltaTime);
        //rb.AddForce(jumpSpeed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.CompareTag("Collectible")) {
            col.gameObject.SetActive(false);
            count += 1;
            SetCountText();
        }
    }

    void SetCountText() {
        countText.text = "Count: " + count.ToString();
        if (count >= 8) {
            winTextObject.SetActive(true);
            restartBtn.SetActive(true);
        }
    }
}
