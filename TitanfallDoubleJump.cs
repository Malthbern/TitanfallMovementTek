using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TitanfallDoubleJump : UdonSharpBehaviour
{
    //Values For DJump
    private int JumpCount = 0;
    private float JumpTimeWaited = 0;
    private bool IDetection = false;
    private bool trueground = true;

    public int JumpsAllowed = 2;
    public float JumpPower = 6;
    public float JumpWaitTime = 0.5f;

    //Values for Hop
    private Vector3 SavedV3;
    private Vector3 playerhead1;
    private Vector3 playerhead2;
    private Vector3 playerroot1;
    private Vector3 playerroot2;
    private float playerheight2;
    private float playerheightf;
    private float playercrouchf;
    private float SlideTimeWaited = 0;
    private float UpdateHeight = 0;

    public float SlideVSaveTime = 1.5f;

    //values for slide
    private Vector3 CurrentV;
    private float CVX;
    private float CVZ;
    private bool InSlide = false;

    public float SlideTime = 3.5f;
    public float SlideBeginSpeed = 1.2f;
    public float SlideMultiplier = 1.5f;

    void Start()
    {
        //credits
        Debug.Log("TF Movement Tek started");
        Debug.Log("Writen by Malthbern#0233");
        Debug.Log("https://github.com/Malthbern/TitanfallMovementTek");
    }
    void Update()
    {
        //a dumb way to allow for scripting custom events 
        //in U# in a way U# allows for... i hate this
        UpdateDJump();
        UpdateSlideHop();
    }
    void LateUpdate()
    {
        LateUpdateDJump();
        LateUpdateSlideHop();
    }
    //beginig of all jump logic
    void UpdateDJump()
    {
        //this timer fixes the "Super Jump" bug to an extent
        if (IDetection == true)
        {
            if (JumpTimeWaited < JumpWaitTime)
            {
                JumpTimeWaited += Time.deltaTime;
            }
            else
            {
                Debug.Log("jump wait finished");
                IDetection = false;
            }
        }

        //Simple input check
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            IDetection = true;

            if (JumpCount == 0)
            {
                //basically ignore the first jump
                //vrchat handels it
                Debug.Log("first jump loged");
                JumpCount = 1;
            }
            else
            {
                //check if timer is complete
                if (JumpTimeWaited >= JumpWaitTime)
                {
                    Debug.Log("Djump start");
                    JumpTimeWaited = 0;
                    IDetection = false;
                    DoJumpLogic();
                }
            }
        }
    }
    private void DoJumpLogic()
    {
        Debug.Log("start of DJMP");

        if (JumpCount < JumpsAllowed)
        {
            Debug.Log("appling DJMP");
            //sets player's V3 to their V3 + what's set in Jumppower's float value
            //also incriments the jump count to avoid infinate jumps
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + new Vector3(0, JumpPower, 0));
            JumpCount += 1;
        }
    }
    void GeneralReset()
    {
        //this is to reset for wallrunning
        JumpCount = 0;
        JumpTimeWaited = 0;
        Debug.Log("Jump reset by outside script");
    }
    void LateUpdateDJump()
    {
        //this looks dumb because VRC is dumb and dosen't instantly change "isplayergrounded"
        //to false so i need to make a frame buffer to fix this
        if (trueground == false)
        {
            if (Networking.LocalPlayer.IsPlayerGrounded() == true)
            {
                Debug.Log("jump ended");
                JumpCount = 0;
                trueground = true;
                IDetection = false;
            }
        }
        else
        {
            trueground = Networking.LocalPlayer.IsPlayerGrounded();
        }
    }
    //begining of all slidehop logic
    void UpdateSlideHop()
    {


        //get player height this frame
        playerhead1 = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
        playerroot1 = Networking.LocalPlayer.GetPosition();

        playerheightf = playerhead1.y - playerroot1.y;

        //hop logic starts
        if (trueground == false)
        {
            //save previous v3 and set y to 0
            SavedV3 = Networking.LocalPlayer.GetVelocity();
            SavedV3.y = 0;
            Debug.Log("velocity saved");
        }
        else
        {
            if (UpdateHeight < 15.0f)
            {
                UpdateHeight += Time.deltaTime;
            }
            else
            {
                if (trueground == true && InSlide == false)
                {
                    //calibrateing crouch level
                     playerhead2 = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
                     playerroot2 = Networking.LocalPlayer.GetPosition();

                    playerheight2 = playerhead2.y - playerroot2.y;
                    playercrouchf = playerheight2 * 0.68f;
                    UpdateHeight = 0.0f;

                    Debug.Log("Heigh Calibrated, Player Height: " + playerheight2 + ", Crouch Height: " + playercrouchf);
                }

            }
            //buffer timer to actually allow slide hoping for limited time
            if (SlideTimeWaited < SlideVSaveTime && playerheightf < playercrouchf)
            {
                SlideTimeWaited += Time.deltaTime;
            }
            else
            {
                SavedV3 = new Vector3(0, 0, 0);
                Debug.Log("velocity boost reset");
            }
        }

        //slide logic starts

        //get velocity
        CurrentV = Networking.LocalPlayer.GetVelocity();

        CVX = CurrentV.x;
        CVZ = CurrentV.z;

        Debug.Log("Absoulute value of V is" + Mathf.Abs(CVX) + "X " + Mathf.Abs(CVZ) + "Z");
    }
    void LateUpdateSlideHop()
    {
        //logic for appling boost for a slide hop
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) && playerheightf <= playercrouchf && trueground == true)
        {
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + SavedV3);
            Debug.Log("hop boost applied");
        }

        //logic for sliding
        if (playerheightf <= playercrouchf && trueground == true && Mathf.Abs(CVX) >= SlideBeginSpeed || Mathf.Abs(CVZ) >= SlideBeginSpeed)
        {
            //not quite working yet, velocity needs to be clamped
            InSlide = true;
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + new Vector3(CVX * SlideMultiplier,0,CVZ * SlideMultiplier));
        }
        else
        {
            InSlide = false;
        }
    }
}