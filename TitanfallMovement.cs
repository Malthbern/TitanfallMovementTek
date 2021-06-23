using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TitanfallMovement : UdonSharpBehaviour
{
    //Values For DJump
    private int JumpCount = 0;
    private float JumpTimeWaited = 0;
    private bool IDetection = false;
    private bool trueground = true;

    public int JumpsAllowed = 2;
    public float JumpPower = 6;
    public float JumpWaitTime = 0.5f;


    //vars for wallrun

    private bool isWallruning = false;

    public string WallDirection;
    public float HangPower = 0.3f;


    //currently unused
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
    private float CVY;
    private bool InSlide = false;
    private float slidespeedx;
    private float slidespeedz;

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
        //common update stuff

        //get velocity
        CurrentV = Networking.LocalPlayer.GetVelocity();

        CVX = CurrentV.x;
        CVZ = CurrentV.z;
        CVY = CurrentV.y;

        //a dumb way to allow for scripting custom events in the way i needed
        //in U# in a way U# allows for... i hate this
        UpdateDJump();
        WallrunUpdate();
        //UpdateSlideHop();
    }
    void LateUpdate()
    {
        LateUpdateDJump();
        //LateUpdateSlideHop();
    }
    //beginig of all jump logic
    private void UpdateDJump()
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
    private void GeneralReset()
    {
        //this is to reset for wallrunning
        JumpCount = 0;
        JumpTimeWaited = 0;
        Debug.Log("Jump reset by outside script");
    }
    private void LateUpdateDJump()
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


    private void WallrunUpdate()
    {
        //initate wallrun by detecting wall direction and appling a "wall stick and y = 0 force"
        if (Networking.LocalPlayer.IsPlayerGrounded() == false && isWallruning == false)
        {
            if (WallDirection == "Back")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CVX, 2.2f, CVZ));
                isWallruning = true;
            }
            else if (WallDirection == "Right")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CVX, 2.2f, CVZ));
                isWallruning = true;
            }
            else if (WallDirection == "Left")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CVX, CVY * 2.2f, CVZ));
                isWallruning = true;
            }
            else if (WallDirection == "Front")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CVX, CVY * 2.2f, CVZ));
                isWallruning = true;
            }
            else if (WallDirection == "NW")
            {
                Networking.LocalPlayer.SetGravityStrength(1f);
                isWallruning = false;
            }
        }
        else
        {
            if (Networking.LocalPlayer.IsPlayerGrounded() == true)
            {
                WallDirection = null;
                isWallruning = false;
                Networking.LocalPlayer.SetGravityStrength(1f);
            }
        }
           
    }


    //this has been commented out in the base update call
    //begining of all slidehop logic
    private void UpdateSlideHop()
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
            // Update Heught every 15 seconds
            //this is done because VRC does not expose a "onavatarload" event
            if (UpdateHeight < 15.0f)
            {
                UpdateHeight += Time.deltaTime;
            }
            else
            {
                //only update height if player is not sliding
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
                //Debug.Log("velocity boost reset");
            }
        }

        //slide logic starts
        //slide speed math
        if (InSlide == false)
        {
            slidespeedx = (CVX * SlideMultiplier);
            slidespeedz = (CVZ * SlideMultiplier);
        }
    }
    private void LateUpdateSlideHop()
    {
        Debug.Log("player height is " + playerheightf + " player crough level is " + playercrouchf);
        //logic for appling boost for a slide hop
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) && playerheightf < playercrouchf && trueground == true)
        {
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + SavedV3);
            Debug.Log("hop boost applied");
        }

        //logic for sliding
        if (playerheightf < playercrouchf && trueground == true && Mathf.Abs(CVX) >= SlideBeginSpeed || Mathf.Abs(CVZ) >= SlideBeginSpeed)
        {
            Networking.LocalPlayer.SetVelocity(new Vector3(slidespeedx, CVY, slidespeedz));
            InSlide = true;
        }
        else
        {
            InSlide = false;
        }
    }
}