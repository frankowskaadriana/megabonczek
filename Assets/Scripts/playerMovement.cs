using UnityEditorInternal;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode fowardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    [Header("CustomData")]
    public Rigidbody rb ;
    public float Movespeed = 5f;
    public float JumpHeight = 5f;



    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(fowardKey))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * Movespeed);
        }
        if (Input.GetKey(backKey))
        {
            transform.Translate(Vector3.back * Time.deltaTime * Movespeed);
        }
        if (Input.GetKey(leftKey))
        {
            transform.Translate(Vector3.left * Time.deltaTime * Movespeed);
        }
        if (Input.GetKey(rightKey))
        {
            transform.Translate(Vector3.right * Time.deltaTime * Movespeed);
        }
         if (Input.GetKey(jumpKey))
        {
            transform.Translate(Vector3.up * Time.deltaTime * JumpHeight);
        }
    }
}
