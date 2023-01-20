using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.CrazyD.FPSProto
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables
        public Gun[] loadout;
        public Transform weaponParent;
        public GameObject bulletholePrefab;
        public LayerMask canBeShot;

        private float currentCooldown;
        private int currentindex;
        private GameObject currentWeapon;

        private bool isReloading;


        Animator animator;
        #endregion


        #region MonoBehavior Callbacks
        // Start is called before the first frame update
        private void Start()
        {
            foreach (Gun a in loadout) a.Ininitialize();
            Equip(0);
        }

        // Update is called once per frame
        void Update()
        {
                if (!photonView.IsMine)
                {

                    if (currentWeapon != null)
                    {

                        currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

                    }

                    return;
                }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            }

            if (currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1));

                if (Input.GetMouseButton(0) && currentCooldown <= 0)
                {
                    if (loadout[currentindex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                    else StartCoroutine(Reload(loadout[currentindex].reload));
                }

                if(Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[currentindex].reload));

                // Weapon Position Elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

                //Cooldown
                if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
            }
        }
        #endregion

        #region Private Methods

        IEnumerator Reload (float p_wait)
        {
            isReloading = true;
            currentWeapon.SetActive(false);

            yield return new WaitForSeconds(p_wait);

            loadout[currentindex].Reload();
            currentWeapon.SetActive(true);

            isReloading = false;
        }

        [PunRPC]
        void Equip(int p_ind)
        {

            if (currentWeapon != null)
            {
                if(isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }

            // animator.SetBool("Rifle", true);
            currentindex = p_ind;

            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().ismine = photonView.IsMine;


            currentWeapon = t_newWeapon;

        }

        void Aim(bool p_isAiming)
        {
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                //aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentindex].aimSpeed);

            }
            else
            {
                //hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentindex].aimSpeed);
            }

        }

        [PunRPC]
        void Shoot()
        {

            Transform t_spwan = transform.Find("Cameras/FirstPersonCamera");


            //bloom
            Vector3 t_bloom = t_spwan.position + t_spwan.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentindex].bloom, loadout[currentindex].bloom) * t_spwan.up;
            t_bloom += Random.Range(-loadout[currentindex].bloom, loadout[currentindex].bloom) * t_spwan.right;
            t_bloom -= t_spwan.position;
            t_bloom.Normalize();


            //Cooldown
            currentCooldown = loadout[currentindex].fireRate;


            //Raycast
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spwan.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newhole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newhole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newhole, 5f);

                if (photonView.IsMine)
                {
                    //shooting other player on network
                    if (t_hit.collider.gameObject.layer == 11)
                    {
                        // RPC call to damage player goes here
                        t_hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentindex].damage);
                    }
                }
            }

            //gun fx
            currentWeapon.transform.Rotate(-loadout[currentindex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentindex].kickback;


        }

        [PunRPC]
        private void TakeDamage (int p_damage)
        {
           GetComponent<Player>().TakeDamage(p_damage);
        }

        #endregion

        #region Public Methods

        public void RefreashAmmo(Text p_text)
        {
            int t_clip = loadout[currentindex].GetClip();
            int t_stash = loadout[currentindex].GetStash();

            p_text.text = t_clip.ToString("D2") + " / " + t_stash.ToString("D2");
        }


        #endregion
    }
}