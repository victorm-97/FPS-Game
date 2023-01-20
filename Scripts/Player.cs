using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.CrazyD.FPSProto
{
    public class Player : MonoBehaviourPunCallbacks
    {
        Animator animator;

        #region Variables

        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public int max_Health;
        public int isWalkingHash;
        public Camera normal_cam;
        public GameObject camera_parent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;

        private Rigidbody rig;

        private Vector3 weaponParentOrigin;
        private Vector3 targetWeaponBobposition;

        private Transform ui_healthbar;
        private Text ui_ammo;
        private float movementCounter;
        private float idleCounter;
        private float basedFOV;
        private float sprintFOVModifier = 1.5f;
        private int current_health;


        private Manager manager;
        private Weapon weapon;

        #endregion


        #region MonoBehavior Callbacks
        // Start is called before the first frame update
         private void Start()
        {
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();
            current_health = max_Health;
            
            camera_parent.SetActive(photonView.IsMine);

            animator = gameObject.GetComponent<Animator>();
            

            if(!photonView.IsMine)  gameObject.layer = 11;
            
            
            basedFOV = normal_cam.fieldOfView;

            if(Camera.main) Camera.main.enabled = false;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;

            if (photonView.IsMine)
            {
                ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefereshHealthBar();
            }
        }


        
        private void Update()
        {
            if (!photonView.IsMine) return;

            //Axes
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            Debug.Log(t_vmove);


            //Controls
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift);


            //States
            // bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);


            bool isJumping = jump && Isgrounded();
            bool issprinting = sprint && t_vmove > 0 && !isJumping && Isgrounded();


            animator.SetFloat("IsWalking", t_vmove);
            animator.SetBool("Jump", isJumping);
            animator.SetBool("Sprinting", issprinting);



            //Jumping
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }


            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100);

            // Head Bob
            if (t_hmove == 0 && t_vmove == 0)
            {
                Headbob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobposition, Time.deltaTime * 2f);
            }
            else if(!issprinting)
            {
                Headbob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobposition, Time.deltaTime * 6f);
            }
            else
            {
                Headbob(movementCounter, 0.15f, 0.075f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobposition, Time.deltaTime * 10f);
            }

            //UI Refreshes
            RefereshHealthBar();
            weapon.RefreashAmmo(ui_ammo);
        }
        bool Isgrounded()

        {
            //Controls
            return Physics.CheckSphere(groundDetector.position, .1f, ground);

        }

    

         void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            //Axes
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");


            //Controls
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift);


            //States
            // bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            

            bool isJumping = jump && Isgrounded();
            bool issprinting = sprint && t_vmove > 0 && !isJumping && Isgrounded();



            //Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustSpeed = speed;
            if (issprinting) t_adjustSpeed *= sprintModifier;

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustSpeed * Time.deltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            //Field of View
            if(issprinting)
            {
                normal_cam.fieldOfView = Mathf.Lerp(normal_cam.fieldOfView, basedFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else
            {
                normal_cam.fieldOfView = Mathf.Lerp(normal_cam.fieldOfView, basedFOV, Time.deltaTime * 8f);
            }
            
        }
        #endregion

        #region Private Methods

        void Headbob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            targetWeaponBobposition  = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z * 2) * p_y_intensity, 0);

        }

        void RefereshHealthBar()
        {
            float t_health_ratio = (float)current_health / (float)max_Health;
            ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }
        #endregion

        #region Public Methods

        
        public void TakeDamage (int p_damage)
        {
            if (photonView.IsMine)
            {
                current_health -= p_damage;
                RefereshHealthBar();

                if(current_health <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                    
                }
            }

        }

        #endregion

    }

}