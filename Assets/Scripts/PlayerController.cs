using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header ("Movement")] 
    [SerializeField] float moveSpeed = 200f;
    [SerializeField] float jumpSpeed = 70f;
    [SerializeField] float jumpingGravityMod = 60f; //For double jump
    [SerializeField] float fallingGravityMod = 20f; //Speed of falling
    [SerializeField] float jumpCache;
    // [SerializeField] int maxJumps = 2;
    // [SerializeField] int jumpCount = 0;
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

    Vector2 MovementDir() {
        return input.Player.Move.ReadValue<Vector2>();
    }

    bool isJumping() {
        return input.Player.Jump.IsPressed();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isJumping() && CheckGrounded()) {// If player presses space
            jumpVelocity = jumpDir.normalized * jumpSpeed;
            jumpCache = Time.time + 0.3f; 
        }
        
        else if (!CheckGrounded() && isJumping() && !doubleJump && Time.time > jumpCache) { // Player is in the air
            jumpVelocity = jumpDir.normalized * jumpingGravityMod;
            doubleJump = true;
        } 
        else if (CheckGrounded()) { // Player is grounded but not jumping
            jumpVelocity = Vector3.zero;
            doubleJump = false;
        }
        else { // Player is falling down 
            jumpVelocity += Physics.gravity * fallingGravityMod * Time.fixedDeltaTime;
        }

        movement = MovementDir() * moveSpeed;
        Vector3 dir = new Vector3(movement.x, 0.0f, movement.y);

        if (movement.x != 0 || movement.y != 0 || jumpVelocity != Vector3.zero) {
            rb.AddForce(dir * Time.fixedDeltaTime);
        }
        else {
            rb.velocity *= 0.9f;
        }
        rb.AddForce(jumpVelocity * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    bool CheckGrounded() {
        return Physics.CheckSphere(playerTransform.position, collisionDetectionRad, groundMask);
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
