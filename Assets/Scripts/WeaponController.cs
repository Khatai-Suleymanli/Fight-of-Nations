using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

public class WeaponController : MonoBehaviour
{
    [Header("Shooting settings")]
    [SerializeField] float shootRange = 200;
    [SerializeField] float impact = 30.0f;
    [SerializeField] float fireRate = 19f;
    [SerializeField] float fireRateSniper = 5f;
    [SerializeField] float fireRateMakarov = 10f;
    [SerializeField] float launchVelocity = 2000f;

    [Header("Impact Effects")]
    public GameObject impactEffect;
    public GameObject impactEffectSniper;
    public GameObject impactEffectMakarov;

    [Header("Weapon Heads")]
    public GameObject weaponHead;
    public GameObject sniperHead;
    public GameObject makarovHead;


    private Animator animator;

    // shooting effects
    [Header("Bullet Spawn points for weapons")]
    public Transform spawnPoint;
    public Transform sniperSpawnPoint;
    public Transform makarowSpawnPoint;

    [Header("Muzzles")]
    public GameObject muzzle;  // ------------------------------------------------------------------------
    public GameObject muzzleSniper;
    public GameObject muzzleMakarov;
    [SerializeField] GameObject[] muzzles;  // muzzle array to collect and then destroy all the muzzle effects when changing weapons

    [Header("GORE effects")]
    public GameObject blood;

    [Header("Bullet shells")]
    // bullet gilizleri 
    [SerializeField] GameObject bulletShell;  // --------------------------------------------------------
    [SerializeField] GameObject bulletShellSniper;
    [SerializeField] GameObject bulletShellMakarov;

    // Spawn points for bullet chells
    [Header("Spawn points for bullet chells")]
    [SerializeField] Transform spawnPoint2;
    [SerializeField] Transform sniperSpawnPoint2;
    [SerializeField] Transform makarovSpawnPoint2;

    //rotation
    [Header("Weapon Rotation")]
    [SerializeField] private Transform weaponTransform;

    [Header("crosshair")]
    [SerializeField] private RawImage cross;
    Rect rect;
    float width;
    float height;
    public RectTransform xHitEffectUI; // Assuming it's a RectTransform (like for an Image or Text)

    [Header("Camera Ref")]
    public Camera mainCamera; // Reference to the main camera in the scene



    private float nextTimeToShoot;

    [Header("Reload Stuff")]
    [SerializeField] int bulletCount = 30; // gulle sayi
    [SerializeField] int bulletCountSniper = 10;
    [SerializeField] int bulletCountMakarov = 8;

    [SerializeField] bool isEmpty = false;
    [SerializeField] bool isEmptySniper = false;
    [SerializeField] bool isEmptyMakarov = false;

    public bool isReloading = false;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI bulletCountText;
    public Image bullet3;
    public Image bullet2;
    public Image bullet1;

    public Image AKImage;
    public Image SniperImage;
    public Image MakarovImage;


    [Header("Audio Stuff")]
    [SerializeField] AudioClip gunSound;
    [SerializeField] AudioClip emptyhot;
    [SerializeField] AudioClip gunSoundSniper;
    [SerializeField] AudioClip gunSoundMakarov;


    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip reloadSoundSniper;
    [SerializeField] AudioClip reloadSoundMakarov;

    AudioSource audioSource;

