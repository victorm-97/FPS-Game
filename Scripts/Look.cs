using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.CrazyD.FPSProto
{
    public class Look : MonoBehaviourPunCallbacks
    {
        

        #region Variables

        public static bool cursorlock = true;
        public Transform player;
        public Transform cams;
        public Transform weapon;

        public float xSensitivity;
        public float ySensitivity;
        public float maxAngle;

        private Quaternion camCenter;

        #endregion


        #region MonoBehavior Callbacks


        // Start is called before the first frame update
        void Start()
        {
            camCenter = cams.localRotation; // set rotation origin for cameras to camCenter
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.IsMine) return;

            SetY();
            SetX();

            UpdateCursorLock();
        }
        #endregion

        #region Private Methods

        void SetY()
        {
            float t_Input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_Input, -Vector3.right);
            Quaternion t_delta = cams.localRotation * t_adj;

            if (Quaternion.Angle(camCenter, t_delta) < maxAngle)
            {
                cams.localRotation = t_delta;
            }
            weapon.rotation = cams.rotation;
        }
        void SetX()
        {
            float t_Input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_Input, Vector3.up);
            Quaternion t_delta = player.localRotation * t_adj;
            player.localRotation = t_delta;

        }

        void UpdateCursorLock()
        {
            if (cursorlock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorlock = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorlock = true;
                }
            }
        }

        #endregion
       
    }
}
