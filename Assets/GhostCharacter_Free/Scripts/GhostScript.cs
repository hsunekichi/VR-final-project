using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        // Cache hash values
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");
        // dissolve
        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1;
        private bool DissolveFlg = false;
        private const int maxHP = 3;
        [SerializeField] private int HP = maxHP;
        //private TextElement HP_text;

        [SerializeField] string TargetTag;
        private GameObject Target => GameObject.FindGameObjectWithTag(TargetTag); // Find the target by tag

        static private float lastAudioPitch = 1.0f; // last audio pitch for the hit audio

        // Hit audio
        [System.Serializable]
        public class AudioSettings
        {
            public AudioClip hitAudio;
            public AudioClip spawnAudio;
            public float maxPitchDelta;
            public float minPitch;
            public float maxPitch;
        }
        [SerializeField] AudioSettings audioSettings;

        // moving speed
        [System.Serializable]
        public class MoveSettings
        {
            public float SpeedToTarget = 4;
            public float AttackDistance = 4;
            public bool FreezeY = false; // freeze Y axis movement

            public bool lateralMoveX;
            public bool lateralMoveY;
            public AnimationCurve lateralSpeedX;
            public AnimationCurve lateralSpeedY;

            public float moveNoiseStrength = 0.1f; // Noise strength for movement
        }
        [SerializeField] public MoveSettings moveSettings;

        bool AttackProgrammed = false;
        bool AttackDone = false;
        bool CanAttack => !PlayerStatus.ContainsValue(true);
        bool CanMove => !PlayerStatus[Surprised];
        float born_time = 0.0f; // time when the object was born

        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();
            //HP_text = GameObject.Find("Canvas/HP").GetComponent<TextElement>();
            //HP_text.text = "HP " + HP.ToString();

            born_time = Time.time; // record the time when the object was born

            // Play spawn audio
            AudioSource audioSource = new GameObject("TempAudio").AddComponent<AudioSource>();
            audioSource.clip = audioSettings.spawnAudio;
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.Play();
        }

        void AIUpdate()
        {
            // AI logic for moving towards the target
            if (Target != null && CanMove)
            {
                Vector3 toTarget = Target.transform.position - transform.position;

                // Too far, move towards the target
                if (Vector3.Magnitude(toTarget) >= moveSettings.AttackDistance)
                {
                    // Move to the target
                    float curveLengthX = moveSettings.lateralSpeedX.keys[moveSettings.lateralSpeedX.length - 1].time;
                    float curveLengthY = moveSettings.lateralSpeedY.keys[moveSettings.lateralSpeedY.length - 1].time;
                    float timeInCurveX = (Time.time - born_time) % curveLengthX;
                    float timeInCurveY = (Time.time - born_time) % curveLengthY;

                    Vector3 lateralMove = moveSettings.lateralSpeedX.Evaluate(timeInCurveX) * Vector3.right * (moveSettings.lateralMoveX ? 1 : 0) +
                                          moveSettings.lateralSpeedY.Evaluate(timeInCurveY) * Vector3.up * (moveSettings.lateralMoveY ? 1 : 0);

                    Vector3 moveNoise = Random.insideUnitSphere * moveSettings.moveNoiseStrength;
                    Vector3 toTargetDir = toTarget.normalized;
                    MoveDirection = toTargetDir * moveSettings.SpeedToTarget + lateralMove + moveNoise;

                    if (moveSettings.FreezeY)
                        MoveDirection.y = 0; // Keep the movement on the horizontal plane

                    MOVE_Velocity(MoveDirection, Quaternion.LookRotation(toTargetDir).eulerAngles);
                }
                else // Too close, attack
                {
                    // Stop moving
                    MoveDirection = Vector3.zero;
                    //MOVE_Velocity(MoveDirection, Vector3.zero);

                    if (CanAttack && !AttackProgrammed) // Close enough and can attack
                    {
                        Anim.CrossFade(AttackState, 0.1f, 0, 0); // Start attack animation
                        AttackProgrammed = true;                 // Prevent attacking again
                    }
                }
            }
        }

        void Update()
        {
            AIUpdate();
            STATUS();

            if (PlayerStatus.ContainsValue(true))
            {
                int status_name = 0;
                foreach (var i in PlayerStatus)
                {
                    if (i.Value == true)
                    {
                        status_name = i.Key;
                        break;
                    }
                }
                if (status_name == Dissolve)
                {
                    PlayerDissolve();
                }
                else if (status_name == Attack)
                {
                    //PlayerAttack();
                }
                else if (status_name == Surprised)
                {
                    // nothing method
                }
            }

            // Dissolve
            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            // processing at respawn
            else if (HP == maxHP && DissolveFlg)
            {
                DissolveFlg = false;
            }

            // We attack once, and kill ourselves after the animation is done
            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
                AttackDone = true;
            else if (AttackDone)    // Attack done and not in attack state, we kill ourselves
                HP = 0;
        }

        //---------------------------------------------------------------------
        // character status
        //---------------------------------------------------------------------
        private const int Dissolve = 1;
        private const int Attack = 2;
        private const int Surprised = 3;
        private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
    {
        {Dissolve, false },
        {Attack, false },
        {Surprised, false },
    };
        //------------------------------
        private void STATUS()
        {
            // during dissolve
            if (DissolveFlg && HP <= 0)
            {
                PlayerStatus[Dissolve] = true;
            }
            else if (!DissolveFlg)
            {
                PlayerStatus[Dissolve] = false;
            }
            // during attacking
            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                PlayerStatus[Attack] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
            {
                PlayerStatus[Attack] = false;
            }
            // during damaging
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
            {
                PlayerStatus[Surprised] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState)
            {
                if (PlayerStatus[Surprised])
                    HP--; // We lose HP after the surprise animation finishes

                PlayerStatus[Surprised] = false;
            }
        }
        // dissolve shading
        private void PlayerDissolve()
        {
            Dissolve_value -= Time.deltaTime;
            for (int i = 0; i < MeshR.Length; i++)
            {
                MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
            }
            if (Dissolve_value <= 0)
            {
                Destroy(gameObject);
                Ctrl.enabled = false;
            }
        }


        //---------------------------------------------------------------------
        // value for moving
        //---------------------------------------------------------------------
        private void MOVE_Velocity(Vector3 velocity, Vector3 rot)
        {
            MoveDirection = new Vector3(velocity.x, MoveDirection.y, velocity.z);
            if (Ctrl.enabled)
            {
                Ctrl.Move(MoveDirection * Time.deltaTime);
            }
            MoveDirection.x = 0;
            MoveDirection.z = 0;
            this.transform.rotation = Quaternion.Euler(rot);
        }

        //---------------------------------------------------------------------
        // damage
        //---------------------------------------------------------------------
        public void Damage()
        {
            Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
            //HP--; We will lose the HP after the surprise animation finishes
            // Play audio

            float deltaSign = Mathf.Sign(Random.Range(lastAudioPitch <= 1.0f ? -1.0f : -1.5f, lastAudioPitch >= 1.0f ? 1.5f : 2f));
            float newPitch = lastAudioPitch + deltaSign * Random.Range(0, audioSettings.maxPitchDelta);
            newPitch = Mathf.Clamp(newPitch, audioSettings.minPitch, audioSettings.maxPitch);

            AudioSource audioSource = new GameObject("TempAudio").AddComponent<AudioSource>();
            audioSource.clip = audioSettings.hitAudio;
            audioSource.pitch = newPitch;
            lastAudioPitch = audioSource.pitch;
            audioSource.Play();
            Destroy(audioSource.gameObject, audioSettings.hitAudio.length / audioSource.pitch);
        }

        //---------------------------------------------------------------------
        // respawn
        //---------------------------------------------------------------------
        private void Respawn()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // player HP
                HP = maxHP;

                Ctrl.enabled = false;
                this.transform.position = Vector3.zero; // player position
                this.transform.rotation = Quaternion.Euler(Vector3.zero); // player facing
                Ctrl.enabled = true;

                // reset Dissolve
                Dissolve_value = 1;
                for (int i = 0; i < MeshR.Length; i++)
                {
                    MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
                }
                // reset animation
                Anim.CrossFade(IdleState, 0.1f, 0, 0);
            }
        }
    }
}