    [Header("bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isMoving;

    public bool Sniper = false;
    [SerializeField] bool AK47 = true;
    public bool isMakarov = false;


    [Header("Weapons")]

    public GameObject AKM;
    public GameObject SniperRifle;
    public GameObject MakarovPistol;





    // empty code:
    public bool isAiming;
    public bool canNotShoot;
    Combined combined;

    // light
    [Header("Shooting Lights")]
    public GameObject pointLight;
    public GameObject pointLightSniper;
    public GameObject pointLightMakarov;



    [Header("Post Processing Effects")]
    public PostProcessVolume volume;
    private DepthOfField depthOfField;
    private AmbientOcclusion ambientOcclusion;

    [Header("bool for changing weapons")]
    [SerializeField] private bool isChanging = false;

    private void Start()
    {
        InitializeAnimator();

        audioSource = GetComponent<AudioSource>();
        combined = GetComponent<Combined>();

        // setting volume effects true at the start
        if (volume.profile.TryGetSettings(out depthOfField) &&
            volume.profile.TryGetSettings(out ambientOcclusion))
        {
            SetEffectsActive(true);
        }
    }

    private void LateUpdate()
    {
        HandleAiming();
        HandleShooting();
        HandleReloading();
        getImageSize();

         // updating movement bools
        isSprinting = Input.GetKey(KeyCode.LeftShift) ? true : false;
        isMoving = Input.GetKey(KeyCode.W) ? true : false;

        isAiming = Input.GetKey(KeyCode.Mouse1) ? true : false;
        canNotShoot = (isSprinting && isMoving || isReloading) && !isAiming || (isReloading) && isAiming;  // not-shooting cases


        // setting basic weapon parameters on change:  bullet count, blur effect
        if (AK47)
        {
            bulletCountText.text = bulletCount.ToString() + "/30";
        }
        else if (Sniper)
        {
            bulletCountText.text = bulletCountSniper.ToString() + "/10";
            // if aiming with the sniper disable aiming blur effect
            if (isAiming)
            {
                DisableEffects();
            }
            else if (!isAiming)  // if stopping aiming re-activate the effects
            {
                SetEffectsActive(true);
            }

        }
        else if (isMakarov)
        {
            bulletCountText.text = bulletCountMakarov.ToString() + "/8";
        }

        // ------------------------ setting full parameters for the weapons------------------------:
        // MAKAROV
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isReloading)
        {
            isChanging = true;
            isMakarov = true;
            AK47 = false;
            Sniper = false;
            StartCoroutine(makeBoolFalse());

            animator.SetBool("Makarov", true);
            animator.SetBool("AK47", false);
            animator.SetBool("Sniper", false);

            MakarovPistol.gameObject.SetActive(true);
            AKM.gameObject.SetActive(false);
            SniperRifle.gameObject.SetActive(false);

            MakarovImage.gameObject.SetActive(true);
            AKImage.gameObject.SetActive(false);
            SniperImage.gameObject.SetActive(false);

            muzzles = GameObject.FindGameObjectsWithTag("Effects");

            for (int i = 0; i < muzzles.Length; i++)
            {
                Destroy(muzzles[i]);
            }
            combined.Dayandir();
        }
        // SNIPER
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isReloading)
        {
            isChanging = true;
            Sniper = true;
            AK47 = false;
            isMakarov = false;
            StartCoroutine(makeBoolFalse());

            animator.SetBool("Sniper", true);
            animator.SetBool("AK47", false);
            animator.SetBool("Makarov", false);

            SniperRifle.gameObject.SetActive(true);
            AKM.gameObject.SetActive(false);
            MakarovPistol.gameObject.SetActive(false);

            SniperImage.gameObject.SetActive(true);
            MakarovImage.gameObject.SetActive(false);
            AKImage.gameObject.SetActive(false);

            muzzles = GameObject.FindGameObjectsWithTag("Effects");

            for (int i = 0; i < muzzles.Length; i++)
            {
                Destroy(muzzles[i]);
                //muzzles[i].(false);
            }
            combined.Dayandir();
        }
        // AK
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isReloading)
        {
            isChanging = true;
            Sniper = false;
            AK47 = true;
            isMakarov = false;
            StartCoroutine(makeBoolFalse());

            animator.SetBool("Sniper", false);
            animator.SetBool("AK47", true);
            animator.SetBool("Makarov", false);

            AKM.gameObject.SetActive(true);
            SniperRifle.gameObject.SetActive(false);
            MakarovPistol.gameObject.SetActive(false);

            AKImage.gameObject.SetActive(true);
            SniperImage.gameObject.SetActive(false);
            MakarovImage.gameObject.SetActive(false);
            muzzles = GameObject.FindGameObjectsWithTag("Effects");
            for (int i = 0; i < muzzles.Length; i++)
            {
                //muzzles[i].SetActive(false);
                Destroy(muzzles[i]);
            }
            combined.Dayandir();
        }

    }

    IEnumerator makeBoolFalse()
    {
        yield return new WaitForSeconds(0.9f);
        isChanging = false;
    }


    // ------------------ crosshair functions-------------------------:
    public void getImageSize()
    {
        rect = cross.rectTransform.rect;
        width = rect.width;
        height = rect.height;

    }
    public void setImageSize()
    {
        if (width < 100 && height < 100)
        {
            cross.rectTransform.sizeDelta = new Vector2(width += 10f, height += 10f);

        }

    }
    public void resetImageSize()
    {
        while (height > 50f && width > 50f)
        {
            cross.rectTransform.sizeDelta = new Vector2(width -= 10f, height -= 10f);
        }
    }
    private void ShowXHitEffectAtPosition(Vector3 worldPosition)
    {
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // Move the X hit effect UI element to the calculated screen position
        xHitEffectUI.gameObject.SetActive(true);
        xHitEffectUI.position = screenPosition;

        // Optionally, you can start a coroutine to hide this effect after some time
        StartCoroutine(HideXHitEffect());
    }
    IEnumerator HideXHitEffect()
    {
        yield return new WaitForSeconds(0.2f); // Duration for which the effect is shown, adjust as needed
        xHitEffectUI.gameObject.SetActive(false);
    }
    //----------------------------------------------------------------

    private void InitializeAnimator()
    {
        animator = GetComponent<Animator>();
    }

    private void HandleAiming()
    {
        // corsshair settings
        if (Sniper)
        {
            cross.gameObject.SetActive(false);
        }
        else
        {
            cross.gameObject.SetActive(true);
        }

        // bool-setting for the animator
        if (Input.GetKey(KeyCode.Mouse1))
        {
            animator.SetBool("aiming", true);
            cross.gameObject.SetActive(false);  // setting up crosshair object false

        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            animator.SetBool("aiming", false);
            cross.gameObject.SetActive(true);
        }
    }


    private void HandleShooting() // handling the shooting function for all the weapons
    {
        if (AK47 && !isChanging)
        {
            // Updated logic: Can't shoot if (sprinting and moving) or reloading, unless aiming.


            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextTimeToShoot && !isEmpty && !canNotShoot)
            {
                nextTimeToShoot = Time.time + 1f / fireRate;
                Shoot();
                animator.SetBool("shooting", true);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0) || canNotShoot || isEmpty)
            {
                animator.SetBool("shooting", false);
                combined.Dayandir(); // stop camera shake/recoil effect
                resetImageSize();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && isEmpty && !isReloading)  // sound
            {
                AudioSource.PlayClipAtPoint(emptyhot, gameObject.transform.position, 1f);
                //audioSource.PlayOneShot(emptyhot, 1f);
            }
        }           // AK
        else if (Sniper && !isChanging)
        {
            // Updated logic: Can't shoot if (sprinting and moving) or reloading, unless aiming.
            //bool canNotShoot = (isSprinting && isMoving || isReloading) && !isAiming;

            if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToShoot && !isEmptySniper && !canNotShoot)
            {
                nextTimeToShoot = Time.time + 1f / fireRateSniper;
                Shoot();
                if (isAiming)
                {
                    StartCoroutine(StopRecoil2());
                }
                if (!isAiming)
                {
                    StartCoroutine(StopRecoil());
                }

                animator.SetBool("shooting", true);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0) || canNotShoot || isEmptySniper)
            {
                animator.SetBool("shooting", false);
                resetImageSize();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) && isEmptySniper && !isReloading)  // sound
            {
                AudioSource.PlayClipAtPoint(emptyhot, gameObject.transform.position, 1f);
                //audioSource.PlayOneShot(emptyhot, 1f);
            }

        }    // SNIPER
        else if (isMakarov && !isChanging)
        {
            // Updated logic: Can't shoot if (sprinting and moving) or reloading, unless aiming.
            //bool canNotShoot = (isSprinting && isMoving || isReloading) && !isAiming;

            if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToShoot && !isEmptyMakarov && !canNotShoot)
            {
                nextTimeToShoot = Time.time + 1f / fireRateMakarov;
                Shoot();
                if (isAiming)
                {
                    StartCoroutine(StopRecoil2());
                }
                if (!isAiming)
                {
                    StartCoroutine(StopRecoil());
                }

                animator.SetBool("shooting", true);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0) || canNotShoot || isEmptyMakarov)
            {
                animator.SetBool("shooting", false);
                //combined.Dayandir();
                if (isAiming)
                {
                    StartCoroutine(StopRecoil2());
                }
                if (!isAiming)
                {
                    StartCoroutine(StopRecoil());
                }
                resetImageSize();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && isEmptyMakarov && !isReloading)
            {
                AudioSource.PlayClipAtPoint(emptyhot, gameObject.transform.position, 1f);  // sound
            }

        } // MAKAROV
    }


    // Shoot function. internally varies from weapon-to-weapon
    private void Shoot()  
    {
        if (AK47 && !Sniper && !isMakarov)
        {
            // Prevent shooting when reloading
            if (isReloading) return;

            GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();
            if (bullet != null && bulletCount > 0)
            {
                bullet.transform.position = weaponHead.transform.position;
                bullet.transform.rotation = weaponHead.transform.rotation;
                bullet.SetActive(true);

                bullet.GetComponent<bullet>().InitializeBullet(launchVelocity);

                GameObject currentMuzzle = Instantiate(muzzle, spawnPoint.transform.position, spawnPoint.transform.rotation);
                currentMuzzle.transform.parent = spawnPoint;





                GameObject currentBulletShell = Instantiate(bulletShell, spawnPoint2.transform.position, spawnPoint2.transform.rotation);
                currentBulletShell.transform.parent = spawnPoint2;




                AudioSource.PlayClipAtPoint(gunSound, gameObject.transform.position, 0.5f);
                //audioSource.PlayOneShot(gunSound, 1f);
                bulletCount--;
                pointLight.gameObject.SetActive(true);
                StartCoroutine(LightBlyat());
                combined.TriggerRecoil();
            }

            ProcessRaycast();

            //animator.SetBool("shooting", true);
            setImageSize();
        }       // AK
        else if (Sniper && !AK47 && !isMakarov)
        {
            // Prevent shooting when reloading
            if (isReloading) return;

            GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();
            if (bullet != null && bulletCountSniper > 0)
            {
                bullet.transform.position = sniperHead.transform.position;
                bullet.transform.rotation = sniperHead.transform.rotation;
                bullet.SetActive(true);

                bullet.GetComponent<bullet>().InitializeBullet(launchVelocity);

                GameObject currentMuzzle = Instantiate(muzzleSniper, sniperSpawnPoint.transform.position, sniperSpawnPoint.transform.rotation);
                currentMuzzle.transform.parent = sniperSpawnPoint;





                GameObject currentBulletShell = Instantiate(bulletShellSniper, sniperSpawnPoint2.transform.position, sniperSpawnPoint2.transform.rotation);
                currentBulletShell.transform.parent = sniperSpawnPoint2;




                AudioSource.PlayClipAtPoint(gunSoundSniper, gameObject.transform.position, 2f);
                //audioSource.PlayOneShot(gunSound, 1f);
                bulletCountSniper--;
                pointLightSniper.gameObject.SetActive(true);
                StartCoroutine(LightBlyatSniper());
                combined.TriggerRecoil();
            }

            ProcessRaycast();

            setImageSize();
        }  // SNIPER
        else if (isMakarov && !Sniper && !AK47)
        {
            // Prevent shooting when reloading
            if (isReloading) return;

            GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();
            if (bullet != null && bulletCountMakarov > 0)
            {
                bullet.transform.position = makarovHead.transform.position;
                bullet.transform.rotation = makarovHead.transform.rotation;
                bullet.SetActive(true);

                bullet.GetComponent<bullet>().InitializeBullet(launchVelocity);

                GameObject currentMuzzle = Instantiate(muzzleMakarov, makarowSpawnPoint.transform.position, makarowSpawnPoint.transform.rotation);
                currentMuzzle.transform.parent = makarowSpawnPoint;





                GameObject currentBulletShell = Instantiate(bulletShellMakarov, makarovSpawnPoint2.transform.position, makarovSpawnPoint2.transform.rotation);
                currentBulletShell.transform.parent = makarovSpawnPoint2;




                AudioSource.PlayClipAtPoint(gunSoundMakarov, gameObject.transform.position, .5f);
                //audioSource.PlayOneShot(gunSound, 1f);
                bulletCountMakarov--;
                pointLightMakarov.gameObject.SetActive(true);
                StartCoroutine(LightBlyatMakarov());
                combined.TriggerRecoil();
            }

            ProcessRaycast();

            setImageSize();
        }  // MAKAROV
    } 


    // Post-Processing Effects functions
    public void SetEffectsActive(bool isActive)
    {
        depthOfField.active = isActive;
        ambientOcclusion.active = isActive;
    }
    public void DisableEffects()
    {
        SetEffectsActive(false);
    }

    // IENumerators to set light false after short time
    IEnumerator LightBlyat()
    {
        yield return new WaitForSeconds(0.1f);
        pointLight.gameObject.SetActive(false);
    }
    IEnumerator LightBlyatSniper()
    {
        yield return new WaitForSeconds(0.1f);
        pointLightSniper.gameObject.SetActive(false);
    }
    IEnumerator LightBlyatMakarov()
    {
        yield return new WaitForSeconds(0.1f);
        pointLightMakarov.gameObject.SetActive(false);
    }


    private void HandleReloading()
    {
        if (AK47)
        {
            if (bulletCount == 0)
            {
                isEmpty = true;
                Debug.Log("bullet finished!!!");
                StartCoroutine(NotStopShootingWhenOne());
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 0.5f);
            }
            if (bulletCount == 30)
            {
                animator.SetBool("reload", false);
                bullet3.color = new Color(255, 255, 255, 1);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCount == 20)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCount == 10)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 1);
            }
        }
        else if (Sniper)
        {
            if (bulletCountSniper == 0)
            {
                isEmptySniper = true;
                Debug.Log("bullet finished!!!");
                StartCoroutine(NotStopShootingWhenOne());
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 0.5f);
            }
            if (bulletCountSniper == 10)
            {
                animator.SetBool("reload", false);
                bullet3.color = new Color(255, 255, 255, 1);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCount == 6)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCount == 2)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 1);
            }
        }
        else if (isMakarov)
        {
            if (bulletCountMakarov == 0)
            {
                animator.SetBool("OutOfBulletMakarov", true);
                isEmptyMakarov = true;
                Debug.Log("bullet finished!!!");
                StartCoroutine(NotStopShootingWhenOne());
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 0.5f);
            }
            if (bulletCountMakarov == 8)
            {
                animator.SetBool("reload", false);
                bullet3.color = new Color(255, 255, 255, 1);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCountMakarov == 4)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 1);
                bullet1.color = new Color(255, 255, 255, 1);
            }
            if (bulletCountMakarov == 2)
            {
                bullet3.color = new Color(255, 255, 255, 0.5f);
                bullet2.color = new Color(255, 255, 255, 0.5f);
                bullet1.color = new Color(255, 255, 255, 1);
            }
        }

        // set reloading bool to true when pressing: R
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            animator.SetBool("shooting", false);
            Reload();
        }
    }

    IEnumerator setOutOfBulletFalse()
    {
        yield return new WaitForSeconds(2f);
        animator.SetBool("OutOfBulletMakarov", false);
    }


    // recoil IENumerators for weapons. varies depending on aiming or not.
    IEnumerator StopRecoil()
    {
        yield return new WaitForSeconds(0.1f);
        combined.Dayandir();
    }
    IEnumerator StopRecoil2()
    {
        yield return new WaitForSeconds(0.2f);
        combined.Dayandir();
    }

    //  not make bullet count directly 0 and let functions to be called.
    IEnumerator NotStopShootingWhenOne()
    {
        yield return new WaitForSeconds(.1f);
        animator.SetBool("shooting", false);
    }

    private void Reload()
    {
        if (AK47)
        {
            if (bulletCount < 30)
            {
                animator.SetBool("empty", false);
                isReloading = true;
                animator.SetBool("reload", true);
                StartCoroutine(reload());
                //audioSource.PlayOneShot(reloadSound, 1f);
                AudioSource.PlayClipAtPoint(reloadSound, gameObject.transform.position, 0.05f);
            }
        }
        else if (Sniper)
        {
            if (bulletCountSniper < 10)
            {
                animator.SetBool("empty", false);
                isReloading = true;
                animator.SetBool("reload", true);
                StartCoroutine(reloadSniper());
                //audioSource.PlayOneShot(reloadSound, 1f);
                AudioSource.PlayClipAtPoint(reloadSoundSniper, gameObject.transform.position, 1f);
            }
        }
        else if (isMakarov)
        {
            if (bulletCountMakarov < 8)
            {
                
                animator.SetBool("empty", false);
                isReloading = true;
                animator.SetBool("reload", true);
                StartCoroutine(reloadMakarov());


                //audioSource.PlayOneShot(reloadSound, 1f);
                AudioSource.PlayClipAtPoint(reloadSoundMakarov, gameObject.transform.position, 1f);
            }
        }

    }

    // Reloading IENumerators
    IEnumerator reload()
    {
        yield return new WaitForSeconds(3.0f);
        bulletCount = 30;
        isEmpty = false;
        isReloading = false;
        animator.SetBool("reload", false);  // Ensure shooting state is reset after reloading
    }
    IEnumerator reloadSniper()
    {
        yield return new WaitForSeconds(5.2f);
        bulletCountSniper = 10;
        isEmptySniper = false;
        isReloading = false;
        animator.SetBool("reload", false);  // Ensure shooting state is reset after reloading
    }
    IEnumerator reloadMakarov()
    {
        yield return new WaitForSeconds(2.2f);
        bulletCountMakarov = 8;
        isEmptyMakarov = false;
        isReloading = false;
        animator.SetBool("OutOfBulletMakarov", false);
        animator.SetBool("reload", false);  // Ensure shooting state is reset after reloading
    }


    private void ProcessRaycast()
    {
        if (AK47)
        {
            RaycastHit hit;
            if (Physics.Raycast(weaponHead.transform.position, weaponHead.transform.forward, out hit, shootRange))
            {
                // ... existing code ...
                Debug.Log(hit.transform.tag);
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impact);
                }

                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

                Destroy(impactGO, 2f);

                // Check if hit an enemy
                if (hit.transform.CompareTag("Enemy")) // Ensure your enemy GameObjects have the tag "Enemy"
                {
                    Enemy enemy = hit.transform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(20); // Decrease health by 20
                        GameObject bloodGo = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.normal));

                        Destroy(bloodGo, 1f);
                        // Convert the hit point to a screen position and show the X hit effect
                        ShowXHitEffectAtPosition(hit.point);
                    }
                }
            }
        }
        else if (Sniper)
        {
            RaycastHit hit;
            if (Physics.Raycast(sniperHead.transform.position, sniperHead.transform.forward, out hit, shootRange))
            {
                // ... existing code ...
                Debug.Log(hit.transform.tag);
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impact);
                }

                GameObject impactGO = Instantiate(impactEffectSniper, hit.point, Quaternion.LookRotation(hit.normal));

                Destroy(impactGO, 2f);

                // Check if hit an enemy
                if (hit.transform.CompareTag("Enemy")) // Ensure your enemy GameObjects have the tag "Enemy"
                {
                    Enemy enemy = hit.transform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(100); // Decrease health by 20
                        GameObject bloodGo = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.normal));

                        Destroy(bloodGo, 1f);
                        // Convert the hit point to a screen position and show the X hit effect
                        ShowXHitEffectAtPosition(hit.point);
                    }
                }
            }
        }
        else if (isMakarov)
        {
            RaycastHit hit;
            if (Physics.Raycast(makarovHead.transform.position, makarovHead.transform.forward, out hit, shootRange))
            {
                // ... existing code ...
                Debug.Log(hit.transform.tag);
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impact);
                }

                GameObject impactGO = Instantiate(impactEffectSniper, hit.point, Quaternion.LookRotation(hit.normal));

                Destroy(impactGO, 2f);

                // Check if hit an enemy
                if (hit.transform.CompareTag("Enemy")) // Ensure your enemy GameObjects have the tag "Enemy"
                {
                    Enemy enemy = hit.transform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(20); // Decrease health by 20
                        GameObject bloodGo = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.normal));

                        Destroy(bloodGo, 1f);
                        // Convert the hit point to a screen position and show the X hit effect
                        ShowXHitEffectAtPosition(hit.point);
                    }
                }
            }
        }
    }  // PROCESS RAYCAST
    
}